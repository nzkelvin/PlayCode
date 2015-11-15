using Crm.CrmCommon.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            var xrm = new DataContext(new OrganizationService("Xrm"));

            var request = new WhoAmIRequest();
            var response = xrm.Execute(request);

            System.Console.WriteLine(response.Results.FirstOrDefault());

            System.Console.ReadKey();
            
        }
    }
}
