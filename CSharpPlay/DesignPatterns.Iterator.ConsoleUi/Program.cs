using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatterns.Iterator.ConsoleUi
{
    class Program
    {
        static void Main(string[] args)
        {
            var sequence = new FibonacciSequence(3);

            foreach(var i in sequence)
            {
                Console.WriteLine(i);
            }

            Console.ReadKey();
        }
    }
}
