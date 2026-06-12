using System.Windows;
using System.Windows.Controls;

namespace Magic_Symbol_Recognizer.Controls
{
    public partial class LibraryControl : UserControl
    {
        public LibraryControl()
        {
            InitializeComponent();
            BtnBack.Click += (s, e) => BackClicked?.Invoke(this, e);
            ListSpells.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, e);
        }

        public ListBox SpellList => ListSpells;
        public Canvas Preview => PreviewCanvas;
        public TextBlock TitleText => TxtTitle;

        public event RoutedEventHandler BackClicked;
        public event RoutedEventHandler SelectionChanged;

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // bubble event
            SelectionChanged?.Invoke(this, new RoutedEventArgs());
        }
    }
}