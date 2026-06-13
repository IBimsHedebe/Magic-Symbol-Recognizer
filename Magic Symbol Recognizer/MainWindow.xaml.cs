using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Magic_Symbol_Recognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<Point>> _perfectPatterns = new Dictionary<string, List<Point>>();
        private System.Diagnostics.Stopwatch _magicTimer = new System.Diagnostics.Stopwatch();
        private Controls.MainMenuControl _mainMenuCtrl;
        private Controls.SpellcastingControl _spellcastingCtrl;
        private Controls.BattleArenaControl _battleArenaCtrl;
        private Controls.LibraryControl _libraryCtrl;

        // Battle state
        private bool _isBattleActive = false;
        private DispatcherTimer _botTimer;
        private int _botHP = 500;
        private int _playerHP = 500;
        private string _lastPlayerElement = string.Empty;
        private string _currentBotElement = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // Wire up control events by resolving them from XAML names
            _mainMenuCtrl = this.FindName("MainMenuControl") as Controls.MainMenuControl;
            _battleArenaCtrl = this.FindName("BattleArenaControl") as Controls.BattleArenaControl;
            _spellcastingCtrl = this.FindName("SpellcastingControl") as Controls.SpellcastingControl;
            _libraryCtrl = this.FindName("LibraryControl") as Controls.LibraryControl;

            if (_mainMenuCtrl != null)
            {
                _mainMenuCtrl.OpenSpellcastingClicked += (s, e) => OpenSpellcasting_Click(s, e);
                _mainMenuCtrl.OpenBattleArenaClicked += (s, e) => OpenBattleArena_Click(s, e);
                _mainMenuCtrl.OpenLibraryClicked += (s, e) => OpenLibrary_Click(s, e);
                _mainMenuCtrl.CloseAppClicked += (s, e) => CloseApp_Click(s, e);
            }

            if (_battleArenaCtrl != null)
            {
                _battleArenaCtrl.BackClicked += (s, e) => BackToMenu_Click(s, e);
                _battleArenaCtrl.CastClicked += (s, e) => CastBattleSpell_Click(s, e);
                _battleArenaCtrl.ClearClicked += (s, e) => ClearCanvas_Click(s, e);

                _battleArenaCtrl.InkCanvas.PreviewMouseDown += MagicCanvas_PreviewMouseDown;
                _battleArenaCtrl.InkCanvas.SizeChanged += MagicCanvas_SizeChanged;
            }

            if (_spellcastingCtrl != null)
            {
                _spellcastingCtrl.BackClicked += (s, e) => BackToMenu_Click(s, e);
                _spellcastingCtrl.SavePatternClicked += (s, e) => SavePattern_Click(s, e);
                _spellcastingCtrl.CastClicked += (s, e) => CastSpell_Click(s, e);
                _spellcastingCtrl.ClearClicked += (s, e) => ClearCanvas_Click(s, e);

                _spellcastingCtrl.InkCanvas.PreviewMouseDown += MagicCanvas_PreviewMouseDown;
                _spellcastingCtrl.InkCanvas.SizeChanged += MagicCanvas_SizeChanged;
            }

            if (_libraryCtrl != null)
            {
                _libraryCtrl.BackClicked += (s, e) => BackToMenu_Click(s, e);
                _libraryCtrl.SelectionChanged += (s, e) => DrawPerfectPatternInLibrary();
                _libraryCtrl.Preview.SizeChanged += LibraryPreviewCanvas_SizeChanged;
            }

            LoadPatternsFromFiles();
        }

        // Start timer at first stroke
        private void MagicCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_magicTimer.IsRunning && _spellcastingCtrl != null && _spellcastingCtrl.InkCanvas.Strokes.Count == 0)
            {
                _magicTimer.Restart();
            }
        }

        private void OpenSpellcasting_Click(object sender, RoutedEventArgs e)
        {
            if (_mainMenuCtrl != null) _mainMenuCtrl.Visibility = Visibility.Collapsed;
            if (_spellcastingCtrl != null) _spellcastingCtrl.Visibility = Visibility.Visible;
            if (_spellcastingCtrl != null) _spellcastingCtrl.InkCanvas.Strokes.Clear();
            _magicTimer.Reset();
        }

        private void OpenLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (_mainMenuCtrl != null) _mainMenuCtrl.Visibility = Visibility.Collapsed;
            if (_libraryCtrl != null) _libraryCtrl.Visibility = Visibility.Visible;
            if (_libraryCtrl != null)
            {
                _libraryCtrl.SpellList.ItemsSource = null;
                _libraryCtrl.SpellList.ItemsSource = _perfectPatterns.Keys;
                if (_libraryCtrl.SpellList.Items.Count > 0) _libraryCtrl.SpellList.SelectedIndex = 0;
            }
        }

        private void OpenBattleArena_Click(object sender, RoutedEventArgs e)
        {
            if (_mainMenuCtrl != null) _mainMenuCtrl.Visibility = Visibility.Collapsed;
            if (_battleArenaCtrl != null) _battleArenaCtrl.Visibility = Visibility.Visible;
            if (_battleArenaCtrl != null)
            {
                _playerHP = 500;
                _botHP = 500;
                _isBattleActive = true;
                _battleArenaCtrl.UpdatePlayerHP(_playerHP);
                _battleArenaCtrl.UpdateBotHP(_botHP);
                _battleArenaCtrl.UpdateBotStatus("Bereit für den Kampf!");
                UpdateHPUI();
                PlanNextBotAttack();
            }
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl != null) _spellcastingCtrl.Visibility = Visibility.Collapsed;
            if (_battleArenaCtrl != null) _battleArenaCtrl.Visibility = Visibility.Collapsed;
            if (_libraryCtrl != null) _libraryCtrl.Visibility = Visibility.Collapsed;
            if (_mainMenuCtrl != null) _mainMenuCtrl.Visibility = Visibility.Visible;
            _magicTimer.Reset();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LstSpells_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawPerfectPatternInLibrary();
        }

        private void LibraryPreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Keep preview square
            _libraryCtrl.Preview.SizeChanged -= LibraryPreviewCanvas_SizeChanged;
            FrameworkElement parent = _libraryCtrl.Preview.Parent as FrameworkElement;
            if (parent != null)
            {
                double min = Math.Min(parent.ActualWidth, parent.ActualHeight);
                if (min > 0)
                {
                    _libraryCtrl.Preview.Width = min;
                    _libraryCtrl.Preview.Height = min;
                }
            }
            _libraryCtrl.Preview.SizeChanged += LibraryPreviewCanvas_SizeChanged;
            DrawPerfectPatternInLibrary();
        }

        private void DrawPerfectPatternInLibrary()
        {
            _libraryCtrl.Preview.Children.Clear();
            if (_libraryCtrl.SpellList.SelectedItem == null) return;
            string selected = _libraryCtrl.SpellList.SelectedItem.ToString();
            _libraryCtrl.TitleText.Text = $"Rune: {selected}";
            if (!_perfectPatterns.TryGetValue(selected, out var points)) return;
            if (points.Count < 2) return;

            double w = _libraryCtrl.Preview.ActualWidth;
            double h = _libraryCtrl.Preview.ActualHeight;
            if (w <= 0 || h <= 0) return;

            Polyline poly = new Polyline
            {
                Stroke = Brushes.Gold,
                StrokeThickness = 4,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            foreach (var p in points)
            {
                poly.Points.Add(new Point(p.X * w, p.Y * h));
            }
            _libraryCtrl.Preview.Children.Add(poly);
        }

        private void MagicCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _spellcastingCtrl.InkCanvas.SizeChanged -= MagicCanvas_SizeChanged;
            FrameworkElement parent = _spellcastingCtrl.InkCanvas.Parent as FrameworkElement;
            if (parent != null)
            {
                double min = Math.Min(parent.ActualWidth, parent.ActualHeight);
                if (min > 0)
                {
                    _spellcastingCtrl.InkCanvas.Width = min - 20; // leave margin
                    _spellcastingCtrl.InkCanvas.Height = min - 20;
                }
            }
            _spellcastingCtrl.InkCanvas.SizeChanged += MagicCanvas_SizeChanged;
        }

        private void LoadPatternsFromFiles()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var names = asm.GetManifestResourceNames();
                foreach (var rn in names)
                {
                    if (rn.Contains(".ZauberMuster.") && rn.EndsWith(".json"))
                    {
                        using var stream = asm.GetManifestResourceStream(rn);
                        if (stream == null) continue;
                        using var reader = new StreamReader(stream);
                        string json = reader.ReadToEnd();
                        var pts = JsonSerializer.Deserialize<List<Point>>(json);
                        var parts = rn.Split('.');
                        var name = parts[parts.Length - 2];
                        _perfectPatterns[name] = pts ?? new List<Point>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Vorlagen: {ex.Message}", "Ladefehler");
            }
        }

        private (string Label, int Score) ProcessSpellDrawing(System.Windows.Controls.InkCanvas canvas, double secondsTaken)
        {
            // 1. Bild für die KI rendern (aus dem übergebenen Canvas)
            string tempImagePath = Path.Combine(Path.GetTempPath(), "temp_processed_spell.jpg");
            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;

            if (width <= 0 || height <= 0) return ("Unerkennbar", 0);

            // Render normalized image (white background, scaled) to improve ML consistency
            SaveCanvasAsNormalizedJpg(tempImagePath, canvas, 224, 224);

            // 2. KI Klassifizierung aufrufen
            var input = new MLModel1.ModelInput() { ImageSource = File.ReadAllBytes(tempImagePath) };
            var scores = MLModel1.PredictAllLabels(input);
            var best = scores.FirstOrDefault();
            string recognizedLabel = best.Equals(default(KeyValuePair<string, float>)) ? "Unbekannt" : best.Key;

            // Debug: Ausgabe aller Scores, Pfad und bestes Label
            System.Diagnostics.Debug.WriteLine($"ProcessSpellDrawing: tempImage={tempImagePath}");
            foreach (var kv in scores)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessSpellDrawing: Label={kv.Key} Score={kv.Value:F4}");
            }
            System.Diagnostics.Debug.WriteLine($"ProcessSpellDrawing: Best={recognizedLabel} ({best.Value:F4})");

            // 3. Punkte für mathematischen Resampling-Abgleich auslesen
            List<Point> playerPoints = new List<Point>();
            foreach (var stroke in canvas.Strokes)
                foreach (var p in stroke.StylusPoints)
                    playerPoints.Add(new Point(p.X, p.Y));

            if (playerPoints.Count < 5) return ("Unerkennbar", 0);
            var nDrawn = NormalizePoints(playerPoints);

            double formScore = 0;
            if (_perfectPatterns.TryGetValue(recognizedLabel, out var pattern) && pattern.Count > 0)
            {
                double total = 0;
                int n = pattern.Count;
                for (int i = 0; i < n; i++)
                {
                    double relativePosition = (double)i / (n - 1);
                    int playerIndex = (int)Math.Round(relativePosition * (nDrawn.Count - 1));
                    total += Math.Sqrt(Math.Pow(nDrawn[playerIndex].X - pattern[i].X, 2) + Math.Pow(nDrawn[playerIndex].Y - pattern[i].Y, 2));
                }
                formScore = 100.0 - ((total / n) * 75.0);
            }

            // 4. Zeit-Abzug berechnen
            double timeScore = 100.0;
            if (secondsTaken > 4.0)
            {
                timeScore -= (secondsTaken - 4.0) * 5;
                if (timeScore < 0) timeScore = 0;
            }

            int finalScore = (int)((formScore * 0.9) + (timeScore * 0.1));
            finalScore = Math.Clamp(finalScore, 0, 100);

            // Gibt den erkannten Namen und den errechneten Score als Paket zurück
            return (recognizedLabel, finalScore);
        }



        private void DrawPracticeGhostLine(string spellName, InkCanvas canvas)
        {
            // 1. Prüfen, ob für diesen erkannten Zauber überhaupt ein perfektes Muster geladen ist
            if (!_perfectPatterns.TryGetValue(spellName, out var pattern) || pattern.Count < 2) return;

            // 2. Die aktuelle quadratische Größe des Malfelds auslesen
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            // 3. Eine WPF-Punktsammlung für den neuen Strich erstellen
            StylusPointCollection stylusPoints = new StylusPointCollection();
            foreach (var p in pattern)
            {
                // Hochrechnen der relativen Koordinaten (0..1) auf die echten Canvas-Pixel
                double pixelX = p.X * width;
                double pixelY = p.Y * height;
                stylusPoints.Add(new StylusPoint(pixelX, pixelY));
            }

            // 4. Den magischen "Geister-Look" definieren (Rot, halbtransparent und gestrichelt)
            DrawingAttributes ghostAttributes = new DrawingAttributes()
            {
                Color = Colors.Crimson,
                Width = 4,
                Height = 4,
                IsHighlighter = true, // Macht die Linie leicht transparent/leuchtend
            };

            // 5. Einen neuen WPF-Pinselstrich erzeugen und dem Malfeld hinzufügen
            Stroke ghostStroke = new Stroke(stylusPoints, ghostAttributes);
            canvas.Strokes.Add(ghostStroke);
        }

        private void UpdateHPUI()
        {
            if (_battleArenaCtrl != null)
            {
                _battleArenaCtrl.UpdatePlayerHP(_playerHP);
                _battleArenaCtrl.UpdateBotHP(_botHP);
            }
        }

        private void PlanNextBotAttack()
        {
            // Minimaler Bot-Angriffsplan: Timer startet nach kurzer Verzögerung
            _botTimer?.Stop();
            _botTimer = new DispatcherTimer();
            _botTimer.Interval = TimeSpan.FromSeconds(2.0);
            _botTimer.Tick += (s, e) =>
            {
                _botTimer.Stop();
                // einfacher Bot-Angriff: zufälliges Element aus Muster
                var rnd = new Random();
                var keys = _perfectPatterns.Keys.ToList();
                if (keys.Count == 0) return;
                _currentBotElement = keys[rnd.Next(keys.Count)];
                // Schaden berechnen
                int botDamage = rnd.Next(20, 60);
                _playerHP -= botDamage;
                if (_playerHP < 0) _playerHP = 0;
                UpdateHPUI();
                _battleArenaCtrl?.UpdateBotStatus($"Bot wirkt {_currentBotElement} und trifft dich ({botDamage} DMG)!");
                if (_playerHP <= 0) EndBattle(false);
            };
            _botTimer.Start();
        }

        private void EndBattle(bool playerWon)
        {
            _isBattleActive = false;
            _botTimer?.Stop();
            if (_battleArenaCtrl != null)
            {
                _battleArenaCtrl.UpdateBotStatus(playerWon ? "Bot besiegt!" : "Du wurdest besiegt!");
            }
            // Rückkehr zum Hauptmenü
            BackToMenu_Click(this, new RoutedEventArgs());
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl != null) _spellcastingCtrl.InkCanvas.Strokes.Clear();
            if (_battleArenaCtrl != null) _battleArenaCtrl.InkCanvas.Strokes.Clear();
        
            _magicTimer.Reset();
        }

        private void SavePattern_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl.InkCanvas.Strokes.Count == 0)
            {
                MessageBox.Show("Bitte zeichnen Sie zuerst ein Muster auf das Malfeld!", "Malfeld leer");
                return;
            }

            var tb = _spellcastingCtrl.SpellNameTextBox;
            if (tb == null)
            {
                MessageBox.Show("Textfeld für Zaubernamen nicht gefunden.", "Fehler");
                return;
            }

            string spellName = tb.Text.Trim();
            if (string.IsNullOrWhiteSpace(spellName))
            {
                MessageBox.Show("Bitte geben Sie einen Namen für den Zauber ein!", "Name fehlt");
                return;
            }

            List<Point> playerPoints = new List<Point>();
            foreach (var stroke in _spellcastingCtrl.InkCanvas.Strokes)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    playerPoints.Add(new Point(point.X, point.Y));
                }
            }

            var normalized = NormalizePoints(playerPoints);
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZauberMuster");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var opts = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(normalized, opts);
                string fp = Path.Combine(folder, $"{spellName}.json");
                File.WriteAllText(fp, json);
                _perfectPatterns[spellName] = normalized;
                MessageBox.Show($"Muster gespeichert: {fp}", "Erfolg");
                if (_spellcastingCtrl != null) _spellcastingCtrl.InkCanvas.Strokes.Clear();
                tb.Clear();
                _magicTimer.Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler");
            }
        }

        private void CastSpell_Click(object sender, RoutedEventArgs e)
        {
            var canvas = _spellcastingCtrl?.InkCanvas;
            if (canvas == null || canvas.Strokes.Count == 0) return;

            _magicTimer.Stop();
            double secondsTaken = _magicTimer.Elapsed.TotalSeconds;

            var spellResult = ProcessSpellDrawing(canvas, secondsTaken);

            // Style the player's strokes slightly transparent / darker blue
            foreach (var stroke in canvas.Strokes)
            {
                stroke.DrawingAttributes.Color = Color.FromArgb(100, 20, 50, 100);
                stroke.DrawingAttributes.Width = 5;
                stroke.DrawingAttributes.Height = 5;
            }

            if (spellResult.Score < 40)
            {
                MessageBox.Show($"Unerkennbare Magie!\nScore: {spellResult.Score}% (Unter 40%)\n\nBlau = Dein Versuch\nRot = Die perfekte Form", "Übung fehlgeschlagen");
                DrawPracticeGhostLine(spellResult.Label, canvas);
            }
            else
            {
                MessageBox.Show($"Erfolgreich geübt!\nZauber: {spellResult.Label}\nScore: {spellResult.Score}%", "Übung erfolgreich");
                if (spellResult.Score < 70)
                {
                    DrawPracticeGhostLine(spellResult.Label, canvas);
                }
            }

            _magicTimer.Reset();
        }

        private void CastBattleSpell_Click(object sender, RoutedEventArgs e)
        {
            // Weiterleitung an die vorhandene interne Implementierung (keine Logik hier duplizieren)
            // Implement inline to avoid separate missing method
            if (_battleArenaCtrl == null || !_isBattleActive)
            {
                System.Diagnostics.Debug.WriteLine("CastBattleSpell_Click: Battle not active or control missing.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"CastBattleSpell_Click: StrokeCount={_battleArenaCtrl.InkCanvas.Strokes.Count}");
            if (_battleArenaCtrl.InkCanvas.Strokes.Count == 0) return;

            _magicTimer.Stop();
            double secondsTaken = _magicTimer.Elapsed.TotalSeconds;

            var spellResult = ProcessSpellDrawing(_battleArenaCtrl.InkCanvas, secondsTaken);
            System.Diagnostics.Debug.WriteLine($"CastBattleSpell_Click: Recognized={spellResult.Label} Score={spellResult.Score}");
            string recognizedLabel = spellResult.Label;
            int finalScore = spellResult.Score;

            if (finalScore < 40)
            {
                _battleArenaCtrl.UpdateBotStatus("Deine Magie verpufft wirkungslos (Unter 40%)!");
            }
            else
            {
                int basePlayerDamage = (int)(finalScore * 0.25);
                int finalPlayerDamage = basePlayerDamage;

                bool isCounter = CheckIfPlayerCountersBot(recognizedLabel, _currentBotElement);

                if (isCounter)
                {
                    _botTimer?.Stop();
                    finalPlayerDamage = (int)(basePlayerDamage * 0.5);
                    _botHP -= finalPlayerDamage;
                    if (_botHP < 0) _botHP = 0;

                    UpdateHPUI();
                    _battleArenaCtrl.UpdateBotStatus($"GEKONTERT! Du löschst {_currentBotElement} mit {recognizedLabel}! Bot erleidet {finalPlayerDamage} DMG.");

                    _lastPlayerElement = recognizedLabel;
                    PlanNextBotAttack();
                }
                else
                {
                    _botHP -= finalPlayerDamage;
                    if (_botHP < 0) _botHP = 0;

                    UpdateHPUI();
                    _battleArenaCtrl.UpdateBotStatus($"Du triffst den Bot mit {recognizedLabel} ({finalPlayerDamage} DMG)!");
                    _lastPlayerElement = recognizedLabel;
                }
            }

            _battleArenaCtrl.InkCanvas.Strokes.Clear();
            _magicTimer.Reset();

            if (_botHP <= 0)
            {
                EndBattle(true);
            }
        }

        private void SaveCanvasAsToJpg(string path, InkCanvas canvas)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;
            if (width <= 0 || height <= 0) throw new InvalidOperationException("Canvas hat ungültige Größe zum Speichern.");
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);
            JpegBitmapEncoder enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using var fs = File.OpenWrite(path);
            enc.Save(fs);
        }

        private void SaveCanvasAsNormalizedJpg(string path, InkCanvas canvas, int targetWidth, int targetHeight)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            if (targetWidth <= 0 || targetHeight <= 0) throw new ArgumentOutOfRangeException("targetWidth/Height");

            // Create a DrawingVisual that paints a white background and the canvas content scaled to target size
            DrawingVisual dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, targetWidth, targetHeight));
                VisualBrush vb = new VisualBrush(canvas)
                {
                    Stretch = Stretch.Uniform
                };
                dc.DrawRectangle(vb, null, new Rect(0, 0, targetWidth, targetHeight));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(targetWidth, targetHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            JpegBitmapEncoder enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using var fs = File.OpenWrite(path);
            enc.Save(fs);
        }

        private double CalculateFormAccuracy(string spellName, InkCanvas canvas)
        {
            if (!_perfectPatterns.TryGetValue(spellName, out var pattern) || pattern.Count == 0) return 0.0;

            List<Point> drawn = new List<Point>();
            foreach (var stroke in canvas.Strokes)
                foreach (var p in stroke.StylusPoints)
                    drawn.Add(new Point(p.X, p.Y));

            if (drawn.Count < 5) return 0.0;

            var nDrawn = NormalizePoints(drawn);
            var nPattern = pattern;

            int n = nPattern.Count;

            double total = 0.0;

            for (int i = 0; i < n; i++)
            {
                double relativePosition = (double)i / (n - 1);

                int playerIndex = (int)Math.Round(relativePosition * (nDrawn.Count - 1));

                var a = nDrawn[playerIndex];
                var b = nPattern[i];

                total += Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            }

            double avg = total / n;

            double acc = 100.0 - (avg * 75.0);

            acc = Math.Max(0.0, Math.Min(100.0, acc));

            return acc;
        }

        private List<Point> NormalizePoints(List<Point> points)
        {
            if (points == null || points.Count == 0) return new List<Point>();
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            double w = Math.Max(1e-6, maxX - minX);
            double h = Math.Max(1e-6, maxY - minY);
            var res = new List<Point>(points.Count);
            foreach (var p in points)
            {
                res.Add(new Point((p.X - minX) / w, (p.Y - minY) / h));
            }
            return res;
        }

        private bool CheckIfPlayerCountersBot(string playerElement, string botElement)
        {
            switch (playerElement)
            {
                case "Water": return botElement == "Fire";     // Wasser löscht Feuer
                case "Fire": return botElement == "Earth";     // Feuer schmilzt Erde
                case "Earth": return botElement == "Wind";     // Erde blockiert Wind
                case "Wind": return botElement == "Thunder";   // Wind bläst Blitze weg
                case "Thunder": return botElement == "Water";  // Blitz schlägt in Wasser ein
                case "Light": return botElement == "Dark";     // Licht vertreibt Dunkelheit
                case "Dark": return botElement == "Light";     // Dunkelheit verschlingt Licht
                default: return false;
            }
        }

    }
}
