using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Async
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();

            Console.Read();
        }

        static async void Run()
        {
            AuthenticationService service = new AuthenticationService();

            var id = await service.AuthenticateUserAsync("jon", "c#5");

            Console.WriteLine(id);
        }
    }
}
