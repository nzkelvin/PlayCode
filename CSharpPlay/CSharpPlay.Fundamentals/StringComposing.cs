using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public class StringComposing
    {
        public static void Run()
        {
            // Composite formatting with format string. String compositing
            var dateStr = String.Format("String compositing: {0:dd/MM/yyyy hh:mm:ss}", DateTime.Now);
            Console.WriteLine(dateStr);

            // String interpolation
            var dateInterStr = $"Interpolated String: {DateTime.Now:dd/MM/yyy hh:mm:ss}";
            Console.WriteLine(dateInterStr);

            // Format specifier
            var price = 99.99;
            Console.WriteLine($"Format specifier C2: {price:C2}");

            var perc = .9999;
            Console.WriteLine($"Format specifier P2: {perc:P2}");

            var hex = 299;
            Console.WriteLine($"Format specifier X: {hex:X}");

            // Dynamic format specifier
            int l = 6;
            string lFormat = $"D{l}";
            int val = 3333;
            // This won't work cos string interpolation is just a nicer version of String.Format.
            Console.WriteLine($"Dyna Interpolated String: {val:lFormat}");
            // This will work.
            Console.WriteLine($"Static Interpolated String: {val:D6}");

        }
    }
}
