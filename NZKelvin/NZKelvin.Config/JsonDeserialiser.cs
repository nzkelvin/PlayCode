using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NZKelvin.Config
{
    public static class JsonDeserialiser
    {
        public static T DeserialiseJsonStringToObject<T>(this string jsonString) where T : class
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                return serialiser.ReadObject(stream) as T;
            }
        }
    }
}
