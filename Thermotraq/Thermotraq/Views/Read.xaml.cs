using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Thermotraq.Views
{

    public class TimeSeriesData
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }

        public Color ForegroundColor
        {
            get
            {
                return Colors.Red;
            }
        }

    }
    /// <summary>
    /// Interaction logic for Read.xaml
    /// </summary>
    public partial class Read : Page
    {


        ObservableCollection<TimeSeriesData> temperatureData;

        ChartValues<ObservablePoint> maxPoints;
        ChartValues<ObservablePoint> minPoints;
        ChartValues<ObservablePoint> timeSeries;

        ObservableCollection<TimeSeriesData> TemperatureData
        {
            get { return temperatureData; }

        }
        public Read()
        {
            InitializeComponent();

            maxPoints = new ChartValues<ObservablePoint>();
            minPoints = new ChartValues<ObservablePoint>();
            timeSeries = new ChartValues<ObservablePoint>();

            temperatureData = new ObservableCollection<TimeSeriesData>();
            temperatureData.Add(new TimeSeriesData());

            DataGridTemperature.ItemsSource = temperatureData;

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Upper Temperature Limit",
                    Values = maxPoints
                },
                new LineSeries
                {
                    Title = "Lower Temperature Limit",
                    Values = minPoints,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Current Temperature",
                    Values = timeSeries,
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            //Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            //XFormatter = value => DateTime.FromOADate(value).ToLongDateString();
            XFormatter = value => DateTime.FromOADate(value).ToString("MM-dd HH:mm:ss");

            ////modifying the series collection will animate and update the chart
            //SeriesCollection.Add(new LineSeries
            //{
            //    Title = "Series 4",
            //    Values = new ChartValues<double> { 5, 3, 2, 4 },
            //    LineSmoothness = 0, //0: straight lines, 1: really smooth lines
            //    PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            //    PointGeometrySize = 50,
            //    PointForeround = Brushes.Gray
            //});

            ////modifying any series values will also animate and update the chart
            //SeriesCollection[3].Values.Add(5d);

            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> XFormatter { get; set; }
        private void ButtonReadData_Click(object sender, RoutedEventArgs e)
        {
          DataGridTemperature.ItemsSource = null;
            temperatureData.Clear();

            App currentApp = Application.Current as App;

            if (currentApp.PortOpened && currentApp.RecordCount > 0)
            {
                string[] temp = null;
                string[] date = null;
                string max = "", min = "";
                currentApp.RFID_API.RFID_Download_TenpID(out temp, out date, out max, out min);


                string pattern = "yyyy- MM- dd  HH:mm:ss";
                for (int i = 0; i < temp.Length; i++)
                {
                    TimeSeriesData data = new TimeSeriesData();
                    DateTime dt;
                    if (DateTime.TryParseExact(date[i], pattern, null, System.Globalization.DateTimeStyles.None, out dt))
                    {
                        data.DateTime = dt;
                        data.Value = double.Parse(temp[i]);
                        temperatureData.Add(data);
                    }
                }

                minPoints.Clear();
                maxPoints.Clear();
                timeSeries.Clear();

                minPoints.Add(new ObservablePoint(temperatureData[0].DateTime.ToOADate(), double.Parse(min)));
                minPoints.Add(new ObservablePoint(temperatureData[TemperatureData.Count - 1].DateTime.ToOADate(), double.Parse(min)));


                maxPoints.Add(new ObservablePoint(temperatureData[0].DateTime.ToOADate(), double.Parse(max)));
                maxPoints.Add(new ObservablePoint(temperatureData[TemperatureData.Count - 1].DateTime.ToOADate(), double.Parse(max)));


                for (int i = 0; i < temperatureData.Count; i++)
                {
                    TimeSeriesData tsData = temperatureData[i];
                    timeSeries.Add(new ObservablePoint(tsData.DateTime.ToOADate(), tsData.Value));
                }

              DataGridTemperature.ItemsSource = temperatureData;
            }
        }
    }
}
