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

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Microsoft.Crm.Sdk.Samples.HelperCode
{
    /// <summary>
    ///  Helper class that provides an HttpClient, authentication and configuration support as well as methods to perform actions using the Web API Preview
    /// </summary>
    public class WebAPIPreviewService : IDisposable
    {

        private HttpClient httpClient;
        private Authentication auth;
        private Configuration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAPIPreviewService"/> class.
        /// </summary>
        /// <param name="auth">The authentication.</param>
        /// <param name="config">The configuration.</param>
        public WebAPIPreviewService(Authentication auth, Configuration config)
        {

            this.auth = auth;
            this.config = config;
          


            // Define the Web API address of the service and the period of time each request has to execute.
            httpClient = initialize(new HttpClient());

        }

        /// <summary>
        /// Sets the client if you want to modify the default settings, such as the Timeout.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public HttpClient Client
        {
            set
            {
                httpClient = initialize(value);
            }
            get {
                return httpClient;
            }
        }

        private HttpClient initialize(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(config.ServiceUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            return httpClient;

        }

        /// <summary>
        /// Creates a new entity for a the type specified by the entitySetName
        /// </summary>
        /// <param name="entitySetName">Name of the entityset.</param>
        /// <param name="JSONObject">The json object.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<Uri> Create(String entitySetName, JObject JSONObject, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            String entityUri = String.Format("api/data/{0}", entitySetName);
            HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, entityUri);
            if (impersonateUser)
            {
                createRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }

            createRequest.Content = new StringContent(JsonConvert.SerializeObject(JSONObject), Encoding.UTF8, "application/json");
            //Refresh the access token
            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage createResponse;

            try
            {
                createResponse = await httpClient.SendAsync(createRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Create method", ex);
            }

            if (!createResponse.IsSuccessStatusCode)
            {
                throwCrmException(createResponse);
            }
            return new Uri(createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault());
        }

        /// <summary>
        /// Retrieves the specified entity.
        /// </summary>
        /// <param name="entityUri">The entity URI.</param>
        /// <param name="properties">The properties of the entity to return.</param>
        /// <param name="navigationProperties">The navigation properties to expand.</param>
        /// <param name="includeFormattedValues">if set to <c>true</c> [include formatted values].</param>
        /// <param name="eTag" optional="true">The previously retrieved ETag value for the entity. When included data will be returned only when it has changed.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JObject> Retrieve(Uri entityUri, String[] properties, String[] navigationProperties, Boolean includeFormattedValues, String eTag = null, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;
            Boolean retrieveOnlyIfChanged = eTag != null;

            if (!entityUri.IsAbsoluteUri)
            {
                entityUri = new Uri(new Uri(config.ServiceUrl), entityUri);
            }

            UriBuilder uri = new UriBuilder(entityUri);

            if (properties != null && properties.Length > 0)
            {

                uri.Query = String.Format("$select={0}", String.Join(",", properties));

            }

            if (navigationProperties != null && navigationProperties.Length > 0)
            {
                String navigationPropertyQuery = String.Format("$expand={0}", String.Join(",", navigationProperties));

                if (uri.Query != null && uri.Query.Length > 1)
                {
                    uri.Query = String.Format("{0}&{1}", uri.Query.Substring(1), navigationPropertyQuery);
                }
                else
                {
                    uri.Query = navigationPropertyQuery;
                }
            }

            HttpRequestMessage retrieveRequest = new HttpRequestMessage(HttpMethod.Get, uri.ToString());

            if (impersonateUser)
            {
                retrieveRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }

            if (retrieveOnlyIfChanged)
            {
                retrieveRequest.Headers.Add("If-None-Match", eTag);
            }
            //Refresh the access token
            retrieveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            if (includeFormattedValues)
            {
                retrieveRequest.Headers.Add("Prefer", "odata.include-annotations=\"mscrm.formattedvalue\"");
            }
            HttpResponseMessage retrieveResponse;

            try
            {
                retrieveResponse = await httpClient.SendAsync(retrieveRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Retrieve method", ex);
            }

            if (!retrieveResponse.IsSuccessStatusCode)
            {
                throwCrmException(retrieveResponse);
            }
            return (JObject)JsonConvert.DeserializeObject(await retrieveResponse.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Retrieves an individual property value from an entity
        /// </summary>
        /// <typeparam name="T">The Data type of the property</typeparam>
        /// <param name="entityUri">The entity Uri</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<T> RetrievePropertyValue<T>(Uri entityUri, String propertyName, Guid callerId = default(Guid)) {
            Boolean impersonateUser = callerId != Guid.Empty;



            HttpRequestMessage retrievePropertyValueRequest = new HttpRequestMessage(HttpMethod.Get, String.Format("{0}/{1}", entityUri.ToString(), propertyName));

            if (impersonateUser)
            {
                retrievePropertyValueRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            //Refresh the access token
            retrievePropertyValueRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

 
            HttpResponseMessage retrievePropertyValueResponse;

            try
            {
                retrievePropertyValueResponse = await httpClient.SendAsync(retrievePropertyValueRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.RetrievePropertyValue method", ex);
            }

            if (!retrievePropertyValueResponse.IsSuccessStatusCode)
            {
                throwCrmException(retrievePropertyValueResponse);
            }
            //return JsonConvert.DeserializeObject(await retrievePropertyValueResponse.Content.ReadAsStringAsync());

            JObject returnValue =  (JObject)JsonConvert.DeserializeObject(await retrievePropertyValueResponse.Content.ReadAsStringAsync());
            if (returnValue != null)
            {
                return returnValue.Value<T>("value");
            }
            else
            {
                return default(T);
            }
               
            
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entityUri">The entity URI.</param>
        /// <param name="updates">The updates.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task Update(Uri entityUri, JObject updates, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            HttpRequestMessage updateRequest = new HttpRequestMessage(new HttpMethod("PATCH"), entityUri);
            if (impersonateUser)
            {
                updateRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            updateRequest.Content = new StringContent(JsonConvert.SerializeObject(updates), Encoding.UTF8, "application/json");
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage updateResponse;

            try
            {
                updateResponse = await httpClient.SendAsync(updateRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Update method", ex);
            }

            if (!updateResponse.IsSuccessStatusCode)
            {
                throwCrmException(updateResponse);
            }
        }

        /// <summary>
        /// Updates an individual property of an entity
        /// </summary>
        /// <param name="entityUri">The entity Uri</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="updateValue">The value</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task UpdatePropertyValue(Uri entityUri, String propertyName, JToken updateValue, Guid callerId = default(Guid)) {
            Boolean impersonateUser = callerId != Guid.Empty;
            Uri propertyUri = new Uri(String.Format("{0}/{1}",entityUri.ToString(),propertyName));
            HttpRequestMessage updatePropertyValueRequest = new HttpRequestMessage(HttpMethod.Put, propertyUri);
            if (impersonateUser)
            {
                updatePropertyValueRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            JObject valueObjectContainer = new JObject();
            valueObjectContainer.Add("value", updateValue);
            updatePropertyValueRequest.Content = new StringContent(JsonConvert.SerializeObject(valueObjectContainer), Encoding.UTF8, "application/json");
            updatePropertyValueRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage updatePropertyValueResponse;

            try
            {
                updatePropertyValueResponse = await httpClient.SendAsync(updatePropertyValueRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.UpdatePropertyValue method", ex);
            }

            if (!updatePropertyValueResponse.IsSuccessStatusCode)
            {
                throwCrmException(updatePropertyValueResponse);
            }
        }

        /// <summary>
        /// Upserts an entity
        /// </summary>
        /// <param name="entityUri">The entity Uri</param>
        /// <param name="entity">The entity to create or update</param>
        /// <param name="preventCreate" optional="true">Do not create the entity if it doesn't already exist</param>
        /// <param name="preventUpdate" optional="true">Do not update the entity if it already exists</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task Upsert(Uri entityUri, JObject entity, Boolean preventCreate = false, Boolean preventUpdate = false, Guid callerId = default(Guid)) {
            Boolean impersonateUser = callerId != Guid.Empty;

            if (preventCreate && preventUpdate)
            {               
                //Do nothing if both preventCreate and preventUpdate are true
                return;
            }

            HttpRequestMessage upsertRequest = new HttpRequestMessage(new HttpMethod("PATCH"), entityUri);
            if (impersonateUser)
            {
                upsertRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            if (preventCreate) {
                upsertRequest.Headers.Add("If-Match", "*");
            }
            if (preventUpdate)
            {
                upsertRequest.Headers.Add("If-None-Match", "*");
            }
            upsertRequest.Content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
            upsertRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage upsertResponse;

            try
            {
                upsertResponse = await httpClient.SendAsync(upsertRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Upsert method", ex);
            }

            if (!upsertResponse.IsSuccessStatusCode)
            {
                //StatusCodee 412 PreconditionFailed expected when preventUpdate is true
                if (upsertResponse.StatusCode == HttpStatusCode.PreconditionFailed && preventUpdate)
                {
                    return;
                }
                //StatusCodee 404 NotFound expected when preventCreate is true
                if (upsertResponse.StatusCode == HttpStatusCode.NotFound && preventCreate)
                {
                    return;
                }
                throwCrmException(upsertResponse);
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entityUri">The entity URI.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task Delete(Uri entityUri, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            if (!entityUri.IsAbsoluteUri)
            {
                entityUri = new Uri(new Uri(config.ServiceUrl), entityUri);
            }

            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, entityUri);
            if (impersonateUser)
            {
                deleteRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);
            HttpResponseMessage deleteResponse;

            try
            {
                deleteResponse = await httpClient.SendAsync(deleteRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Delete method", ex);
            }


            if (!deleteResponse.IsSuccessStatusCode)
            {
                throwCrmException(deleteResponse);
            }
        }

        /// <summary>
        /// Deletes the value of the specified entity property
        /// </summary>
        /// <param name="entityUri">The entity Uri</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task DeletePropertyValue(Uri entityUri, String propertyName, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            if (!entityUri.IsAbsoluteUri)
            {
                entityUri = new Uri(new Uri(config.ServiceUrl), entityUri);
            }

            Uri propertyUri = new Uri(String.Format("{0}/{1}",entityUri.ToString(),propertyName));

            HttpRequestMessage deletePropertyValueRequest = new HttpRequestMessage(HttpMethod.Delete, propertyUri);
            if (impersonateUser)
            {
                deletePropertyValueRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            deletePropertyValueRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);
            HttpResponseMessage deletePropertyValueResponse;

            try
            {
                deletePropertyValueResponse = await httpClient.SendAsync(deletePropertyValueRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.DeletePropertyValue method", ex);
            }


            if (!deletePropertyValueResponse.IsSuccessStatusCode)
            {
                throwCrmException(deletePropertyValueResponse);
            }
        }

        /// <summary>
        /// Associates two entities according to a specific relationship.
        /// </summary>
        /// <param name="parentUri">The parent entity URI.</param>
        /// <param name="navigationPropertyName">Name of the relationship.</param>
        /// <param name="childUri">The child entity URI.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task Associate(Uri parentUri, String navigationPropertyName, Uri childUri, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            Uri entityUri = new Uri(String.Format("{0}/{1}/$ref", parentUri, navigationPropertyName));
            HttpRequestMessage associateRequest = new HttpRequestMessage(HttpMethod.Post, entityUri);
            if (impersonateUser)
            {
                associateRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            JObject rel = new JObject();
            rel.Add("@odata.id", childUri.ToString());
            associateRequest.Content = new StringContent(JsonConvert.SerializeObject(rel), Encoding.UTF8, "application/json");
            //Refresh the access token
            associateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage associateResponse;
            try
            {
                associateResponse = await httpClient.SendAsync(associateRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

                throw new Exception("Error from WebAPIPreviewService.Associate method", ex);
            }


            if (!associateResponse.IsSuccessStatusCode)
            {
                throwCrmException(associateResponse);
            }

        }

        /// <summary>
        /// Disassociates two entities according to a specific relationship
        /// </summary>
        /// <param name="parentUri">The parent entity URI.</param>
        /// <param name="navigationPropertyName">Name of the relationship.</param>
        /// <param name="childUri">The child entity URI.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task Disassociate(Uri parentUri, String navigationPropertyName, Uri childUri, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            Uri entityUri = new Uri(String.Format("{0}/{1}/$ref?$id={2}", parentUri, navigationPropertyName, childUri));
            HttpRequestMessage disassociateRequest = new HttpRequestMessage(HttpMethod.Delete, entityUri);
            if (impersonateUser)
            {
                disassociateRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            //Refresh the access token
            disassociateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage disassociateResponse;

            try
            {
                disassociateResponse = await httpClient.SendAsync(disassociateRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.Disassociate method", ex);
            }

            if (!disassociateResponse.IsSuccessStatusCode)
            {
                throwCrmException(disassociateResponse);
            }
        }

        /// <summary>
        /// Remove the value of a single-valued navigation property
        /// </summary>
        /// <param name="childUri">The Uri for the child entity</param>
        /// <param name="navigationPropertyName">The name of the navigation property you want to use to disassociate the entities.</param>
        /// <param name="parentUri">The Uri for the entity you want to disassociate with the child entity.<</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task RemoveReference(Uri entityUri, String navigationPropertyName, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            if (!entityUri.IsAbsoluteUri)
            {
                entityUri = new Uri(new Uri(config.ServiceUrl), entityUri);
            }
         
            Uri propertyUri = new Uri(String.Format("{0}/{1}/$ref?", entityUri.ToString(), navigationPropertyName));

            HttpRequestMessage removeReferenceRequest = new HttpRequestMessage(HttpMethod.Delete, propertyUri);
            if (impersonateUser)
            {
                removeReferenceRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            removeReferenceRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);
            HttpResponseMessage removeReferenceResponse;

            try
            {
                removeReferenceResponse = await httpClient.SendAsync(removeReferenceRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.RemoveReference method", ex);
            }


            if (!removeReferenceResponse.IsSuccessStatusCode)
            {
                throwCrmException(removeReferenceResponse);
            }
        }

        /// <summary>
        /// Set the value of a single-valued navigation property
        /// </summary>
        /// <param name="entityUri">The Uri for the entity</param>
        /// <param name="navigationPropertyName">The name of the navigation property you want to use to associate the entities</param>
        /// <param name="referencedEntityUri">The Uri for the entity you want to associate with the child entity.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task AddReference(Uri entityUri, String navigationPropertyName, Uri referencedEntityUri, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            if (!entityUri.IsAbsoluteUri)
            {
                entityUri = new Uri(new Uri(config.ServiceUrl), entityUri);
            }

            if (!referencedEntityUri.IsAbsoluteUri)
            {
                referencedEntityUri = new Uri(new Uri(config.ServiceUrl), referencedEntityUri);
            }

            Uri propertyUri = new Uri(String.Format("{0}/{1}/$ref?", entityUri.ToString(), navigationPropertyName));
            HttpRequestMessage addReferenceRequest = new HttpRequestMessage(HttpMethod.Put, propertyUri);
            if (impersonateUser)
            {
                addReferenceRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            JObject valueObjectContainer = new JObject();
            valueObjectContainer.Add("@odata.id", referencedEntityUri);
            addReferenceRequest.Content = new StringContent(JsonConvert.SerializeObject(valueObjectContainer), Encoding.UTF8, "application/json");
            addReferenceRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage addReferenceResponse;

            try
            {
                addReferenceResponse = await httpClient.SendAsync(addReferenceRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.AddReference method", ex);
            }

            if (!addReferenceResponse.IsSuccessStatusCode)
            {
                throwCrmException(addReferenceResponse);
            }
        }

        /// <summary>
        /// Invokes a bound function.
        /// </summary>
        /// <param name="entitySetName">Name of the logical collection the function is bound to.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JArray> InvokeBoundFunction(String entitySetName, String functionName, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            UriBuilder uri = new UriBuilder(String.Format("{0}/api/data/{1}/{2}()", config.ServiceUrl, entitySetName, functionName));

            HttpRequestMessage boundFunctionRequest = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
            if (impersonateUser)
            {
                boundFunctionRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            boundFunctionRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage boundFunctionResponse;

            try
            {
                boundFunctionResponse = await httpClient.SendAsync(boundFunctionRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.InvokeBoundFunction method", ex);
            }

            if (!boundFunctionResponse.IsSuccessStatusCode)
            {

                throwCrmException(boundFunctionResponse);
            }

            JObject jQueries = (JObject)JsonConvert.DeserializeObject(await boundFunctionResponse.Content.ReadAsStringAsync());
            return (JArray)jQueries.GetValue("value");

        }

        /// <summary>
        /// Invokes an unbound function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The parameters must be a JArray of string values that represent a name and value separated by '='</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JObject> InvokeUnboundFunction(String functionName, JArray parameters, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            String uri = String.Format("api/data/{0}",  functionName);

            //Parsing the parameters to pass them using parameters aliases
            // DateTimeOffset  values can be broken if they are passed using the inline syntax. See https://github.com/OData/WebApi/issues/204
            if (parameters != null && parameters.Count > 0)
            {
                List<String> parameterNames = new List<string>();
                List<String> parameterAliasValues = new List<string>();
                int parameterNumber = 1;
                foreach (var item in parameters)
                {

                    String parameterText = (String)item;
                    String[] parameterParts = parameterText.Split(new Char[] { '=' });

                    if (parameterParts.Length != 2)
                    {
                        throw new ArgumentException("WebAPIService.InvokeUnboundFunction parameters parameter must be a JArray of string values that represent a name and value separated by '='.");
                    }
                    parameterNames.Add(String.Format("{0}=@p{1}", parameterParts[0], parameterNumber.ToString()));
                    parameterAliasValues.Add(String.Format("@p{0}={1}", parameterNumber.ToString(), parameterParts[1]));
                    
                        parameterNumber++;
                } 
                String joinedParameterNames = String.Join<String>(",",parameterNames);
                String joinedParameterAliasValues = String.Join<String>("&", parameterAliasValues);
                uri += String.Format("({0})?{1}",joinedParameterNames, joinedParameterAliasValues);

              
            }
            else
            {
                uri += "()";
            }

            HttpRequestMessage unboundFunctionRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            if (impersonateUser)
            {
                unboundFunctionRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            unboundFunctionRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage functionResponse;

            try
            {
                functionResponse = await httpClient.SendAsync(unboundFunctionRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.InvokeUnboundFunction method", ex);
            }

            if (!functionResponse.IsSuccessStatusCode)
            {

                throwCrmException(functionResponse);
            }

            return (JObject)JsonConvert.DeserializeObject(await functionResponse.Content.ReadAsStringAsync());


        }

        /// <summary>
        /// Invokes an unbound action.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JObject> InvokeUnboundAction(String actionName, JObject parameters, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            String actionUri = String.Format("api/data/{0}", actionName);
            HttpRequestMessage actionRequest = new HttpRequestMessage(HttpMethod.Post, actionUri);
            if (impersonateUser)
            {
                actionRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            if (parameters != null)
            {
                actionRequest.Content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            }

            //Refresh the access token
            actionRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage actionResponse;

            try
            {
                actionResponse = await httpClient.SendAsync(actionRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.InvokeUnboundAction method", ex);
            }

            if (!actionResponse.IsSuccessStatusCode)
            {
                throwCrmException(actionResponse);
            }
            switch (actionResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    //When the action returns a value
                    return new JObject(await actionResponse.Content.ReadAsStringAsync());
                default:
                    //When the action does not return a value, return an empty object
                    return new JObject();
            }
        }

        /// <summary>
        /// Retrieves multiple entities.
        /// </summary>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="query">The query.</param>
        /// <param name="includeFormattedValues">if set to <c>true</c> [include formatted values].</param>
        /// <param name="maxPageSize">Maximum number of entities to include with the page.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JObject> QueryEntitySet(String entitySetName, String query, Boolean includeFormattedValues, uint? maxPageSize, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            UriBuilder uri = new UriBuilder(String.Format("{0}/api/data/{1}", config.ServiceUrl, entitySetName));

            if (query != null && query.Length > 0)
            {

                uri.Query = query;

            }

            HttpRequestMessage queryEntitySetRequest = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
            if (impersonateUser)
            {
                queryEntitySetRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            //Refresh the access token
            queryEntitySetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            if (includeFormattedValues)
            {
                queryEntitySetRequest.Headers.Add("Prefer", "odata.include-annotations=\"mscrm.formattedvalue\"");
            }
            if (maxPageSize != null)
            {
                queryEntitySetRequest.Headers.Add("Prefer", String.Format("odata.maxpagesize={0}", maxPageSize.ToString()));
            }
            HttpResponseMessage queryEntitySetResponse;

            try
            {
                queryEntitySetResponse = await httpClient.SendAsync(queryEntitySetRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.QueryEntitySet method", ex);
            }

            if (!queryEntitySetResponse.IsSuccessStatusCode)
            {
                throwCrmException(queryEntitySetResponse);
            }

            return (JObject)JsonConvert.DeserializeObject(await queryEntitySetResponse.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Gets the next page after an initial retrieveMultiple.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="includeFormattedValues">if set to <c>true</c> [include formatted values].</param>
        /// <param name="maxPageSize">Maximum number of entities to include with the page.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<JObject> GetNextPage(Uri query, Boolean includeFormattedValues, uint? maxPageSize, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            HttpRequestMessage getNextPageRequest = new HttpRequestMessage(HttpMethod.Get, query.ToString());
            if (impersonateUser)
            {
                getNextPageRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }
            //Refresh the access token
            getNextPageRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            if (includeFormattedValues && maxPageSize != null)
            {
                getNextPageRequest.Headers.Add("Prefer", String.Format("odata.include-annotations=\"mscrm.formattedvalue\",odata.maxpagesize={0}", maxPageSize.ToString()));
            }
            else
            {
                if (includeFormattedValues)
                {
                    getNextPageRequest.Headers.Add("Prefer", "odata.include-annotations=\"mscrm.formattedvalue\"");
                }
                if (maxPageSize != null)
                {
                    getNextPageRequest.Headers.Add("Prefer", String.Format("odata.maxpagesize={0}", maxPageSize.ToString()));
                }
            }

            HttpResponseMessage getNextPageResponse;

            try
            {
                getNextPageResponse = await httpClient.SendAsync(getNextPageRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.GetNextPage method", ex);
            }

            if (!getNextPageResponse.IsSuccessStatusCode)
            {
                throwCrmException(getNextPageResponse);

            }
            return (JObject)JsonConvert.DeserializeObject(await getNextPageResponse.Content.ReadAsStringAsync());

        }

        /// <summary>
        /// Executes a batch operation.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="batchId">The batch id.</param>
        /// <param name="callerId" optional="true">The systemuserid of the user to impersonate</param>
        /// <returns></returns>
        public async Task<String> ExecuteBatch(List<HttpContent> payload, Guid batchId, Guid callerId = default(Guid))
        {
            Boolean impersonateUser = callerId != Guid.Empty;

            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "api/data/$batch");
            if (impersonateUser)
            {
                batchRequest.Headers.Add("MSCRMCallerID", callerId.ToString());
            }

            MultipartContent batchContent = new MultipartContent("mixed", "batch_" + batchId.ToString());


            payload.ForEach(p => batchContent.Add(p));

            batchRequest.Content = batchContent;

            //Refresh the access token
            batchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

            HttpResponseMessage batchResponse;

            try
            {
                batchResponse = await httpClient.SendAsync(batchRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.ExecuteBatch method", ex);
            }

            if (!batchResponse.IsSuccessStatusCode)
            {
                throwCrmException(batchResponse);
            }
            return await batchResponse.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Retrieve an array of entities available from the service
        /// </summary>
        /// <returns></returns>
        public async Task<JArray> GetEntityList() {


            HttpRequestMessage getEntityListRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(String.Format("{0}{1}", config.ServiceUrl, "/api/data/")));

    
            //Refresh the access token
            getEntityListRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

   
            HttpResponseMessage getEntityListResponse;

            try
            {
                getEntityListResponse = await httpClient.SendAsync(getEntityListRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error from WebAPIPreviewService.GetEntityList method", ex);
            }

            if (!getEntityListResponse.IsSuccessStatusCode)
            {
                throwCrmException(getEntityListResponse);
            }
            JObject result = (JObject)JsonConvert.DeserializeObject(await getEntityListResponse.Content.ReadAsStringAsync());
            return (JArray)result["value"];
        }

        private static void throwCrmException(HttpResponseMessage resp)
        {
            String responseContent = resp.Content.ReadAsStringAsync().Result;
            JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseContent);

            String message = "Unexpected Error";

            //There are at least two types of error and to differentiate you must 
            //Check whether the object returned has a property of "error" or "Message"
            //This dictionary allows for testing the object using ContainsKey
            IDictionary<string, JToken> responseObjectPropertyDictionary = responseObject;

            if (responseObjectPropertyDictionary.ContainsKey("error"))
            {
                JObject error = (JObject)responseObject.Property("error").Value;
                message = (String)error.Property("message").Value;
            }

            if (responseObjectPropertyDictionary.ContainsKey("Message"))
            {
                message = (String)responseObject.Property("Message").Value;
            }

            throw new Exception(message);
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
        }
    }

}
