using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatanaIntro
{
    using System.IO;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://localhost:8080";

            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("Start!");
                Console.ReadKey();
                Console.WriteLine("Stopping!");
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<HelloWorldComponent>();

            //app.UseWelcomePage();

            //app.Run(ctx =>
            //    {
            //        return ctx.Response.WriteAsync("Hello World!");
            //    });
        }
    }

    public class HelloWorldComponent
    {
        AppFunc _next;
        public HelloWorldComponent(AppFunc next)
        {
            this._next = next;
        }

        public Task Invoke(IDictionary<string, Object> environment)
        {
            var response = environment["owin.ResponseBody"] as Stream;
            using (var writer = new StreamWriter(response))
            {
                return writer.WriteAsync("Hello World!!!!!");
            }
        }
    }
}
