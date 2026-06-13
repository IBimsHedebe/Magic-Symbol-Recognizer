using System.Windows;
using System.Windows.Controls;

namespace Magic_Symbol_Recognizer.Controls
{
    public partial class MainMenuControl : UserControl
    {
        public MainMenuControl()
        {
            InitializeComponent();
            BtnOpenSpellcasting.Click += (s, e) => OpenSpellcastingClicked?.Invoke(this, e);
            BtnOpenBattleArena.Click += (s, e) => OpenBattleArenaClicked?.Invoke(this, e);
            BtnOpenLibrary.Click += (s, e) => OpenLibraryClicked?.Invoke(this, e);
            BtnCloseApp.Click += (s, e) => CloseAppClicked?.Invoke(this, e);
        }

        public event RoutedEventHandler OpenSpellcastingClicked;
        public event RoutedEventHandler OpenBattleArenaClicked;
        public event RoutedEventHandler OpenLibraryClicked;
        public event RoutedEventHandler CloseAppClicked;
    }
}
