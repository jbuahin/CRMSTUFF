using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thermotraq.Model;

namespace Thermotraq.Views
{
    /// <summary>
    /// Interaction logic for Configure.xaml
    /// </summary>
    public partial class Configure : Page
    {
        System.Timers.Timer timer;

        public Configure()
        {
            InitializeComponent();
            ComboBoxPorts.ItemsSource = SerialPort.GetPortNames();
            this.DataContext = App.Current as App;
            RetrieveCompanies();
            timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += Timer_Elapsed;
        }


        private async void RetrieveCompanies() {
           
            IEnumerable<company> companies = await GetAllCompanies();
            ComboBoxCompany.ItemsSource = companies;

            if (companies.Count()> 0) {
                ComboBoxCompany.SelectedItem = companies.First();
            }
        }
        

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate () { LabelStatus.Content = ""; });
            timer.Stop();
        }

        private void ButtonOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxPorts.SelectedValue != null && !string.IsNullOrEmpty(ComboBoxPorts.SelectedValue.ToString()))
            {
                App currentApp = App.Current as App;
                string errorMessage = "";

                if (currentApp.PortOpened && currentApp.ClosePort(out errorMessage))
                {
                    ButtonOpenPort.Content = "Open Port";
                    ComboBoxPorts.IsEnabled = true;
                    errorMessage = "Port Closed Successfully";
                }
                else if (!currentApp.PortOpened && currentApp.OpenPort(ComboBoxPorts.SelectedValue.ToString(), out errorMessage))
                {
                    ButtonOpenPort.Content = "Close Port";
                    ComboBoxPorts.IsEnabled = false;
                    errorMessage = "Port Opened Successfully";
                }

                LabelStatus.Content = errorMessage;
                timer.Start();
            }
        }

        private void ButtonRefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxPorts.ItemsSource = SerialPort.GetPortNames();
        }

        private void ButtonCanStart_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            ((App)App.Current).StartLogger(out errorMessage);
            errorMessage = "Chip Started Recording Successfully";
            LabelStatus.Content = errorMessage;
            timer.Start();
        }

        private void ButtonCanStop_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            ((App)App.Current).StopLogger(out errorMessage);
            errorMessage = "Chip Stopped Recording Successfully";
            LabelStatus.Content = errorMessage;
            timer.Start();
        }

        private void ButtonConfigure_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            ((App)App.Current).SetConfiguration(out errorMessage);
            errorMessage = "Chip Configured Successfully";
            LabelStatus.Content = errorMessage;
            timer.Start();

        }

        public async Task<IEnumerable<company>> GetAllCompanies()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("http://thermotraq.com");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("/api/companies");

            response.EnsureSuccessStatusCode(); // Throw on error code.
            var companies = await response.Content.ReadAsAsync<IEnumerable<company>>();
            return companies;
        }

        private void ComboBoxCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            App currentApp = App.Current as App;
            currentApp.CurrentCompany = ComboBoxCompany.SelectedItem as company;
        }
    }
}
