using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BulkUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ExcelDataService _objExcelSer;
        InpuDataObject _lookup = new InpuDataObject();
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>  
        /// Getting Data From Excel Sheet  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetLookupData();
        }

        private void GetLookupData()
        {
            _objExcelSer = new ExcelDataService();
            try
            {
                dataGridLookup.ItemsSource = _objExcelSer.ReadRecordFromEXCELAsync().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefreshRecord_Click(object sender, RoutedEventArgs e)
        {
            GetLookupData();
        }

        /// <summary>  
        /// Getting Data of each cell  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void dataGridLookup_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                FrameworkElement lookup_ID = dataGridLookup.Columns[0].GetCellContent(e.Row);
                if (lookup_ID.GetType() == typeof(TextBox))
                {
                    _lookup.LookupFieldId = ((TextBox)lookup_ID).Text;
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>  
        /// Get entire Row  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void dataGridLookup_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                bool IsSave = _objExcelSer.ManageExcelRecordsAsync(_lookup).Result;
                if (IsSave)
                {
                    MessageBox.Show("Lookup Record Added Successfully.");
                }
                else
                {
                    MessageBox.Show("Some Problem Occured.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>  
        /// Get Record info to update  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void dataGridLookup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lookup = dataGridLookup.SelectedItem as InpuDataObject;
        }
    }
}

