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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;
using Microsoft.Crm.Sdk.Samples.HelperCode;



namespace Microsoft.Crm.Sdk.Samples
{
    class WebAPIPreviewServiceSample
    {
        static public void Main(string[] args)
        {
            try
            {
                WebAPIPreviewServiceSample app = new WebAPIPreviewServiceSample();


                // The first argument on the command line is the connection string name.
                String[] arguments = Environment.GetCommandLineArgs();

                // Create a configuration object to store the service URL and app registration settings.
                Configuration config = null;
                if (arguments.Length > 1)
                    config = new Configuration(arguments[1], arguments[0] + ".config");
                else
                    config = new Configuration();


                // Authenticate the user to obtain the OAuth access and refresh tokens.
                Authentication auth = new Authentication(config);


                Task.WaitAll(Task.Run(async () => await app.Run(auth, config)));
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

            finally
            {
                Console.WriteLine("Press <Enter> to exit the program.");
                Console.ReadLine();
            }

        }

        public async Task Run(Authentication auth, Configuration config)
        {

            try
            {

                using (WebAPIPreviewService webAPIPreviewService = new WebAPIPreviewService(auth, config))
                {
                    //Demonstrates how to retrieve a list of entities
                    #region Get entity list
                    JArray entities = await webAPIPreviewService.GetEntityList();

                    Console.WriteLine("{0} entities returned.", entities.Count);
                    JArray sortedEntities = new JArray(entities.OrderBy(obj => obj["name"].ToString().ToLower()));
                    foreach (var item in sortedEntities)
                    {
                        Console.WriteLine(item["name"]);
                    }

                    #endregion Get entity list
                    
                    //Demonstrates how to associate and disassociate using the single-valued navigation property.
                    #region Add and Remove Reference

                    //Create a contact
                    JObject contactA = new JObject();
                    contactA.Add("firstname", "Tom");
                    contactA.Add("lastname", "Test");
                    Uri contactAUri = await webAPIPreviewService.Create("contacts", contactA);

                    //Create an account
                    JObject accountA = new JObject();
                    accountA.Add("name", "Tom's Company");
                    Uri accountAUri = await webAPIPreviewService.Create("accounts", accountA);

                    //Set the contact as the primary contact for the account
                    await webAPIPreviewService.AddReference(accountAUri, "account_primary_contact", contactAUri);

                    //Retrieve the account
                    accountA = await webAPIPreviewService.Retrieve(accountAUri, new String[] { "name" }, new String[] { "account_primary_contact($select=fullname)" }, true);

                    //Get the fullname property of the primary contact - it should have a value
                    String primaryContactValue = (accountA["account_primary_contact"] == null) ? "null" : accountA["account_primary_contact"]["fullname"].ToString();
                    //Show the primary contact value - it should be 'Tom Test'
                    Console.WriteLine("Primary contact for {0} is {1}.", accountA["name"], primaryContactValue);
                    //Remove the contact as the primary contact for the account
                    await webAPIPreviewService.RemoveReference(accountAUri, "account_primary_contact");
                    //Retrieve the account again
                    accountA = await webAPIPreviewService.Retrieve(accountAUri, new String[] { "name" }, new String[] { "account_primary_contact($select=fullname)" }, true);
                    //Get the fullname property of the primary contact - it should be null
                    primaryContactValue = (accountA["account_primary_contact"] == null) ? "null" : accountA["account_primary_contact"]["fullname"].ToString();
                    //Show the primary contact value - it should be null
                    Console.WriteLine("Primary contact for {0} is {1}.", accountA["name"], primaryContactValue);

                    //Delete the account and contact created
                    await webAPIPreviewService.Delete(accountAUri);
                    await webAPIPreviewService.Delete(contactAUri);


                    #endregion  Add and Remove Reference
                    
                    //Demonstrates the use of Upsert with options to prevent create or update
                    #region Upsert Contact
                    //Create a new contact with a specific contactid
                    Uri newContactUri = new Uri("/api/data/contacts(80db55c7-a16b-4851-b5c8-24186f3d86b6)", UriKind.Relative);
                    JObject contact = new JObject();
                    contact.Add("firstname", "Joe");
                    contact.Add("lastname", "Jones");
                    await webAPIPreviewService.Upsert(newContactUri, contact);

                    String fullName = await webAPIPreviewService.RetrievePropertyValue<String>(newContactUri, "fullname");
                    Console.WriteLine("New Contact fullname returned: {0}", fullName);

                    //Do not update the contact if it already exists
                    JObject doNotUpdateContact = new JObject();
                    doNotUpdateContact.Add("firstname", "Joseph");
                    doNotUpdateContact.Add("lastname", "Jones");
                    await webAPIPreviewService.Upsert(newContactUri, doNotUpdateContact, false, true);

                    String sameFullName = await webAPIPreviewService.RetrievePropertyValue<String>(newContactUri, "fullname");
                    Console.WriteLine("New Contact fullname returned: {0}", sameFullName);

                    //Do not create the contact if it doesn't already exist
                    Uri doNotCreateContactUri = new Uri("/api/data/contacts(ebeefeb4-c3aa-4d7b-a094-43288d3ccf95)", UriKind.Relative);
                    JObject doNotCreateContact = new JObject();
                    doNotCreateContact.Add("firstname", "Bob");
                    doNotCreateContact.Add("lastname", "Burns");
                    await webAPIPreviewService.Upsert(doNotCreateContactUri, doNotCreateContact, true);
                    try
                    {
                        JObject noContact = await webAPIPreviewService.Retrieve(doNotCreateContactUri, new String[] { "fullname" }, null, false);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.EndsWith("Does Not Exist"))
                        {
                            throw ex;
                        }

                        Console.WriteLine("Expected Error: {0}", ex.Message);
                    }

                    //Delete the contact
                    await webAPIPreviewService.Delete(newContactUri);
                    Console.WriteLine("Contact Deleted");

                    #endregion Upsert Contact

                    //Demonstrates creating new entity
                    #region Create New Account
                    Uri newAccountUri = null;
                    String newAccountId = null;

                    JObject account = new JObject();

                    //Application Required String
                    account.Add("name", "Sample Account");
                    //Boolean
                    account.Add("creditonhold", false);
                    // Double
                    account.Add("address1_latitude", 47.6395830);
                    //Memo
                    account.Add("description", "This is the description of the full account");
                    //Money
                    account.Add("revenue", 5000000);
                    //Picklist
                    account.Add("accountcategorycode", 1); //Preferred Customer

                    newAccountUri = await webAPIPreviewService.Create("accounts", account);
                    Console.WriteLine("New account created with Uri = {0}", newAccountUri);
                    #endregion Create New Account

                    //Demonstrates how to retrieve individual properties
                    #region Retrieve individual properties

                    //Retrieve individual properties from the new account
                    DateTime createdOn = await webAPIPreviewService.RetrievePropertyValue<DateTime>(newAccountUri, "createdon");
                    Console.WriteLine("Returned createdon: {0}", createdOn.ToLongDateString());
                    String createdby = await webAPIPreviewService.RetrievePropertyValue<String>(newAccountUri, "createdby");
                    Console.WriteLine("Returned createdby: {0}", createdby);
                    Int32 statusCode = await webAPIPreviewService.RetrievePropertyValue<Int32>(newAccountUri, "statuscode");
                    Console.WriteLine("Returned statusCode: {0}", statusCode);
                    //CreditLimit is null
                    Decimal? creditLimit = await webAPIPreviewService.RetrievePropertyValue<Decimal?>(newAccountUri, "creditlimit");
                    Console.WriteLine("Returned creditLimit: {0}", (creditLimit == null) ? "null" : creditLimit.ToString());

                    //Capture this value to use with QueryEntitySet filter example;
                    newAccountId = await webAPIPreviewService.RetrievePropertyValue<String>(newAccountUri, "accountid");

                    #endregion Retrieve individual properties

                    //Demonstrates how to update individual properties
                    #region Update individual properties

                    await webAPIPreviewService.UpdatePropertyValue(newAccountUri, "name", "New Improved Account Name");
                    //Then retrieve to verify it was set
                    String newName = await webAPIPreviewService.RetrievePropertyValue<String>(newAccountUri, "name");
                    Console.WriteLine("Updated name: {0}", newName);

                    #endregion Update individual properties

                    //Demonstrates how to associate entities using a collection-valued navigation property
                    #region Create Parent Account
                    JObject parentAccount = new JObject();
                    parentAccount.Add("name", "Parent Account");

                    Uri parentAccountUri = await webAPIPreviewService.Create("accounts", parentAccount);
                    Console.WriteLine("New Parent Account created with Uri = {0}", parentAccountUri);

                    #region Associate accounts

                    await webAPIPreviewService.Associate(parentAccountUri, "Referencedaccount_parent_account", newAccountUri);
                    Console.WriteLine("Accounts Associated");
                    #endregion Associate accounts

                    #endregion Create Parent Account

                    //Demonstrates associating records on create using @odata.bind
                    #region Add 3 related tasks

                    DateTime now = DateTime.Now;
                    DateTime tomorrow = now.AddDays(1);

                    for (int i = 1; i < 4; i++)
                    {

                        JObject task = new JObject();
                        task.Add("scheduledstart", tomorrow);
                        task.Add("Account_Tasks@odata.bind", newAccountUri);
                        task.Add("subject", String.Format("Task: {0}", i.ToString()));
                        await webAPIPreviewService.Create("tasks", task);

                    }
                    #endregion Add 3 related tasks

                    //Demonstrates the use of batch operations
                    #region Batch
                    // Add 2 related tasks in a batch
                    Guid batchId = Guid.NewGuid();

                    List<JObject> tasks = new List<JObject>();

                    JObject firstTask = new JObject();
                    firstTask.Add("subject", "Task 1 in batch");
                    firstTask.Add("Account_Tasks@odata.bind", newAccountUri);
                    tasks.Add(firstTask);

                    JObject secondTask = new JObject();
                    secondTask.Add("subject", "Task 2 in batch");
                    secondTask.Add("Account_Tasks@odata.bind", newAccountUri);
                    tasks.Add(secondTask);

                    List<HttpContent> payload = new List<HttpContent>();

                    String changeSetId = Guid.NewGuid().ToString();
                    MultipartContent changeSet = new MultipartContent("mixed", "changeset_" + changeSetId);

                    int taskNumber = 1;

                    tasks.ForEach(t =>
                    {
                        HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, config.ServiceUrl + "/api/data/tasks");
                        message.Content = new StringContent(JsonConvert.SerializeObject(t));
                        message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        HttpMessageContent messageContent = new HttpMessageContent(message);
                        messageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                        messageContent.Headers.Add("Content-Transfer-Encoding", "binary");
                        messageContent.Headers.Add("Content-ID", taskNumber.ToString());

                        changeSet.Add(messageContent);

                        taskNumber++;

                    });

                    payload.Add(changeSet);

                    HttpRequestMessage retrieveTasks = new HttpRequestMessage(HttpMethod.Get, newAccountUri + "/Account_Tasks?$select=subject");
                    HttpMessageContent retrieveTasksContent = new HttpMessageContent(retrieveTasks);

                    retrieveTasksContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                    retrieveTasksContent.Headers.Add("Content-Transfer-Encoding", "binary");
                    payload.Add(retrieveTasksContent);

                    String batchResponse = await webAPIPreviewService.ExecuteBatch(payload, batchId);

                    Console.WriteLine("Batch Response START");
                    Console.WriteLine(batchResponse);
                    Console.WriteLine("Batch Response END");


                    #endregion Batch

                    //Demonstrates a simple update
                    #region Update
                    JObject accntObj = new JObject();
                    accntObj.Add("name", "Updated Account Name");
                    accntObj.Add("description", "Sample Account Description Updated.");

                    await webAPIPreviewService.Update(newAccountUri, accntObj);
                    Console.WriteLine("Account Updated");

                    #endregion Update

                    //Demonstrates retrieve with related entities
                    #region Retrieve
                    String[] properties = {
"accountcategorycode",
"accountclassificationcode",
"accountid",
"accountnumber",
"accountratingcode",
"businesstypecode",
"creditonhold",
"createdon",
"lastusedincampaign",
"address1_latitude",
"address1_longitude",
"numberofemployees",
"parentaccountid",
"description",
"name",
"revenue"};

                    String[] navigationProperties = { 
"Referencingaccount_parent_account($select=createdon,name)", //Data from parent account   
"Account_Tasks($select=subject,scheduledstart)"  //Data from related tasks                                   
};

                    JObject retrievedAccount = await webAPIPreviewService.Retrieve(newAccountUri, properties, navigationProperties, true);

                    Console.WriteLine("Account retrieved");
                    Console.WriteLine("Account accountcategorycode value: {0}", retrievedAccount.GetValue("accountcategorycode"));
                    //Access formatted value
                    String formattedAccountCategoryCodeValue = (String)retrievedAccount.GetValue("accountcategorycode@mscrm.formattedvalue");
                    Console.WriteLine("Account accountcategorycode formatted value: {0}", formattedAccountCategoryCodeValue);

                    Console.WriteLine("Parent Account name: {0}", retrievedAccount.GetValue("Referencingaccount_parent_account")["name"]);



                    //Access related tasks
                    Console.WriteLine("Related tasks subject values:");
                    retrievedAccount.GetValue("Account_Tasks").ToList().ForEach(delegate(JToken relatedTask)
                    {
                        Console.WriteLine("    Task Subject: {0}", relatedTask.ToObject<JObject>().GetValue("subject"));
                    });

                    #endregion Retrieve

                    //Demonstrates querying an entity set and retrieving additional pages
                    #region QueryEntitySet

                    String query = String.Format("$filter=regardingobjectid eq {0}&$select=subject", newAccountId);
                    Boolean includeFormattedValues = true;
                    uint maxPageSize = 2;


                    JObject QueryEntitySetActivitiesResponse = await webAPIPreviewService.QueryEntitySet("activitypointers", query, includeFormattedValues, maxPageSize);


                    Console.WriteLine("First page of activities retrieved using QueryEntitySet:");
                    QueryEntitySetActivitiesResponse.GetValue("value").ToList().ForEach(delegate(JToken relatedActivity)
                    {
                        Console.WriteLine("    Activity Subject: {0}", relatedActivity.ToObject<JObject>().GetValue("subject"));
                    });

                    Uri nextPageQuery = (Uri)QueryEntitySetActivitiesResponse.GetValue("@odata.nextLink");
                    if (nextPageQuery != null)
                    {
                        JObject QueryEntitySetActivitiesNextPageResponse = await webAPIPreviewService.GetNextPage(nextPageQuery, includeFormattedValues, maxPageSize);
                        Console.WriteLine("Second page of activities retrieved using QueryEntitySet:");
                        QueryEntitySetActivitiesNextPageResponse.GetValue("value").ToList().ForEach(delegate(JToken relatedActivity)
                        {
                            Console.WriteLine("    Activity Subject: {0}", relatedActivity.ToObject<JObject>().GetValue("subject"));
                        });
                    }
                    #endregion QueryEntitySet

                    //Demonstrates disassociation using a collection-valued navigation property
                    #region Disassociate
                    await webAPIPreviewService.Disassociate(parentAccountUri, "Referencedaccount_parent_account", newAccountUri);
                    Console.WriteLine("Accounts disassociated");
                    #endregion Disassociate

                    //Demonstrates deleteing an individual property
                    #region Delete Property Value
                    Decimal? beforeRevenueValue = await webAPIPreviewService.RetrievePropertyValue<Decimal?>(newAccountUri, "revenue");
                    Console.WriteLine("Before revenue value: {0}", beforeRevenueValue);

                    await webAPIPreviewService.DeletePropertyValue(newAccountUri, "revenue");

                    Decimal? afterRevenueValue = await webAPIPreviewService.RetrievePropertyValue<Decimal?>(newAccountUri, "revenue");
                    Console.WriteLine("After revenue value: {0}", afterRevenueValue);

                    #endregion Delete Property Value

                    //Demonstrates deleteing entities
                    #region Delete

                    await webAPIPreviewService.Delete(newAccountUri);
                    Console.WriteLine("Account Deleted");
                    //Tasks are deleted with the Account
                    await webAPIPreviewService.Delete(parentAccountUri);
                    Console.WriteLine("Parent Account Deleted");

                    #endregion Delete

                    //Demonstrates using an unbound function : WhoAmI
                    #region WhoAmI
                    String UserId; //Used in a following RetrieveUserQueues sample

                    JObject WhoAmIResponse = await webAPIPreviewService.InvokeUnboundFunction("WhoAmI", null);
                    Console.WriteLine("Results from WhoAmI function:");
                    UserId = (String)WhoAmIResponse.GetValue("UserId");
                    Console.WriteLine(" UserId: {0}", WhoAmIResponse.GetValue("UserId"));
                    Console.WriteLine(" BusinessUnitId: {0}", WhoAmIResponse.GetValue("BusinessUnitId"));
                    Console.WriteLine(" OrganizationId: {0}", WhoAmIResponse.GetValue("OrganizationId"));

                    #endregion WhoAmI

                    //Demonstrates using an unbound function : GetAllTimeZonesWithDisplayName
                    #region GetAllTimeZonesWithDisplayName
                    JArray lcidParams = new JArray();
                    lcidParams.Add("LocaleId=1033");

                    JObject GATZWDNResponse = await webAPIPreviewService.InvokeUnboundFunction("GetAllTimeZonesWithDisplayName", lcidParams);

                    Console.WriteLine("GetAllTimeZonesWithDisplayName Function Response values:");

                    foreach (var item in GATZWDNResponse.GetValue("value").Children())
                    {
                        Console.WriteLine(" {0}", item["userinterfacename"].ToString());
                    }

                    #endregion GetAllTimeZonesWithDisplayName

                    //Demonstrates using an unbound function : RetrieveUserQueues
                    #region RetrieveUserQueues
                    JArray parameters = new JArray();
                    parameters.Add(String.Format("UserId={0}", UserId));
                    parameters.Add("IncludePublic=true");
                    JObject RetrieveUserQueuesResponse = await webAPIPreviewService.InvokeUnboundFunction("RetrieveUserQueues", parameters);

                    Console.WriteLine("Returned {0} user queues.", RetrieveUserQueuesResponse.GetValue("value").Children().Count());

                    #endregion RetrieveUserQueues

                    //Demonstrates using a bound function : mscrm.GetSavedQueries
                    #region mscrm.GetSavedQueries

                    JArray queries = await webAPIPreviewService.InvokeBoundFunction("accounts", "mscrm.GetSavedQueries");
                    if (queries.Count > 0)
                    {
                        Console.WriteLine("These are the saved queries for the account entity.");

                        foreach (var item in queries.Children())
                        {
                            Console.WriteLine("   {0}", item["name"].ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no saved queries for this entity.");
                    }

                    #endregion mscrm.GetSavedQueries

                    //Demonstrates using an unbound action : WinOpportunity
                    #region WinOpportunity action

                    //Create an Account to own the Opportunity
                    JObject winOppAccount = new JObject();
                    winOppAccount.Add("name", "Win Opp Account");
                    Uri winOppAccountUri = await webAPIPreviewService.Create("accounts", winOppAccount);

                    //Create an associated opportunity to win
                    JObject winOpp = new JObject();
                    winOpp.Add("name", "Opportunity to Win");
                    winOpp.Add("opportunity_parent_account@odata.bind", winOppAccountUri);
                    Uri winOppUri = await webAPIPreviewService.Create("opportunities", winOpp);


                    //Create Opportunityclose object to pass with the parameters
                    JObject opportunityClose = new JObject();
                    opportunityClose.Add("@odata.type", "#mscrm.opportunityclose"); //Essential to describe the type of entity
                    opportunityClose.Add("subject", "Won Opp Activity");
                    opportunityClose.Add("description", "We won this opportunity.");
                    opportunityClose.Add("Opportunity_OpportunityClose@odata.bind", winOppUri);

                    //Prepare Parameter object
                    JObject winOppParams = new JObject();
                    winOppParams.Add("Status", 3);
                    winOppParams.Add("OpportunityClose", opportunityClose);

                    //Invoke the action
                    JObject winOppResponse = await webAPIPreviewService.InvokeUnboundAction("WinOpportunity", winOppParams);

                    Console.WriteLine("Opportunity Closed as Won.");

                    //Delete Account
                    await webAPIPreviewService.Delete(winOppAccountUri);
                    Console.WriteLine("Account deleted and all related records with it.");

                    #endregion WinOpportunity action

                }

            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }

        }

        private static void DisplayException(Exception ex)
        {
            Console.WriteLine("The application terminated with an error.");
            Console.WriteLine(ex.Message);
            while (ex.InnerException != null)
            {
                Console.WriteLine("\t{0}", ex.InnerException.Message);
                ex = ex.InnerException;
            }
        }

    }
}
