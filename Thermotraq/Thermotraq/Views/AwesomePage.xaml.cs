using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for AwesomePage.xaml
    /// </summary>
    public partial class AwesomePage : Page
    {

        HttpClient client = new HttpClient();
        public AwesomePage()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://thermotraq.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.Loaded += Admin_Loaded;
        }

        async void Admin_Loaded(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync("/api/chips");
            response.EnsureSuccessStatusCode(); // Throw on error code.
            var chips = await response.Content.ReadAsAsync<IEnumerable<chips>>();
            ChipsDataGrid.ItemsSource = chips;
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var chip = new chips()
                {
                    uid = txbChipId.Text,
                    company_id = int.Parse(txbCompanyId.Text)
                };
                var response = await client.PutAsJsonAsync("/api/chips/", chip);
                response.EnsureSuccessStatusCode(); // Throw on error code.
                MessageBox.Show("Chip Added Successfully", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
                ChipsDataGrid.ItemsSource = await GetAllChips();
                ChipsDataGrid.ScrollIntoView(ChipsDataGrid.ItemContainerGenerator.Items[ChipsDataGrid.Items.Count - 1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public async Task<IEnumerable<chips>> GetAllChips()
        {
            HttpResponseMessage response = await client.GetAsync("/api/chips");
            response.EnsureSuccessStatusCode(); // Throw on error code.
            var chips = await response.Content.ReadAsAsync<IEnumerable<chips>>();
            return chips;
        }

    }
}
