using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    // https://blog.udemy.com/json-serializer-c-sharp/
    public class JsonSerialization
    {
        public static void Run()
        {
            Spell frostShock = new Spell()
            {
                cast = "Instance",
                cooldown = "6 sec cooldown",
                description = "Instantly shocks an enemy with frost...",
                icon = "spell_frost_frostshock",
                id = 8056,
                name = "Frost Shock",
                cost = "21% of base mana",
                range = "25 yd range"
            };

            var js = new DataContractJsonSerializer(typeof(Spell));
            MemoryStream ms = new MemoryStream();
            js.WriteObject(ms, frostShock);

            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            Console.WriteLine(sr.ReadToEnd());
            sr.Close();
            ms.Close();
        }
    }

    [DataContract]
    class Spell
    {
        [DataMember]
        public String cast;

        [DataMember]
        public String cooldown;

        [DataMember]
        public String cost;

        [DataMember]
        public String description;

        [DataMember]
        public String icon;

        [DataMember]
        public Int16 id;

        [DataMember]
        public String name;

        [DataMember]
        public String range;
    }
}
