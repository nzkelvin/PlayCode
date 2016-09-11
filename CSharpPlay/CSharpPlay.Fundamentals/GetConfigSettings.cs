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
            Console.WriteLine(Properties.Settings.Default.MyApplicationSetting);
            Console.WriteLine(ConfigurationManager.AppSettings["MyAppSetting"]);
        }
    }
}
