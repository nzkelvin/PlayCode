using Crm.CrmCommon.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.ConsoleApp
{
    // todo: encryption
    class Program
    {
        private const string _connStrName = "Xrm";

        static void Main(string[] args)
        {
            //var xrm = new DataContext(new OrganizationService(_connStrName));

            //var request = new WhoAmIRequest();
            //var response = xrm.Execute(request);

            //Console.WriteLine(response.Results.FirstOrDefault());
            TestQuery();

            Console.ReadKey();
        }

        public static void TestQuery()
        {
            using (var ctx = new DataContext(new OrganizationService(_connStrName)))
            {
                var contacts = ctx.ContactSet.Select(c => c).Take(5);

                foreach (var c in contacts)
                {
                    Console.WriteLine(c.FirstName);
                }
            }
        }
    }
}
