using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace Magic_Symbol_Recognizer.Controls
{
    public partial class BattleArenaControl : UserControl
    {
        public BattleArenaControl()
        {
            InitializeComponent();
            BtnBack.Click += (s, e) => BackClicked?.Invoke(this, e);
            BtnCast.Click += (s, e) => CastClicked?.Invoke(this, e);
            BtnClear.Click += (s, e) => ClearClicked?.Invoke(this, e);
        }

        public InkCanvas InkCanvas => InkCanvasControl;

        public event RoutedEventHandler BackClicked;
        public event RoutedEventHandler CastClicked;
        public event RoutedEventHandler ClearClicked;

        public void UpdatePlayerHP(int currentHP, int maxHP = 500)
        {
            PlayerHPBar.Maximum = maxHP;
            PlayerHPBar.Value = currentHP;
            TxtPlayerHP.Text = $"{currentHP} / {maxHP} HP";

            UpdateHPBar(currentHP, maxHP);
        }

        public void UpdateBotHP(int currentHP, int maxHP = 500)
        {
            BotHPBar.Maximum = maxHP;
            BotHPBar.Value = currentHP;
            TxtBotHP.Text = $"{currentHP} / {maxHP} HP";

            UpdateHPBar(currentHP, maxHP);
        }

        public void UpdateHPBar(int currentHP, int maxHP)
        {
            if (currentHP > (1 / 3 * maxHP)) PlayerHPBar.Foreground = Brushes.LimeGreen;
            else if (currentHP > (1 / 2 * maxHP)) PlayerHPBar.Foreground = Brushes.Orange;
            else PlayerHPBar.Foreground = Brushes.Crimson;
        }

        public void UpdateBotStatus(string statusText)
        {
            TxtBotStatus.Text = statusText;
        }

        private void CanvasContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasContainer.SizeChanged -= CanvasContainer_SizeChanged;

            FrameworkElement parent = CanvasContainer.Parent as FrameworkElement;
            if (parent != null)
            {
                double minSize = Math.Min(parent.ActualWidth, parent.ActualHeight);
                if (minSize > 0)
                {
                    CanvasContainer.Width = minSize;
                    CanvasContainer.Height = minSize;
                }
            }

            CanvasContainer.SizeChanged += CanvasContainer_SizeChanged;
        }

    }
}