using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Magic_Symbol_Recognizer.Controls
{
    public partial class SpellcastingControl : UserControl
    {
        public SpellcastingControl()
        {
            InitializeComponent();
            BtnBack.Click += (s, e) => BackClicked?.Invoke(this, e);
            BtnSavePattern.Click += (s, e) => SavePatternClicked?.Invoke(this, e);
            BtnCast.Click += (s, e) => CastClicked?.Invoke(this, e);
            BtnClear.Click += (s, e) => ClearClicked?.Invoke(this, e);
        }

        public InkCanvas InkCanvas => InkCanvasControl;
        public TextBox SpellNameTextBox => TxtSpellNameLocal;

        public event RoutedEventHandler BackClicked;
        public event RoutedEventHandler SavePatternClicked;
        public event RoutedEventHandler CastClicked;
        public event RoutedEventHandler ClearClicked;
    }
}