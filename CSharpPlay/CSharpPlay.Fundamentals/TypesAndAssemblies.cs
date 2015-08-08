using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public class TypesAndAssemblies
    {
        /// <summary>
        /// Value type is genetally immutable. 
        /// String is a ref type but also immutable.
        /// </summary>
        public static void Immutable()
        {
            var date = new DateTime(2015, 8, 9);
            date = date.AddHours(25);

            var text = "     A string is ref type but immutatable    ";
            text = text.Trim();

            Console.WriteLine(date);
            Console.WriteLine(text);
        }
    }
}
