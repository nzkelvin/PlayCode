// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Crm.Sdk.Samples
{
    /// <summary>
    /// A basic hello world like application for Dynamics CRM. This sample authenticates
    /// the user and then sends the WhoAmI message to the Web service.
    /// </summary>
    /// <remarks> For information on Azure Active Directory app registration see
    /// https://msdn.microsoft.com/en-us/library/dn531010.aspx
    /// </remarks>
    class Program
    {
        static public void Main(string[] args)
        {
            // TODO Substitute your correct CRM service address, 
            string serviceUrl = "https://neuxrm.crm6.dynamics.com";            

            

            // TODO Substitute your app registration values that can be obtained after you
            // register the app in Active Directory on the Microsoft Azure portal.
            string clientId = "930d7a11-02f0-4ca0-9bb5-831f0dbfbc2e";
            string redirectUrl = "http://localhost/sdksampletest00";

            if (serviceUrl == "https://mydomain.crm.dynamics.com")
            {
                Console.WriteLine("You must update the serviceUrl variable in the main method before you run this sample.");
            }
            
            if (clientId == "e5cf0024-a66a-4f16-85ce-99ba97a24bb2")
            {
                Console.WriteLine("You must update the clientId variable in the main method before you run this sample.");

            }

            try
            {
                // Authenticate the registered application with Azure Active Directory.
                AuthenticationContext authContext =
                    new AuthenticationContext("https://login.windows.net/common", false);
                AuthenticationResult result = authContext.AcquireToken(serviceUrl, clientId, new Uri(redirectUrl));

                // Create an HTTP client to send a request message to the CRM Web service.
                using (HttpClient httpClient = new HttpClient())
                {
                    // Define the Web API address of the service and the period of time each request has to execute.
                    httpClient.BaseAddress = new Uri(serviceUrl);
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  // 2 minutes

                    // Set the authorization header.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                    // Send the WhoAmI request to the Web API.
                    // GET api/data/WhoAmI
                    var response = httpClient.GetAsync("api/data/WhoAmI", HttpCompletionOption.ResponseHeadersRead).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the response content and parse it.
                        JObject jresponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        Guid userId = (Guid)jresponse["UserId"];
                        Console.WriteLine("Your system user ID is {0}", userId);
                    }
                    else
                        Console.WriteLine("The request failed with a status of '{0}'", response.ReasonPhrase);
                }
            }
            catch (AdalException adalEx) {

                switch (adalEx.ErrorCode)
                { 
                    case "authentication_canceled":
                        //Authentication canceled.
                        Console.WriteLine(adalEx.Message);
                        break;
                    case "invalid_grant":
                        Console.WriteLine(adalEx.Message);
                        break;
                    default:
                        throw adalEx;
                        
                }                
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally {
                Console.WriteLine("Press <Enter> to exit the program.");
                Console.ReadLine();
            }

        }
    }
}
