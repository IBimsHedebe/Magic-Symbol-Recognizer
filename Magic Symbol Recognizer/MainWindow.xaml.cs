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
        private Controls.LibraryControl _libraryCtrl;

        public MainWindow()
        {
            InitializeComponent();

            // Wire up control events by resolving them from XAML names
            _mainMenuCtrl = this.FindName("MainMenuControl") as Controls.MainMenuControl;
            _spellcastingCtrl = this.FindName("SpellcastingControl") as Controls.SpellcastingControl;
            _libraryCtrl = this.FindName("LibraryControl") as Controls.LibraryControl;

            if (_mainMenuCtrl != null)
            {
                _mainMenuCtrl.OpenSpellcastingClicked += (s, e) => OpenSpellcasting_Click(s, e);
                _mainMenuCtrl.OpenLibraryClicked += (s, e) => OpenLibrary_Click(s, e);
                _mainMenuCtrl.CloseAppClicked += (s, e) => CloseApp_Click(s, e);
            }

            if (_spellcastingCtrl != null)
            {
                _spellcastingCtrl.BackClicked += (s, e) => BackToMenu_Click(s, e);
                _spellcastingCtrl.SavePatternClicked += (s, e) => SavePattern_Click(s, e);
                _spellcastingCtrl.CastClicked += (s, e) => CastSpell_Click(s, e);
                _spellcastingCtrl.ClearClicked += (s, e) => ClearCanvas_Click(s, e);

                // Start timer on first user input on the InkCanvas
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

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl != null) _spellcastingCtrl.Visibility = Visibility.Collapsed;
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
                    _spellcastingCtrl.InkCanvas.Width = min - 40; // leave margin
                    _spellcastingCtrl.InkCanvas.Height = min - 40;
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

        private void CastSpell_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl == null || _spellcastingCtrl.InkCanvas.Strokes.Count == 0) return;
            _magicTimer.Stop();
            double seconds = _magicTimer.Elapsed.TotalSeconds;

            string tempImage = Path.Combine(Path.GetTempPath(), "temp_spell.jpg");
            try
            {
                SaveCanvasAsToJpg(tempImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Zeichnung: {ex.Message}", "Fehler");
                return;
            }

            var input = new MLModel1.ModelInput() { ImageSource = File.ReadAllBytes(tempImage) };
            string recognized = "Unbekannt";
            int successRate = 0;
            try
            {
                var scores = MLModel1.PredictAllLabels(input);
                var best = scores.FirstOrDefault();
                if (!best.Equals(default(KeyValuePair<string, float>)))
                {
                    recognized = best.Key;
                    successRate = (int)(best.Value * 100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Anwenden des Modells: {ex.Message}", "ML-Fehler");
                return;
            }

            double formScore = CalculateFormAccuracy(recognized);

            double timeScore = 100.0;
            if (seconds > 4.0)
            {
                timeScore -= (seconds - 4.0) * 25.0;
                timeScore = Math.Max(0, timeScore);
            }

            int finalScore = (int)((formScore * 0.7) + (timeScore * 0.3));

            if (finalScore < 40)
            {
                MessageBox.Show($"Unerkennbare Magie!\nDer Zauber wurde abgebrochen.\nGesamt-Score: {finalScore}%\nZeit: {seconds:F1}s", "Zauber fehlgeschlagen");
            }
            else
            {
                MessageBox.Show($"Magie entfesselt!\nErkannter Zauber: {recognized}\nErfolgsrate: {finalScore}%\nZeit: {seconds:F1}s", "Erfolg");
            }

            _spellcastingCtrl.InkCanvas.Strokes.Clear();
            _magicTimer.Reset();
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_spellcastingCtrl != null) _spellcastingCtrl.InkCanvas.Strokes.Clear();
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

        private void SaveCanvasAsToJpg(string path)
        {
            int width = (int)_spellcastingCtrl.InkCanvas.ActualWidth;
            int height = (int)_spellcastingCtrl.InkCanvas.ActualHeight;
            if (width <= 0 || height <= 0) throw new InvalidOperationException("Canvas hat ungültige Größe zum Speichern.");
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            if (_spellcastingCtrl != null) rtb.Render(_spellcastingCtrl.InkCanvas);
            JpegBitmapEncoder enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using var fs = File.OpenWrite(path);
            enc.Save(fs);
        }

        private double CalculateFormAccuracy(string spellName)
        {
            // 1. Prüfen, ob die Vorlage existiert und Punkte enthält
            if (!_perfectPatterns.TryGetValue(spellName, out var pattern) || pattern.Count == 0) return 0.0;

            // 2. Alle gezeichneten Punkte des Spielers einsammeln
            List<Point> drawn = new List<Point>();
            foreach (var stroke in SpellcastingControl.InkCanvas.Strokes)
                foreach (var p in stroke.StylusPoints)
                    drawn.Add(new Point(p.X, p.Y));

            // Falls nichts gezeichnet wurde oder zu wenig Punkte da sind, abbrechen
            if (drawn.Count < 5) return 0.0;

            // 3. Spieler-Punkte normalisieren (Skalieren auf 0.0 bis 1.0)
            var nDrawn = NormalizePoints(drawn);
            var nPattern = pattern;

            int n = nPattern.Count; // Wir orientieren uns strikt an der Anzahl der Vorlagen-Punkte

            double total = 0.0;

            // 4. Der schlaue Vergleich: Wir wandern gleichmäßig durch die Linien
            for (int i = 0; i < n; i++)
            {
                // Wir berechnen die relative Position im Muster (Wert zwischen 0.0 und 1.0)
                double relativePosition = (double)i / (n - 1);

                // Wir suchen den Punkt des Spielers, der zeitlich an genau derselben Stelle gezeichnet wurde
                int playerIndex = (int)Math.Round(relativePosition * (nDrawn.Count - 1));

                var a = nDrawn[playerIndex];
                var b = nPattern[i];

                // Abstand berechnen (Satz des Pythagoras)
                total += Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            }

            // 5. Durchschnittliche Abweichung berechnen
            double avg = total / n;

            // 6. Score berechnen (Toleranzfaktor 75.0 ist durch das Resampling jetzt perfekt balanciert)
            double acc = 100.0 - (avg * 75.0);

            // Unter .NET Framework / älteren Versionen gibt es Math.Clamp evtl. nicht, 
            // falls es Fehler wirft, nutzen Sie die Zeilen darunter:
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
    }
}
