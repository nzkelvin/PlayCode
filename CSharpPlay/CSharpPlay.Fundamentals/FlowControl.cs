using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public class FlowControl
    {
        /// <summary>
        /// Limitation 1: the swithc statement will only accept char string boo enumeralbe and their null types
        /// Limitation 2: You cannot do range in switch. e.g. score > 70 && score < 90
        /// </summary>
        public static void Switching()
        {
            var grade = 'A'; // input
            string result;

            switch (grade)
            {
                case 'A':
                    result = "Excellent";
                    break;
                case 'B':
                    result = "Very Good";
                    break;
                case 'C':
                    result = "Good";
                    break;
                default:
                    result = "Average";
                    break;
            }

            Console.WriteLine(result);
        }
    }
}
