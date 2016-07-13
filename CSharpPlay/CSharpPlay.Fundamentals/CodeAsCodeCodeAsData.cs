using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public class CodeAsCodeCodeAsData
    {
        public static void Run()
        {
            //// Code as code
            //Func<int, int, int> add = (x, y) => x + y;
            //Func<int, int> square = x => x * x;
            //Action<int> log = x => Console.WriteLine(x);

            //log(square(add(3, 5)));

            //Code as data
            Expression<Func<int, int, int>> add = (x, y) => x + y;
            Expression<Func<int, int>> square = x => x * x;
            Expression<Action<int>> log = x => Console.WriteLine(x);

            log.Compile();
        }
    }
}
