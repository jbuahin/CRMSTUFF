using System;
using System.Windows;
using J_RFID;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Thermotraq.Model;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;

namespace Thermotraq
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged, INotifyPropertyChanging
    {

        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds 
        private const int SPLASH_FADE_TIME = 500;     // Miliseconds 

        protected override void OnStartup(StartupEventArgs e)
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://thermotraq.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            // Step 1 - Load the splash screen 
            SplashScreen splash = new SplashScreen("/Asset/splash.jpg");
            splash.Show(false, true);

            // Step 2 - Start a stop watch 
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Step 3 - Load your windows but don't show it yet 
            base.OnStartup(e);
            MainWindow main = new MainWindow();

            // Step 4 - Make sure that the splash screen lasts at least two seconds 
            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            // Step 5 - show the page 
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            main.Show();
        }

        private string m_RFIDId, m_port;
        private bool m_portOpened, m_isRecording, m_isConfigured, m_continuous, m_canStart, m_canStop;
        private int m_recordGap, m_recordCount, m_delayStartTime,
            m_numTimesTempRangeExceeded, m_numRemainingUsage;
        DateTime m_lastConfiguredDateTime;
        int m_upperLimit, m_lowerLimit;

        public company CurrentCompany { get; set; }

        public string RFIDId
        {
            get { return m_RFIDId; }
            set
            {
                OnPropertyChanging("RFIDId");

                m_RFIDId = value;

                OnPropertyChanged("RFIDId");
            }
        }

        public string Port
        {
            get { return m_port; }
            set
            {
                OnPropertyChanging("Port");

                m_port = value;

                OnPropertyChanged("Port");
            }
        }

        public bool PortOpened
        {

            get { return m_portOpened; }
            set
            {
                OnPropertyChanging("PortOpened");

                m_portOpened = value;

                OnPropertyChanged("PortOpened");
            }

        }
        public int RecordGap
        {
            get { return m_recordGap; }
            set
            {
                OnPropertyChanging("RecordGap");

                m_recordGap = value;

                OnPropertyChanged("RecordGap");
            }

        }

        public int DelayStartTime
        {
            get { return m_delayStartTime; }
            set
            {
                OnPropertyChanging("DelayStart");

                m_delayStartTime = value;

                OnPropertyChanged("DelayStart");
            }
        }

        public int UpperLimitTemperature
        {
            get { return m_upperLimit; }
            set
            {
                OnPropertyChanging("UpperLimitTemperature");

                m_upperLimit = value;

                OnPropertyChanged("UpperLimitTemperature");
            }

        }
        public int LowerLimitTemperature
        {
            get { return m_lowerLimit; }
            set
            {
                OnPropertyChanging("LowerLimitTemperature");

                m_lowerLimit = value;

                OnPropertyChanged("LowerLimitTemperature");
            }

        }
        public int NumTimesTempRangeExceeded
        {
            get { return m_numTimesTempRangeExceeded; }
            set
            {
                OnPropertyChanging("NumTimesTempRangeExceeded");

                m_numTimesTempRangeExceeded = value;

                OnPropertyChanged("NumTimesTempRangeExceeded");
            }
        }

        public int RemainingUsage
        {
            get { return m_numRemainingUsage; }
            set
            {
                OnPropertyChanging("RemainingUsage");

                m_numRemainingUsage = value;

                OnPropertyChanged("RemainingUsage");
            }
        }

        public int RecordingTimeSpace
        {
            get { return m_recordGap; }
            set
            {
                OnPropertyChanging("RecordingTimeSpace");

                m_recordGap = value;

                OnPropertyChanged("RecordingTimeSpace");
            }
        }

        public int RecordCount
        {
            get { return m_recordCount; }
            set
            {
                OnPropertyChanging("RecordCount");

                m_recordCount = value;

                OnPropertyChanged("RecordCount");
            }
        }

        public bool Recording
        {
            get { return m_isRecording; }
            set
            {
                OnPropertyChanging("Recording");

                m_isRecording = value;

                OnPropertyChanged("Recording");
            }
        }

        public bool Configured
        {
            get { return m_isConfigured; }
            set
            {
                OnPropertyChanging("Configured");

                m_isConfigured = value;

                OnPropertyChanged("Configured");
            }
        }

        public bool Continuous
        {
            get { return m_continuous; }
            set
            {
                OnPropertyChanging("Continuous");

                m_continuous = value;

                OnPropertyChanged("Continuous");
            }
        }

        public bool CanStart
        {
            get { return m_canStart; }
            set
            {
                OnPropertyChanging("CanStart");

                m_canStart = value;

                OnPropertyChanged("CanStart");
            }
        }

        public bool CanStop
        {
            get { return m_canStop; }
            set
            {
                OnPropertyChanging("CanStop");

                m_canStop = value;

                OnPropertyChanged("CanStop");
            }
        }

        public DateTime LastConfiguredDateTime
        {
            get { return m_lastConfiguredDateTime; }
            set
            {
                OnPropertyChanging("LastConfiguredDateTime");

                m_lastConfiguredDateTime = value;

                OnPropertyChanged("LastConfiguredDateTime");
            }
        }


        public RFIDAPI RFID_API;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            PortOpened = false;
            RFID_API = new RFIDAPI();

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Close the reader and close the port
            PortOpened = false;
        }

        public bool OpenPort(string port, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                if (PortOpened)
                {
                    if (!ClosePort(out errorMessage))
                    {
                        errorMessage = "Unable to close existing opened port:" + Port;
                        return false;
                    }

                }

                int err = RFID_API.RFID_OpenReader(port);

                if (err != 0)
                {
                    errorMessage = "Unable to connect to port: " + port;
                    return false;
                }

                Port = port;
                PortOpened = true;
                ReadConfiguration(out errorMessage);
                return true;


            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return false;
        }

        public bool ClosePort(out string errorMessage)
        {
            errorMessage = "";

            if (PortOpened)
            {
                int err = RFID_API.RFID_CloseReader(Port);

                if (err != 0)
                {
                   errorMessage = "Unable to Close Port: " + Port;
                   return false;
                }
                PortOpened = false;

                return true;
            }
            return false;

        }

        public bool ReadConfiguration(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                if (PortOpened)
                {
                    string UID = "", Button = "", Date = "", Count = "", Upper_limit = "", Lower_limit = "", Countdown = "", Time_limit = "", Time_User = "", Time_cumulate = "", interval = "", State = "";

                    int err = RFID_API.RFID_Get_Info(out UID, out Button, out Date, out Count, out Upper_limit, out Lower_limit, out Countdown, out Time_limit, out Time_User, out Time_cumulate, out interval, out State);

                    RFIDId = UID;

                    switch (Button)
                    {
                        case "00":
                            CanStop = false;
                            CanStart = false;
                            break;
                        case "01":
                            CanStop = false;
                            CanStart = true;
                            break;
                        case "10":
                            CanStop = true;
                            CanStart = false;
                            break;
                        case "11":
                            CanStop = true;
                            CanStart = true;
                            break;
                    }

                    DateTime dtvalue;

                    if (DateTime.TryParse(Date, out dtvalue))
                    {
                        LastConfiguredDateTime = dtvalue;
                    }


                    int value;

                    if (int.TryParse(Count, out value))
                    {
                        RecordCount = value;
                    }

                    if (int.TryParse(Upper_limit, out value))
                    {
                        UpperLimitTemperature = value;
                    }

                    if (int.TryParse(Lower_limit, out value))
                    {
                        LowerLimitTemperature = value;
                    }

                    if (int.TryParse(Time_limit, out value))
                    {
                        NumTimesTempRangeExceeded = value;
                    }
                    if (int.TryParse(interval, out value))
                    {
                        RecordGap = value;
                    }
                    if (int.TryParse(Time_User, out value))
                    {
                        RemainingUsage = value;
                    }


                    switch (State)
                    {
                        case "00":
                            m_isRecording = false;
                            m_isConfigured = false;
                            break;
                        case "01":
                            m_isRecording = false;
                            m_isConfigured = true;
                            break;
                        case "02":
                            m_isRecording = true;
                            m_isConfigured = true;
                            break;
                    }

                    Recording = m_isRecording;
                    Configured = m_isConfigured;


                    if (Time_cumulate == "FF")
                    {
                        m_continuous = false;
                    }
                    else
                    {
                        m_continuous = true;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

            }

            return false;

        }


       public async void PostSessionsConfigurations()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://thermotraq.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           
                var sessions = new sessions()
                {
                    uid = m_RFIDId,
                    max_temp = m_upperLimit,
                    min_temp = m_lowerLimit,
                    receiver_id = CurrentCompany.company_id
                };
                var response = await client.PostAsJsonAsync("/api/sessions/", sessions);
                response.EnsureSuccessStatusCode(); // Throw on error code.
                MessageBox.Show("Chip was Added Successfully", "Result", MessageBoxButton.OK, MessageBoxImage.Information);

            
            
        }

        public async void UpdateChipConfigurations()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://thermotraq.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var chips = new chips()
                {
                    uid = m_RFIDId,
                    uses = m_numRemainingUsage,
                    active = true

                };
                var response = await client.PostAsJsonAsync("/api/chips/" + chips.uid, chips);
                response.EnsureSuccessStatusCode(); // Throw on error code.
                MessageBox.Show("Chip was Updated Successfully", "Result", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                string errorMessage;
                errorMessage = ex.Message;

                MessageBox.Show("Chip not Updated, May be due to Duplicate ID");
            }
            
        }

        public async void PostChipConfigurations()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://thermotraq.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var chips = new chips()
                {
                    uid = m_RFIDId,
                    uses = m_numRemainingUsage,
                    company_id= 4,
                    active = true
                };
                var response = await client.PostAsJsonAsync("/api/chips/", chips);
                response.EnsureSuccessStatusCode(); // Throw on error code.

            }
            catch (Exception ex)
            {
                string errorMessage;
                errorMessage = ex.Message;

               MessageBox.Show("Chip not Added, May be due to Duplicate ID");
                UpdateChipConfigurations();

            }
        }

    


        public bool SetConfiguration(out string errorMessage)
        {
            errorMessage = "";
            if (PortOpened && !m_isRecording)
            {
                string ButtonON = "", ButtonOFF = "", Time_cumulate = "";

                if (m_continuous)
                {
                    Time_cumulate = "0";
                }
                else
                {
                    Time_cumulate = "1";
                }

                ButtonON = m_canStart ? "1" : "0";
                ButtonOFF = m_canStop ? "1" : "0";

                int err = RFID_API.RFID_Set_TenpID(m_recordGap.ToString(),
                                                   m_delayStartTime.ToString(),
                                                   m_upperLimit.ToString(),
                                                   m_lowerLimit.ToString(),
                                                   m_numTimesTempRangeExceeded.ToString(),
                                                   Time_cumulate, ButtonON, ButtonOFF);
                if (err != 0)
                {
                    return false;
                }
                else
                {

                    PostSessionsConfigurations();
                    PostChipConfigurations();
                    ReadConfiguration(out errorMessage);

                }
                
                return true;
            }

            return false;

        }

        public void StartLogger(out string errorMessage)
        {
            try
            {
                int err = RFID_API.RFID_Start_TenpID();
                if (err == 0)
                    errorMessage = "success";
                else
                    errorMessage = "fail";
            }
            catch
            {
                errorMessage = "fail";
            }

            ReadConfiguration(out errorMessage);

        }

        public void StopLogger(out string errorMessage)
        {
            try
            {
                int err = RFID_API.RFID_Start_TenpID();
                if (err == 0)
                    errorMessage = "success";
                else
                    errorMessage = "fail";
            }
            catch
            {
                errorMessage = "fail";
            }

            ReadConfiguration(out errorMessage);


        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnPropertyChanging(string name)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(name));
            }
        }

    }
}
