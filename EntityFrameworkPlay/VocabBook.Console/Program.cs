using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using VocabBook.DataModel.Sql;

namespace VocabBook.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new VocabBookContext("VocabBookContextConnection");
            var c = (from e in ctx.DictionaryEntries
                     select e).Count();

            Console.WriteLine(c);
            Console.ReadKey();
        }
    }
}
