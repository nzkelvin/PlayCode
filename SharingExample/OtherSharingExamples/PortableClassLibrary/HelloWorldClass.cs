using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableClassLibrary
{
    public class HelloWorldClass
    {
        private string _platform;

        public HelloWorldClass(string platform)
        {
            _platform = platform;
        }

        public string SayHi()
        {
            return "Hello from " + _platform;
        }
    }
}
