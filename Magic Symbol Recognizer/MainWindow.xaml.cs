using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Magic_Symbol_Recognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

    private void CastSpell_Click(object sender, RoutedEventArgs e)
        {
            if (MagicCanvas.Strokes.Count == 0) return;

            // 1. Das gezeichnete Canvas als temporäres Bild speichern
            string tempImagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_spell.jpg");
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
    }

}