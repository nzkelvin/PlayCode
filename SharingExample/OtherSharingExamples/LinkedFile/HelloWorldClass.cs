using System;

namespace Shared
{
    public class HelloWorldClass
    {
#if WINDOWS_APP
        private string _platform = "Windows Store";
#elif WINDOWS_PHONE_APP
        private string _platform = "Windows Phone";
#else
        private string _platform = "...";
#endif

        public string SayHi()
	    {
            return "Hello from " + _platform;
	    }
    }
}
