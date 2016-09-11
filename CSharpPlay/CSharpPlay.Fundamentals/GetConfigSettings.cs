using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public static class GetConfigSettings
    {
        public static void Run()
        {
            // http://www.codeproject.com/Articles/616065/Why-Where-and-How-of-NET-Configuration-Files
            Console.WriteLine(Properties.Settings.Default.MyApplicationSetting);
            Console.WriteLine(ConfigurationManager.AppSettings["MyAppSetting"]);
        }
    }
}
