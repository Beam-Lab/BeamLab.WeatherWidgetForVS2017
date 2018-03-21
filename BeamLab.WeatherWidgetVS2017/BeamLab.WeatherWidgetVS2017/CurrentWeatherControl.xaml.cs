namespace BeamLab.WeatherWidgetVS2017
{
    using BeamLab.WeatherWidgetVS2017.Models;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using System.Device.Location;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for CurrentWeatherControl.
    /// </summary>
    public partial class CurrentWeatherControl : UserControl, INotifyPropertyChanged
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        GeoCoordinateWatcher geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

        public WeatherData WeatherData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentWeatherControl"/> class.
        /// </summary>
        public CurrentWeatherControl()
        {
            this.InitializeComponent();

            geoCoordinateWatcher.PositionChanged += GeoCoordinateWatcher_PositionChanged;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();

            UpateWeather();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpateWeather();
        }

        private void UpateWeather()
        {
            geoCoordinateWatcher.Start(true);
        }

        private async void GeoCoordinateWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (!CheckForInternetConnection())
                return;

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"http://api.apixu.com/v1/forecast.json?key=4cb51e4aeb5f48d0b3d02832181803&q={ e.Position.Location.Latitude.ToString().Replace(",", ".") },{ e.Position.Location.Longitude.ToString().Replace(",", ".") }&days=7");

                if (response.IsSuccessStatusCode)
                {
                    WeatherData = JsonConvert.DeserializeObject<WeatherData>(await response.Content.ReadAsStringAsync());

                    WeatherData.Current.Condition.Icon = "http:" + WeatherData.Current.Condition.Icon;

                    geoCoordinateWatcher.Stop();

                    RaisePropertyChanged(nameof(WeatherData));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://www.bing.com/"))
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}