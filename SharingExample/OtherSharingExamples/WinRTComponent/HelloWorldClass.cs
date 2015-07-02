using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinRTComponent
{
    /*
     * WinRT has quite a few restrictions. In terms of, what you can or cannot do. 
     * So use this when you have to. However WinRT component can be shared to differnt languages.
     */
    public sealed class HelloWorldClass
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
