using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    class CallWebApi
    {
        public static void GetLocationInfoRaw()
        {
            using (var client = new HttpClient())
            {
                var uri = "http://apidev.accuweather.com/locations/v1/search?q=auckland, nz&apikey=hoArfRosT1215";
                var response = client.GetAsync(uri).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                }
            }
        }

        public static void GetWeatherInfoSimple()
        {
            var uri = "http://apidev.accuweather.com/currentconditions/v1/349727.json?language=en&apikey=hoArfRosT1215";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(uri).Result;

                if (response.IsSuccessStatusCode)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;

                    //// Test Code
                    //var sr = new StreamReader(stream);
                    //var output = sr.ReadToEnd();
                    //stream.Position = 0;

                    var js = new DataContractJsonSerializer(typeof(Weather[]));
                    var weather = (Weather[])js.ReadObject(stream);

                    Console.WriteLine(weather[0].Temperature.Metric.Value);
                }
            }
        }

        public static void GetWeatherInfo()
        {
            using (var client = new HttpClient())
            {
                //var uri = "http://apidev.accuweather.com/locations/v1/search?q=auckland, nz&apikey=hoArfRosT1215";
                var uri = "http://apidev.accuweather.com/currentconditions/v1/349727.json?language=en&apikey=hoArfRosT1215";
                var response = client.GetAsync(uri).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Read as Stream doesn't work
                    var respContent = response.Content.ReadAsStringAsync().Result;
                    byte[] byteArray = Encoding.UTF8.GetBytes(respContent);
                    var stream = new MemoryStream(byteArray);

                    //// An alternative way to convert a string to stream.
                    //var stream = new MemoryStream();
                    //var sw = new StreamWriter(stream);
                    //sw.Write(respContent);
                    //sw.Flush(); //Don't forget!!!
                    //stream.Position = 0;

                    //// Debugging code
                    //var sr = new StreamReader(stream);
                    //var output = sr.ReadToEnd();
                    //stream.Position = 0;

                    var js = new DataContractJsonSerializer(typeof(Weather[]));
                    var weather = (Weather[])js.ReadObject(stream);

                    Console.WriteLine(weather[0].Temperature.Metric.Value);
                }
            }
        }
    }

    //[DataContract]
    public class Metric
    {
        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public string Unit { get; set; }

        [DataMember]
        public int UnitType { get; set; }
    }

    //[DataContract]
    public class Imperial
    {
        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public string Unit { get; set; }

        [DataMember]
        public int UnitType { get; set; }
    }

    //[DataContract]
    public class Temperature
    {
        [DataMember]
        public Metric Metric { get; set; }

        [DataMember]
        public Imperial Imperial { get; set; }
    }

    [DataContract]
    public class Weather
    {
        [DataMember]
        public string LocalObservationDateTime { get; set; }

        [DataMember]
        public int EpochTime { get; set; }

        [DataMember]
        public string WeatherText { get; set; }

        [DataMember]
        public int WeatherIcon { get; set; }

        [DataMember]
        public bool IsDayTime { get; set; }

        [DataMember]
        public Temperature Temperature { get; set; }

        [DataMember]
        public string MobileLink { get; set; }

        [DataMember]
        public string Link { get; set; }
    }
}
