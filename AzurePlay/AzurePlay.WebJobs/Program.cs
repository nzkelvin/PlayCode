using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzurePlay.WebJobs
{
    class Program
    {
        static void Main(string[] args)
        {
            var i = 0;
            while (i < 10)
            {
                Console.WriteLine("Test");
                Thread.Sleep(5000);
                i++;
            }
        }
    }
}
