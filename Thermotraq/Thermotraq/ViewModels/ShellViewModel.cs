using System;
using MahApps.Metro.IconPacks;
using MahApps.Metro.IconPacks.Material;

namespace Thermotraq.ViewModels
{
    internal class ShellViewModel : ViewModelBase
    {
        public ShellViewModel()
        {
            // Build the menus
            this.Menu.Add(new MenuItem() {Icon = new PackIconFontAwesome() {Kind = PackIconFontAwesomeKind.Connectdevelop}, Text = "Configure", NavigationDestination = new Uri("Views/Configure.xaml", UriKind.RelativeOrAbsolute)});
            this.Menu.Add(new MenuItem() { Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.LifeRing }, Text = "Read", NavigationDestination = new Uri("Views/Read.xaml", UriKind.RelativeOrAbsolute) });
            this.Menu.Add(new MenuItem() { Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FontAwesome }, Text = "Administration", NavigationDestination = new Uri("Views/AwesomePage.xaml", UriKind.RelativeOrAbsolute) });
            this.Menu.Add(new MenuItem() {Icon = new PackIconFontAwesome() {Kind = PackIconFontAwesomeKind.UserOutline}, Text = "User", NavigationDestination = new Uri("Views/UserPage.xaml", UriKind.RelativeOrAbsolute)});

            this.OptionsMenu.Add(new MenuItem() {Icon = new PackIconFontAwesome() {Kind = PackIconFontAwesomeKind.Cogs}, Text = "Settings", NavigationDestination = new Uri("Views/SettingsPage.xaml", UriKind.RelativeOrAbsolute)});
            this.OptionsMenu.Add(new MenuItem() {Icon = new PackIconFontAwesome() {Kind = PackIconFontAwesomeKind.InfoCircle}, Text = "About", NavigationDestination = new Uri("Views/AboutPage.xaml", UriKind.RelativeOrAbsolute)});
        }
    }
}