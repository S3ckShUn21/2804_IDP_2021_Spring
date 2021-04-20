using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WirelessSensorNodeDashboard.Util
{
    class WeatherAPIProcessor
    {
        public static Dictionary<string, string> ImageMapping { get; private set; } = new Dictionary<string, string>()
        {
            { "Clouds", "/Resources/cloudy.png" },
            { "Clear",  "/Resources/sun.png" },
            { "Snow", "/Resources/snowflaske.png" },
            { "Rain", "/Resources/rain.png" },
            { "Thunderstorm", "/Resources/lightning.png" }
        };

        private static string APIString = "https://api.openweathermap.org/data/2.5/weather?zip={0},us&units=metric&appid=dc3c445e8bb901e8c6ff6b00e2803e34";

        public static async Task<WeatherAPIResponse> LoadWeatherInfo(int zipcode)
        {
            string apiCallString = string.Format(APIString, zipcode);

            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(apiCallString))
            {
                if (response.IsSuccessStatusCode)
                {
                    WeatherAPIResponse result = await response.Content.ReadAsAsync<WeatherAPIResponse>();

                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
