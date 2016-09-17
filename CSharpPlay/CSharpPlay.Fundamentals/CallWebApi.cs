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

        public static void GetLocationInfo()
        {
            using (var client = new HttpClient())
            {
                //var uri = "http://apidev.accuweather.com/locations/v1/search?q=auckland, nz&apikey=hoArfRosT1215";
                var uri = "http://apidev.accuweather.com/currentconditions/v1/349727.json?language=en&apikey=hoArfRosT1215";
                var response = client.GetAsync(uri).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Read as Stream doesn't work
                    var result = response.Content.ReadAsStringAsync().Result;
                    // StreamWriter.Write() doesn't work
                    byte[] byteArray = Encoding.UTF8.GetBytes(result);
                    var stream = new MemoryStream(byteArray);

                    //// Debugging code
                    //var sr = new StreamReader(stream);
                    //var output = sr.ReadToEnd();
                    //stream.Position = 0;

                    var js = new DataContractJsonSerializer(typeof(RootObject[]));
                    var location = (RootObject[])js.ReadObject(stream);

                    Console.WriteLine(location[0].Temperature.Metric.Value);
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
    public class RootObject
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
