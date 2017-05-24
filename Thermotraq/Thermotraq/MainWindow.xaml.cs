using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using Thermotraq.Views;
using MenuItem = Thermotraq.ViewModels.MenuItem;


namespace Thermotraq
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // Navigate to the home page.
            Navigation.Navigation.Frame = new Frame(); //SplitViewFrame;
            Navigation.Navigation.Frame.Navigated += SplitViewFrame_OnNavigated;
            this.Loaded += (sender, args) => Navigation.Navigation.Navigate(new MainPage());
        }

        private void SplitViewFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            HamburgerMenuControl.Content = e.Content;
            GoBackButton.Visibility = Navigation.Navigation.Frame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = e.ClickedItem as MenuItem;
            if (menuItem != null && menuItem.IsNavigation)
            {
                Navigation.Navigation.Navigate(menuItem.NavigationDestination);
            }
        }

        private void GoBack_OnClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigation.GoBack();
        }

        private MetroWindow accentThemeTestWindow;
        private void ChangeAppStyleButtonClick(object sender, RoutedEventArgs e)
        {
            if (accentThemeTestWindow != null)
            {
                accentThemeTestWindow.Activate();
                return;
            }

            accentThemeTestWindow = new AccentStyleWindow();
            accentThemeTestWindow.Owner = this;
            accentThemeTestWindow.Closed += (o, args) => accentThemeTestWindow = null;
            accentThemeTestWindow.Left = this.Left + this.ActualWidth / 2.0;
            accentThemeTestWindow.Top = this.Top + this.ActualHeight / 2.0;
            accentThemeTestWindow.Show();
        }
    }
}
