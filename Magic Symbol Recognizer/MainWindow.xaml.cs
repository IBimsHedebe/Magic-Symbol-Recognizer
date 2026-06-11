using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Path = System.IO.Path;

namespace Magic_Symbol_Recognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Dictionary<string, List<Point>> _perfectPatterns = new Dictionary<string, List<Point>>();

        public MainWindow()
        {
            InitializeComponent();
            // Keine zusätzliche Ereignis-Subscription hier nötig
            LoadPatternsFromFile();
        }

        private void LoadPatternsFromFile()
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZauberMuster");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.json");

            foreach (string file in files)
            {
                try
                {
                    string jsonString = File.ReadAllText(file);

                    List<Point> points = JsonSerializer.Deserialize<List<Point>>(jsonString);

                    string spellName = Path.GetFileNameWithoutExtension(file);
                    _perfectPatterns[spellName] = points;
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Fehler beim Lesen der Datei {file}: {ex.Message}");
                }
            }
        }

        private void CastSpell_Click(object sender, RoutedEventArgs e)
        {
            if (MagicCanvas.Strokes.Count == 0) return;

            // 1. Das gezeichnete Canvas als temporäres Bild speichern
            string tempImagePath = Path.Combine(Path.GetTempPath(), "temp_spell.jpg");
            SaveCanvasAsToJpg(tempImagePath);

            // 2. KI aufrufen (Der Klassenname wird von Visual Studio beim Generieren benannt, meist 'MagicModel')
            var input = new MLModel1.ModelInput()
            {
                ImageSource = File.ReadAllBytes(tempImagePath)
            };

            // Vorhersage treffen
            var result = MLModel1.Predict(input);

            // 3. Erfolgsrate (Konfidenz) ermitteln
            // gibt ein Array von Scores zurück. Wir suchen den höchsten Wert.
            float maxScore = 0;
            foreach (var score in result.Score)
            {
                if (score > maxScore) maxScore = score;
            }

            // Umrechnung in Prozent
            int successRate = (int)(maxScore * 100);

            // 4. Ergebnis dem Spieler anzeigen
            MessageBox.Show($"Erkannter Zauber: {result.PredictedLabel}\nErfolgsrate: {successRate}%", "Magie-Analyse");
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            MagicCanvas.Strokes.Clear();
        }

        // Hilfsfunktion: Wandelt das InkCanvas in ein JPG um, das die KI lesen kann
        private void SaveCanvasAsToJpg(string path)
        {
            int width = (int)MagicCanvas.ActualWidth;
            int height = (int)MagicCanvas.ActualHeight;

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(MagicCanvas);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream fs = File.OpenWrite(path))
            {
                encoder.Save(fs);
            }
        }

        // Hinweise: Export/Save-Funktionen sind in SavePattern_Click implementiert

        // Normalisiert eine Liste von Punkten: verschiebt auf 0, skaliert auf 1 und reduziert ggf. Anzahl
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

            double width = Math.Max(1, maxX - minX);
            double height = Math.Max(1, maxY - minY);

            List<Point> normalized = new List<Point>(points.Count);
            foreach (var p in points)
            {
                double nx = (p.X - minX) / width;
                double ny = (p.Y - minY) / height;
                normalized.Add(new Point(nx, ny));
            }

            return normalized;
        }

        private void SavePattern_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validierung: Hat der Spieler überhaupt gezeichnet und einen Namen eingegeben?
            if (MagicCanvas.Strokes.Count == 0)
            {
                MessageBox.Show("Bitte zeichnen Sie zuerst ein perfektes Symbol auf das Malfeld!", "Fehler");
                return;
            }

            string spellName = TxtSpellName.Text.Trim();
            if (string.IsNullOrWhiteSpace(spellName))
            {
                MessageBox.Show("Bitte geben Sie einen Namen für den Zauber in das Textfeld ein!", "Fehler");
                return;
            }

            // 2. Alle gezeichneten Punkte einsammeln
            List<Point> playerPoints = new List<Point>();
            foreach (var stroke in MagicCanvas.Strokes)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    playerPoints.Add(new Point(point.X, point.Y));
                }
            }

            // 3. Punkte normalisieren (Nutzt Ihre bestehende Funktion, um die Form auf 0.0 - 1.0 zu skalieren)
            List<Point> normalizedPoints = NormalizePoints(playerPoints);

            try
            {
                // 4. Ordnerpfad "ZauberMuster" direkt neben der EXE bestimmen
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZauberMuster");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 5. Die Punktliste in das JSON-Format umwandeln (schön formatiert untereinander)
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(normalizedPoints, options);

                // 6. Datei abspeichern (z.B. ZauberMuster/Feuer.json)
                string filePath = Path.Combine(folderPath, $"{spellName}.json");
                File.WriteAllText(filePath, jsonString);

                // 7. Das neue Muster direkt im aktuell laufenden Spiel aktivieren, ohne Neustart
                _perfectPatterns[spellName] = normalizedPoints;

                MessageBox.Show($"Muster für '{spellName}' erfolgreich gespeichert!\n\nDatei erstellt unter:\n{filePath}", "Vorlage gespeichert");

                // Feld und Canvas aufräumen
                MagicCanvas.Strokes.Clear();
                TxtSpellName.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Datei: {ex.Message}", "Schreibfehler");
            }
        }

    }

}