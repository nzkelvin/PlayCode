using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;
using Microsoft.Crm.Sdk.Samples.HelperCode;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Configuration;

namespace Microsoft.Crm.Sdk.Samples
{
    class DocGenerator
    {
   

        //values read from app.config
        static String entityTypesFileName;
        static String actionsFileName;
        static String functionsFileName;
        static String enumTypesFileName;
        static String complexTypesFileName;
        static String outputFileType;
        static String outputPath;


        XDocument csdl;
        XDocument relationshipMetadata;
        List<String> entityTypeNames = new List<string>();
        List<String> enumTypeNames = new List<string>();
        List<String> complexTypeNames = new List<string>();
       

        XNamespace edmx = "http://docs.oasis-open.org/odata/ns/edmx";
        XNamespace mscrm = "http://docs.oasis-open.org/odata/ns/edm";

         
        static void Main(string[] args)
        {
            
            try
            {
                entityTypesFileName = ConfigurationManager.AppSettings["EntityTypesFileName"];
                actionsFileName = ConfigurationManager.AppSettings["ActionsFileName"];
                functionsFileName = ConfigurationManager.AppSettings["FunctionsFileName"];
                enumTypesFileName = ConfigurationManager.AppSettings["EnumTypesFileName"];
                complexTypesFileName = ConfigurationManager.AppSettings["ComplexTypesFileName"];
                outputFileType = ConfigurationManager.AppSettings["OutputFileExtension"];
                outputPath = ConfigurationManager.AppSettings["WriteFilesToPath"];


                DocGenerator app = new DocGenerator();
                Console.WriteLine("CRM Web API Preview Documentation Generator Started...");

                // The first argument on the command line is the connection string name.
                String[] arguments = Environment.GetCommandLineArgs();

                // Create a configuration object to store the service URL and app registration settings.
                HelperCode.Configuration config = null;
                if (arguments.Length > 1)
                    config = new HelperCode.Configuration(arguments[1], arguments[0] + ".config");
                else
                    config = new HelperCode.Configuration();


                // Authenticate the user to obtain the OAuth access and refresh tokens.
                Authentication auth = new Authentication(config);

                app.Run(config, auth);
            }
            catch (Exception ex)
            {

                DisplayException(ex);
            }
            finally
            {

                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }

        }

        public void Run(HelperCode.Configuration config, Authentication auth)
        {
            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("Create a directory at '{0}' or modify the outputPath variable in the application to reference an existing directory.", outputPath);
                return;
            }

            try
            {
                //Download the CSDL
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(config.ServiceUrl);
                    httpClient.Timeout = new TimeSpan(0, 2, 0);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                    HttpResponseMessage metadataResponse = httpClient.GetAsync("/api/data/$metadata").Result;
                    LoadOptions options = new LoadOptions();

                    csdl = XDocument.Parse(metadataResponse.Content.ReadAsStringAsync().Result, options);

                    Console.WriteLine("CSDL downloaded from {0}", config.ServiceUrl + "/api/data/$metadata");
                }

                //Retrieve a list of all the names in order to build links between documents.
             entityTypeNames =   getEntityTypesNames();
             complexTypeNames =   getComplexTypesNames();
             enumTypeNames =   getEnumTypesNames();

                //Retrieve information about entity relationships from the application metadata
           relationshipMetadata =    getRelationshipMetadata(entityTypeNames, config, auth);

   
                //Write each page
                writeEntityTypePage();

                writeActionsPage();

                writeFunctionsPage();

                writeEnumsPage();

                writeComplexTypesPage();


            }
            catch (TimeoutException ex) { DisplayException(ex); }

            catch (HttpRequestException ex) { DisplayException(ex); }


        }

        private List<String> getEntityTypesNames()
        {
            List<String> names = new List<string>();

            IEnumerable<XElement> entityTypes =
    from a in csdl.Root.Descendants(mscrm + "EntityType")
    orderby a.Attribute("Name").Value
    select a;

            foreach (XElement a in entityTypes)
            {
                names.Add(a.Attribute("Name").Value);
            }

            return names;

        }
        private List<String> getComplexTypesNames()
        {
            List<String> names = new List<string>();
            
            IEnumerable<XElement> complexTypes =
               from a in csdl.Root.Descendants(mscrm + "ComplexType")
               orderby a.Attribute("Name").Value
               select a;
            //Build a list of links
            foreach (XElement a in complexTypes)
            {

                names.Add(a.Attribute("Name").Value);

            }
            return names;
                  
        }

        private List<String> getEnumTypesNames()
        {
            List<String> names = new List<string>();
          

            IEnumerable<XElement> enumTypes =
               from a in csdl.Root.Descendants(mscrm + "EnumType")
               orderby a.Attribute("Name").Value
               select a;

            foreach (XElement a in enumTypes)
            {

                names.Add(a.Attribute("Name").Value);

            }

            return names;
        }

        private XDocument getRelationshipMetadata(List<String> entityTypeNames, Microsoft.Crm.Sdk.Samples.HelperCode.Configuration config, Authentication auth)
        {
            XDocument relationshipMetadataDocument = new XDocument(new XElement("Metadata"));

            List<String> formattedNames = new List<string>();
            entityTypeNames.ForEach(delegate(String entityName)
            {
                formattedNames.Add(String.Format("<d:string>{0}</d:string>", entityName));
            });


            String content =
String.Format(@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <Execute xmlns=""http://schemas.microsoft.com/xrm/2011/Contracts/Services"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
      <request i:type=""a:RetrieveMetadataChangesRequest"" xmlns:a=""http://schemas.microsoft.com/xrm/2011/Contracts"">
        <a:Parameters xmlns:b=""http://schemas.datacontract.org/2004/07/System.Collections.Generic"">
          <a:KeyValuePairOfstringanyType>
            <b:key>Query</b:key>
            <b:value i:type=""c:EntityQueryExpression"" xmlns:c=""http://schemas.microsoft.com/xrm/2011/Metadata/Query"">
              <c:Criteria>
                <c:Conditions>
                  <c:MetadataConditionExpression>
                    <c:ConditionOperator>In</c:ConditionOperator>
                    <c:PropertyName>LogicalName</c:PropertyName>
                    <c:Value i:type=""d:ArrayOfstring"" xmlns:d=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">{0}</c:Value> 
                  </c:MetadataConditionExpression>
                  <c:MetadataConditionExpression>
                    <c:ConditionOperator>Equals</c:ConditionOperator>
                    <c:PropertyName>IsIntersect</c:PropertyName>
                    <c:Value i:type=""d:boolean"" xmlns:d=""http://www.w3.org/2001/XMLSchema"">false</c:Value>
                  </c:MetadataConditionExpression>
                </c:Conditions>
                <c:FilterOperator>And</c:FilterOperator>
                <c:Filters />
              </c:Criteria>
              <c:Properties>
                <c:AllProperties>false</c:AllProperties>
                <c:PropertyNames xmlns:d=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
                  <d:string>OneToManyRelationships</d:string>
                  <d:string>ManyToManyRelationships</d:string>
                  <d:string>ManyToOneRelationships</d:string>
                </c:PropertyNames>
              </c:Properties>
              <c:AttributeQuery i:nil=""true"" />
              <c:LabelQuery i:nil=""true"" />
              <c:RelationshipQuery>
                <c:Criteria>
                  <c:Conditions />
                  <c:FilterOperator>And</c:FilterOperator>
                  <c:Filters />
                </c:Criteria>
                <c:Properties>
                  <c:AllProperties>false</c:AllProperties>
                  <c:PropertyNames xmlns:d=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
                    <d:string>SchemaName</d:string>
                    <d:string>ReferencingEntity</d:string>
                    <d:string>ReferencingAttribute</d:string>
                    <d:string>ReferencedEntity</d:string>
                    <d:string>ReferencedAttribute</d:string>
                    <d:string>Entity1LogicalName</d:string>
                    <d:string>Entity2LogicalName</d:string>
                    <d:string>IntersectEntityName</d:string>
                  </c:PropertyNames>
                </c:Properties>
              </c:RelationshipQuery>
            </b:value>
          </a:KeyValuePairOfstringanyType>
        </a:Parameters>
        <a:RequestId i:nil=""true"" />
        <a:RequestName>RetrieveMetadataChanges</a:RequestName>
      </request>
    </Execute>
  </s:Body>
</s:Envelope>", String.Join("", formattedNames.ToArray()));

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(config.ServiceUrl);
                httpClient.Timeout = new TimeSpan(0, 4, 0);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AcquireToken().AccessToken);

                try
                {
                    HttpContent httpContent = new StringContent(content, Encoding.UTF8, "text/xml");
                    httpContent.Headers.Add("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");

                    HttpResponseMessage metadataResponse = httpClient.PostAsync("/XRMServices/2011/Organization.svc/web", httpContent).Result;

                    if (metadataResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Metadata Returned");
                        XDocument rawResponse = XDocument.Parse(metadataResponse.Content.ReadAsStringAsync().Result);

                        XNamespace a = "http://schemas.microsoft.com/xrm/2011/Contracts";
                        XNamespace c = "http://schemas.microsoft.com/xrm/2011/Metadata";



                        IEnumerable<XElement> entities =
                        from e in rawResponse.Root.Descendants(a + "EntityMetadata")
                        select e;


                        foreach (XElement entity in entities)
                        {

                            XElement entityNode = new XElement("Entity");
                            entityNode.Add(new XElement("Name", entity.Element(c + "LogicalName").Value));


                            //OneToManyRelationships
                            XElement oneToManyNode = new XElement("OneToManyRelationships");
                            IEnumerable<XElement> OneToManyRelationships =
                            from f in entity.Element(c + "OneToManyRelationships").Descendants(c + "OneToManyRelationshipMetadata")
                            select f;
                            foreach (XElement OneToManyRelationshipMetadata in OneToManyRelationships)
                            {
                                XElement relationshipNode = new XElement("Relationship",
                                new XElement("SchemaName", OneToManyRelationshipMetadata.Element(c + "SchemaName").Value),
                                new XElement("ReferencingEntity", OneToManyRelationshipMetadata.Element(c + "ReferencingEntity").Value),
                                new XElement("ReferencingAttribute", OneToManyRelationshipMetadata.Element(c + "ReferencingAttribute").Value),
                                new XElement("ReferencedEntity", OneToManyRelationshipMetadata.Element(c + "ReferencedEntity").Value),
                                new XElement("ReferencedAttribute", OneToManyRelationshipMetadata.Element(c + "ReferencedAttribute").Value)
                                );

                                oneToManyNode.Add(relationshipNode);
                            }
                            entityNode.Add(oneToManyNode);


                            //ManyToManyRelationships
                            XElement manyToManyNode = new XElement("ManyToManyRelationships");
                            IEnumerable<XElement> ManyToManyRelationships =
                            from f in entity.Element(c + "ManyToManyRelationships").Descendants(c + "ManyToManyRelationshipMetadata")
                            select f;
                            foreach (XElement ManyToManyRelationshipMetadata in ManyToManyRelationships)
                            {
                                XElement relationshipNode = new XElement("Relationship",
                                new XElement("SchemaName", ManyToManyRelationshipMetadata.Element(c + "SchemaName").Value),
                                new XElement("Entity1LogicalName", ManyToManyRelationshipMetadata.Element(c + "Entity1LogicalName").Value),
                                new XElement("Entity2LogicalName", ManyToManyRelationshipMetadata.Element(c + "Entity2LogicalName").Value),
                                new XElement("IntersectEntityName", ManyToManyRelationshipMetadata.Element(c + "IntersectEntityName").Value)
                                );

                                manyToManyNode.Add(relationshipNode);
                            }
                            entityNode.Add(manyToManyNode);

                            //ManyToOneRelationships
                            XElement manyToOneNode = new XElement("ManyToOneRelationships");
                            IEnumerable<XElement> ManyToOneRelationships =
                            from f in entity.Element(c + "ManyToOneRelationships").Descendants(c + "OneToManyRelationshipMetadata")
                            select f;
                            foreach (XElement OneToManyRelationshipMetadata in ManyToOneRelationships)
                            {
                                XElement relationshipNode = new XElement("Relationship",
                                new XElement("SchemaName", OneToManyRelationshipMetadata.Element(c + "SchemaName").Value),
                                new XElement("ReferencingEntity", OneToManyRelationshipMetadata.Element(c + "ReferencingEntity").Value),
                                new XElement("ReferencingAttribute", OneToManyRelationshipMetadata.Element(c + "ReferencingAttribute").Value),
                                new XElement("ReferencedEntity", OneToManyRelationshipMetadata.Element(c + "ReferencedEntity").Value),
                                new XElement("ReferencedAttribute", OneToManyRelationshipMetadata.Element(c + "ReferencedAttribute").Value)
                                );

                                manyToOneNode.Add(relationshipNode);
                            }
                            entityNode.Add(manyToOneNode);

                            relationshipMetadataDocument.Root.Add(entityNode);
                        }



                    }
                    else
                    {
                        Console.WriteLine("Error Retrieving metadata");
                    }
                }
                catch (TimeoutException ex) { DisplayException(ex); }

                catch (HttpRequestException ex) { DisplayException(ex); }
                catch (Exception ex)
                {

                    DisplayException(ex);
                }
            }
            return relationshipMetadataDocument;
        }

        /// <summary>
        /// This method will search through all the possible named items based on the value parameter and return
        /// either a path to the section of the appropriate document or "NO" to indicate that a match was not found.
        /// </summary>
        /// <param name="value">The name of the object</param>
        /// <returns>String</returns>
        private String isLinkable(String value) {
            String hrefValue = "NO";

            //Only values which contain 'mscrm.' need to be checked.
            if (value.StartsWith("mscrm.") || value.StartsWith("Collection(mscrm."))
            {

                if (value.StartsWith("mscrm."))
                {
                   
                    String testValue = value.Replace("mscrm.", "");

                    if (enumTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", enumTypesFileName, outputFileType, testValue);
                    }

                    if (complexTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", complexTypesFileName, outputFileType, testValue);
                    }

                    if (entityTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", entityTypesFileName, outputFileType, testValue);
                    }
                
                }

                if (value.StartsWith("Collection(mscrm."))
                {

                    String testValue = value.Replace("Collection(mscrm.", "").Replace(")", "");

                    if (enumTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", enumTypesFileName, outputFileType, testValue);
                    }

                    if (complexTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", complexTypesFileName, outputFileType, testValue);
                    }

                    if (entityTypeNames.Contains(testValue))
                    {
                        return hrefValue = String.Format("{0}{1}#{2}", entityTypesFileName, outputFileType, testValue);
                    }

                }
               
            }



            
            return hrefValue;
        
        }

        private void writeComplexTypesPage()
        {
            XDocument contentDoc = XDocument.Load("Resources\\ComplexTypesContent.xml");

            String fileLocation = (outputPath == String.Empty) ? (complexTypesFileName + outputFileType) : (outputPath + "\\" + complexTypesFileName + outputFileType);

            using (TextWriter ctp = new StreamWriter(fileLocation))
            {
                ctp.WriteLine("<!DOCTYPE html>");
                ctp.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
                ctp.WriteLine("<head>");
                ctp.WriteLine("    <meta charset=\"utf-8\" />");
                ctp.WriteLine("    <title>Complex Types</title>");
                ctp.Write(@" <style type='text/css'>
  body {
   font-family: 'Segoe UI';
   font-size:11px;
  }

  table, th, td {
   border: 1px solid black;
   border-collapse: collapse;
  }

  th, td {
   padding-left: 5px;
   padding-right: 5px;
  }

  .section {
   border-top: 1px solid black;
   margin-bottom: 10px;
  }
 .LinkListItem {
font-size:16px;
}
 </style>");
                ctp.WriteLine("</head>");
                ctp.WriteLine("<body>");
                

                ctp.WriteLine("<h1>Complex Types</h1>");
                var complexTypeLinkList = new XElement("ul");
                IEnumerable<XElement> complexTypes =
                 from a in csdl.Root.Descendants(mscrm + "ComplexType")
                 orderby a.Attribute("Name").Value
                 select a;
                //Build a list of links
                foreach (XElement a in complexTypes)
                {
                    var listItem = new XElement("li");
                    listItem.Add(new XElement("a", a.Attribute("Name").Value, new XAttribute("href", "#" + a.Attribute("Name").Value), new XAttribute("class", "LinkListItem")));                    
                    complexTypeLinkList.Add(listItem);
                }
                ctp.Write(complexTypeLinkList);

                // Documentation for each Complex Type
                foreach (XElement a in complexTypes)
                {

                    String ComplexTypeDescription = "TODO";
                    XElement PropertyDescriptions = null;

                    XElement Remarks = null;
                    XElement Samples = null;
                    XElement Links = null;
                    //Get Content Node
                    IEnumerable<XElement> complexTypeContent =
                    from ac in contentDoc.Root.Descendants("ComplexType")
                    where (String)ac.Attribute("Name").Value == (String)a.Attribute("Name").Value
                    select ac;
                    if (complexTypeContent.Count() > 0)
                    {
                        ComplexTypeDescription = complexTypeContent.First().Descendants("Description").First().Value;
                        PropertyDescriptions = complexTypeContent.First().Descendants("Properties").First();


                        if (complexTypeContent.First().Descendants("Remarks").First().Descendants().Count() > 0)
                        {
                            Remarks = complexTypeContent.First().Descendants("Remarks").First();
                        }
                        if (complexTypeContent.First().Descendants("Samples").First().Descendants().Count() > 0)
                        {
                            Samples = complexTypeContent.First().Descendants("Samples").First();
                        }
                        if (complexTypeContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = complexTypeContent.First().Descendants("Links").First();
                        }



                    }


                    ctp.WriteLine("<div class=\"section\" >");
                    ctp.WriteLine("<a name=\"{0}\" id=\"{0}\" ></a>", a.Attribute("Name").Value);
                    ctp.WriteLine(new XElement("h2", a.Attribute("Name").Value + " ComplexType"));
                    ctp.WriteLine("<p>" + ComplexTypeDescription + "</p>");
                    ctp.WriteLine("<h3>Properties</h3>");

                    if (a.Descendants(mscrm + "Property").Count() == 0)
                    {
                        ctp.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not have any properties.</p>");
                    }
                    else
                    {
                        var table = new XElement("table");
                        var headerRow = new XElement("tr");
                        var nameHeader = new XElement("th", "Name");
                        var typeHeader = new XElement("th", "Type");
                        var nullableHeader = new XElement("th", "Nullable");
                        var unicodeHeader = new XElement("th", "Unicode");
                        var descriptionHeader = new XElement("th", "Description");
                        headerRow.Add(nameHeader);
                        headerRow.Add(typeHeader);
                        headerRow.Add(nullableHeader);
                        headerRow.Add(unicodeHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        IEnumerable<XElement> properties =
                        from p in a.Descendants(mscrm + "Property")
                        select p;
                        foreach (XElement p in properties)
                        {
                            String PropertyDescription = "TODO";
                            if (PropertyDescriptions != null)
                            {
                                IEnumerable<XElement> propertyDescriptions =
                                from pd in PropertyDescriptions.Descendants("Property")
                                where (String)pd.Attribute("Name").Value == (String)p.Attribute("Name").Value
                                select pd;
                                if (propertyDescriptions.Count() > 0)
                                {
                                    PropertyDescription = propertyDescriptions.First().Value;
                                }
                            }
                            var typeValue = p.Attribute("Type").Value;
                            var row = new XElement("tr");
                            var nameColumn = new XElement("td", p.Attribute("Name").Value);
                           
                            var typeColumn = new XElement("td", typeValue);
                            var linkValue = isLinkable(typeValue);
                            if (linkValue != "NO")
                            {
                                if (typeValue.StartsWith("Collection(mscrm."))
                                {
                                    typeColumn = new XElement("td", new XText("Collection("), new XElement("a", new XAttribute("href", linkValue), new XText(typeValue.Replace("Collection(", "").Replace(")", ""))), new XText(")"));
                                }
                                else
                                {
                                    typeColumn = new XElement("td", new XElement("a", new XAttribute("href", linkValue), typeValue));
                                }
                            }

                             


                            var nullableColumn = new XElement("td", (p.Attribute("Nullable") == null) ? "true" : p.Attribute("Nullable").Value);
                            var unicodeColumn = new XElement("td", (p.Attribute("Unicode") == null) ? "true" : p.Attribute("Unicode").Value);
                            var descriptionColumn = new XElement("td", PropertyDescription);
                            row.Add(nameColumn);
                            row.Add(typeColumn);
                            row.Add(nullableColumn);
                            row.Add(unicodeColumn);
                            row.Add(descriptionColumn);
                            table.Add(row);
                        }
                        ctp.Write(table);
                    }

                    if (Remarks != null)
                    {
                        ctp.WriteLine("<h3>Remarks</h3>");
                        foreach (XElement para in Remarks.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ctp.WriteLine(para.ToString());
                            }

                        }
                    }
                    if (Samples != null)
                    {
                        ctp.WriteLine("<h3>Samples</h3>");
                        IEnumerable<XElement> samples =
                        from s in Samples.Descendants("Sample")
                        select s;
                        foreach (XElement sample in samples)
                        {
                            ctp.WriteLine("<h4>" + sample.Descendants("Title").First().Value + "</h4>");
                            foreach (XElement para in sample.Descendants("Description").Elements())
                            {
                                if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                                {
                                    ctp.WriteLine(para);
                                }
                            }

                            foreach (XElement snippet in sample.Descendants("Code"))
                            {
                                String platform;
                                String language;
                                switch (snippet.Attribute("platform").Value)
                                {
                                    case "dotNET":
                                        platform = ".NET";
                                        break;
                                    case "WebResource":
                                        platform = "CRM web resource";
                                        break;
                                    default:
                                        platform = snippet.Attribute("platform").Value;
                                        break;
                                }

                                switch (snippet.Attribute("language").Value)
                                {
                                    //In case we want to map attributes to different values
                                    default:
                                        language = snippet.Attribute("language").Value;
                                        break;
                                }
                                XElement table = new XElement("table");
                                XElement heading = new XElement("thead");
                                XElement headingRow = new XElement("tr");
                                XElement headingCell = new XElement("th");
                                XElement platformP = new XElement("p", String.Format("Platform: {0}", platform));
                                XElement languageP = new XElement("p", String.Format("Language: {0}", language));
                                headingCell.Add(platformP);
                                headingCell.Add(languageP);
                                headingRow.Add(headingCell);
                                heading.Add(headingRow);
                                table.Add(heading);
                                XElement body = new XElement("tbody");
                                XElement bodyRow = new XElement("tr");
                                XElement bodyCell = new XElement("td");
                                XElement code = new XElement("pre", snippet.Value);
                                bodyCell.Add(code);
                                bodyRow.Add(bodyCell);
                                body.Add(bodyRow);
                                table.Add(body);
                                ctp.WriteLine(table);



                            }

                        }

                    }
                    if (Links != null)
                    {
                        ctp.WriteLine("<h3>Links</h3>");
                        foreach (XElement para in Links.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ctp.WriteLine(para.ToString());
                            }

                        }
                    }
                    ctp.WriteLine("<br />");
                    ctp.WriteLine("<a href='#'>Complex Types List</a><span>&nbsp;&nbsp;&nbsp;</span><a href='http://go.microsoft.com/fwlink/?LinkID=522513'>Microsoft Dynamics CRM Web API Preview</a>");
                    ctp.WriteLine("</div>");
                }
                ctp.WriteLine("</body>");
                ctp.WriteLine("</html>");
                ctp.Flush();
                Console.WriteLine(String.Format("ComplexTypes document written to {0}", fileLocation));
            }
        }

        private void writeEnumsPage()
        {
            XDocument contentDoc = XDocument.Load("Resources\\EnumTypesContent.xml");

            String fileLocation = (outputPath == String.Empty) ? (enumTypesFileName + outputFileType) : (outputPath + "\\" + enumTypesFileName + outputFileType);

            using (TextWriter etp = new StreamWriter(fileLocation))
            {
                etp.WriteLine("<!DOCTYPE html>");
                etp.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
                etp.WriteLine("<head>");
                etp.WriteLine("    <meta charset=\"utf-8\" />");
                etp.WriteLine("    <title>Enum Types</title>");
                etp.Write(@" <style type='text/css'>
  body {
   font-family: 'Segoe UI';
   font-size:11px;
  }

  table, th, td {
   border: 1px solid black;
   border-collapse: collapse;
  }

  th, td {
   padding-left: 5px;
   padding-right: 5px;
  }

  .section {
   border-top: 1px solid black;
   margin-bottom: 10px;
  }
 .LinkListItem {
font-size:16px;
}
 </style>");
                etp.WriteLine("</head>");
                etp.WriteLine("<body>");
               

                etp.WriteLine("<h1>Enum Types</h1>");
                var enumTypeLinkList = new XElement("ul");
                IEnumerable<XElement> enumTypes =
                 from a in csdl.Root.Descendants(mscrm + "EnumType")
                 orderby a.Attribute("Name").Value
                 select a;
                //Build a list of links
                foreach (XElement a in enumTypes)
                {
                    var listItem = new XElement("li");
                    listItem.Add(new XElement("a", a.Attribute("Name").Value, new XAttribute("href", "#" + a.Attribute("Name").Value), new XAttribute("class", "LinkListItem")));
                    enumTypeLinkList.Add(listItem);
                }
                etp.Write(enumTypeLinkList);

                // Documentation for each Action
                foreach (XElement a in enumTypes)
                {

                    String EnumTypeDescription = "TODO";
                    XElement MemberDescriptions = null;

                    XElement Remarks = null;
                    XElement Samples = null;
                    XElement Links = null;
                    //Get Content Node
                    IEnumerable<XElement> enumTypeContent =
                    from ac in contentDoc.Root.Descendants("EnumType")
                    where (String)ac.Attribute("Name").Value == (String)a.Attribute("Name").Value
                    select ac;
                    if (enumTypeContent.Count() > 0)
                    {
                        EnumTypeDescription = enumTypeContent.First().Descendants("Description").First().Value;
                        MemberDescriptions = enumTypeContent.First().Descendants("Members").First();


                        if (enumTypeContent.First().Descendants("Remarks").First().Descendants().Count() > 0)
                        {
                            Remarks = enumTypeContent.First().Descendants("Remarks").First();
                        }
                        if (enumTypeContent.First().Descendants("Samples").First().Descendants().Count() > 0)
                        {
                            Samples = enumTypeContent.First().Descendants("Samples").First();
                        }
                        if (enumTypeContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = enumTypeContent.First().Descendants("Links").First();
                        }



                    }

                    etp.WriteLine("<div class=\"section\" >");
                    etp.WriteLine("<a name=\"{0}\" id=\"{0}\" ></a>", a.Attribute("Name").Value);
                    etp.WriteLine(new XElement("h2", a.Attribute("Name").Value + " EnumType"));
                    etp.WriteLine("<p>" + EnumTypeDescription + "</p>");
                    etp.WriteLine("<h3>Members</h3>");

                    if (a.Descendants(mscrm + "Member").Count() == 0)
                    {
                        etp.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not have any members.</p>");
                    }
                    else
                    {
                        var table = new XElement("table");
                        var headerRow = new XElement("tr");
                        var nameHeader = new XElement("th", "Name");
                        var valueHeader = new XElement("th", "Value");
                        var descriptionHeader = new XElement("th", "Description");
                        headerRow.Add(nameHeader);
                        headerRow.Add(valueHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        IEnumerable<XElement> members =
                        from m in a.Descendants(mscrm + "Member")
                        select m;
                        foreach (XElement m in members)
                        {
                            String MemberDescription = "TODO";
                            if (MemberDescriptions != null)
                            {
                                IEnumerable<XElement> memberDescriptions =
                                from md in MemberDescriptions.Descendants("Member")
                                where (String)md.Attribute("Name").Value == (String)m.Attribute("Name").Value
                                select md;
                                if (memberDescriptions.Count() > 0)
                                {
                                    MemberDescription = memberDescriptions.First().Value;
                                }
                            }

                            var row = new XElement("tr");
                            var nameColumn = new XElement("td", m.Attribute("Name").Value);
                            var valueColumn = new XElement("td", m.Attribute("Value").Value);

                            var descriptionColumn = new XElement("td", MemberDescription);
                            row.Add(nameColumn);
                            row.Add(valueColumn);
                            row.Add(descriptionColumn);
                            table.Add(row);
                        }
                        etp.Write(table);
                    }

                    if (Remarks != null)
                    {
                        etp.WriteLine("<h3>Remarks</h3>");
                        foreach (XElement para in Remarks.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                etp.WriteLine(para.ToString());
                            }

                        }
                    }
                    if (Samples != null)
                    {
                        etp.WriteLine("<h3>Samples</h3>");
                        IEnumerable<XElement> samples =
                        from s in Samples.Descendants("Sample")
                        select s;
                        foreach (XElement sample in samples)
                        {
                            etp.WriteLine("<h4>" + sample.Descendants("Title").First().Value + "</h4>");
                            foreach (XElement para in sample.Descendants("Description").Elements())
                            {
                                if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                                {
                                    etp.WriteLine(para);
                                }
                            }

                            foreach (XElement snippet in sample.Descendants("Code"))
                            {
                                String platform;
                                String language;
                                switch (snippet.Attribute("platform").Value)
                                {
                                    case "dotNET":
                                        platform = ".NET";
                                        break;
                                    case "WebResource":
                                        platform = "CRM web resource";
                                        break;
                                    default:
                                        platform = snippet.Attribute("platform").Value;
                                        break;
                                }

                                switch (snippet.Attribute("language").Value)
                                {
                                    //In case we want to map attributes to different values
                                    default:
                                        language = snippet.Attribute("language").Value;
                                        break;
                                }
                                XElement table = new XElement("table");
                                XElement heading = new XElement("thead");
                                XElement headingRow = new XElement("tr");
                                XElement headingCell = new XElement("th");
                                XElement platformP = new XElement("p", String.Format("Platform: {0}", platform));
                                XElement languageP = new XElement("p", String.Format("Language: {0}", language));
                                headingCell.Add(platformP);
                                headingCell.Add(languageP);
                                headingRow.Add(headingCell);
                                heading.Add(headingRow);
                                table.Add(heading);
                                XElement body = new XElement("tbody");
                                XElement bodyRow = new XElement("tr");
                                XElement bodyCell = new XElement("td");
                                XElement code = new XElement("pre", snippet.Value);
                                bodyCell.Add(code);
                                bodyRow.Add(bodyCell);
                                body.Add(bodyRow);
                                table.Add(body);
                                etp.WriteLine(table);



                            }

                        }

                    }
                    if (Links != null)
                    {
                        etp.WriteLine("<h3>Links</h3>");
                        foreach (XElement para in Links.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                etp.WriteLine(para.ToString());
                            }

                        }
                    }
                    etp.WriteLine("<br />");
                    etp.WriteLine("<a href='#'>Enum Types List</a><span>&nbsp;&nbsp;&nbsp;</span><a href='http://go.microsoft.com/fwlink/?LinkID=522513'>Microsoft Dynamics CRM Web API Preview</a>");
                    etp.WriteLine("</div>");
                }
                etp.WriteLine("</body>");
                etp.WriteLine("</html>");
                etp.Flush();
                Console.WriteLine(String.Format("EnumTypes document written to {0}", fileLocation));
            }
        }

        private void writeFunctionsPage()
        {
            XDocument contentDoc = XDocument.Load("Resources\\FunctionsContent.xml");

            String fileLocation = (outputPath == String.Empty) ? (functionsFileName + outputFileType) : (outputPath + "\\" + functionsFileName + outputFileType);

            using (TextWriter ap = new StreamWriter(fileLocation))
            {
                ap.WriteLine("<!DOCTYPE html>");
                ap.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
                ap.WriteLine("<head>");
                ap.WriteLine("    <meta charset=\"utf-8\" />");
                ap.WriteLine("    <title>UnBound Functions</title>");
                ap.Write(@" <style type='text/css'>
  body {
   font-family: 'Segoe UI';
   font-size:11px;
  }

  table, th, td {
   border: 1px solid black;
   border-collapse: collapse;
  }

  th, td {
   padding-left: 5px;
   padding-right: 5px;
  }

  .section {
   border-top: 1px solid black;
   margin-bottom: 10px;
  }
 .LinkListItem {
font-size:16px;
}
 </style>");
                ap.WriteLine("</head>");
                ap.WriteLine("<body>");


                ap.WriteLine("<h1>UnBound Functions</h1>");
                var actionLinkList = new XElement("ul");
                IEnumerable<XElement> functions =
                 from a in csdl.Root.Descendants(mscrm + "Function")
                 where a.Attribute("IsBound") == null //Unbound functions only
                 orderby a.Attribute("Name").Value
                 select a;
                //Build a list of links
                foreach (XElement a in functions)
                {
                    var listItem = new XElement("li");
                    listItem.Add(new XElement("a", a.Attribute("Name").Value, new XAttribute("href", "#" + a.Attribute("Name").Value), new XAttribute("class", "LinkListItem")));
                    actionLinkList.Add(listItem);
                }
                ap.Write(actionLinkList);

                // Documentation for each Function
                foreach (XElement a in functions)
                {

                    String FunctionDescription = "TODO";
                    XElement ParameterDescriptions = null;
                    String ReturnTypeDescription = null;
                    XElement Remarks = null;
                    XElement Samples = null;
                    XElement Links = null;
                    //Get Content Node
                    IEnumerable<XElement> functionContent =
                    from ac in contentDoc.Root.Descendants("Function")
                    where (String)ac.Attribute("Name").Value == (String)a.Attribute("Name").Value
                    select ac;
                    if (functionContent.Count() > 0)
                    {
                        FunctionDescription = functionContent.First().Descendants("Description").First().Value;
                        ParameterDescriptions = functionContent.First().Descendants("Parameters").First();
                        if (functionContent.First().Descendants("ReturnType").Count() > 0)
                        {
                            ReturnTypeDescription = functionContent.First().Descendants("ReturnType").First().Value;
                        }

                        if (functionContent.First().Descendants("Remarks").First().Descendants().Count() > 0)
                        {
                            Remarks = functionContent.First().Descendants("Remarks").First();
                        }
                        if (functionContent.First().Descendants("Samples").First().Descendants().Count() > 0)
                        {
                            Samples = functionContent.First().Descendants("Samples").First();
                        }
                        if (functionContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = functionContent.First().Descendants("Links").First();
                        }

                    }


                    ap.WriteLine("<div class=\"section\" >");
                    ap.WriteLine("<a name=\"{0}\" id=\"{0}\" ></a>", a.Attribute("Name").Value);
                    ap.WriteLine(new XElement("h2", a.Attribute("Name").Value + " Function"));
                    ap.WriteLine("<p>" + FunctionDescription + "</p>");
                    ap.WriteLine("<h3>Parameters</h3>");

                    if (a.Descendants(mscrm + "Parameter").Count() == 0)
                    {
                        ap.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not have any parameters.</p>");
                    }
                    else
                    {
                        var table = new XElement("table");
                        var headerRow = new XElement("tr");
                        var nameHeader = new XElement("th", "Name");
                        var typeHeader = new XElement("th", "Type");
                        var nullableHeader = new XElement("th", "Nullable");
                        var unicodeHeader = new XElement("th", "Unicode");
                        var descriptionHeader = new XElement("th", "Description");
                        headerRow.Add(nameHeader);
                        headerRow.Add(typeHeader);
                        headerRow.Add(nullableHeader);
                        headerRow.Add(unicodeHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        IEnumerable<XElement> parameters =
                        from p in a.Descendants(mscrm + "Parameter")
                        select p;
                        foreach (XElement p in parameters)
                        {
                            String ParameterDescription = "TODO";
                            if (ParameterDescriptions != null)
                            {
                                IEnumerable<XElement> parameterDescriptions =
                                from pd in ParameterDescriptions.Descendants("Parameter")
                                where (String)pd.Attribute("Name").Value == (String)p.Attribute("Name").Value
                                select pd;
                                if (parameterDescriptions.Count() > 0)
                                {
                                    ParameterDescription = parameterDescriptions.First().Value;
                                }
                            }

                            var row = new XElement("tr");
                            var nameColumn = new XElement("td", p.Attribute("Name").Value);

                          

                            String typeValue = p.Attribute("Type").Value;
                            var typeColumn = new XElement("td", typeValue);
                            var linkValue = isLinkable(typeValue);
                            if (linkValue != "NO")
                            {


                                if (typeValue.StartsWith("Collection(mscrm."))
                                {
                                    typeColumn = new XElement("td", new XText("Collection("), new XElement("a", new XAttribute("href", linkValue), new XText(typeValue.Replace("Collection(", "").Replace(")", ""))), new XText(")"));
                                }
                                else
                                {
                                    typeColumn = new XElement("td", new XElement("a", new XAttribute("href", linkValue), typeValue));
                                }

                                
                            }


                            var nullableColumn = new XElement("td", (p.Attribute("Nullable") == null) ? "true" : p.Attribute("Nullable").Value);
                            var unicodeColumn = new XElement("td", (p.Attribute("Unicode") == null) ? "true" : p.Attribute("Unicode").Value);
                            var descriptionColumn = new XElement("td", ParameterDescription);
                            row.Add(nameColumn);
                            row.Add(typeColumn);
                            row.Add(nullableColumn);
                            row.Add(unicodeColumn);
                            row.Add(descriptionColumn);
                            table.Add(row);
                        }
                        ap.Write(table);
                    }
                    ap.WriteLine("<h3>ReturnType</h3>");
                    if (a.Descendants(mscrm + "ReturnType").Count() == 0)
                    {
                        ap.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not return a value.</p>");
                    }
                    else
                    {
                        XElement returnNode = a.Descendants(mscrm + "ReturnType").First();
                        var table = new XElement("table");
                        var headerRow = new XElement("tr");
                        var typeHeader = new XElement("th", "Type");
                        var nullableHeader = new XElement("th", "Nullable");
                        var descriptionHeader = new XElement("th", "Description");
                        headerRow.Add(typeHeader);
                        headerRow.Add(nullableHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        var row = new XElement("tr");

                        

                        String typeValue = returnNode.Attribute("Type").Value;
                        var typeColumn = new XElement("td", typeValue);
                        var linkValue = isLinkable(typeValue);
                        if (linkValue != "NO")
                        {
                            if (typeValue.StartsWith("Collection(mscrm."))
                            {
                                typeColumn = new XElement("td", new XText("Collection("), new XElement("a", new XAttribute("href", linkValue), new XText(typeValue.Replace("Collection(", "").Replace(")", ""))), new XText(")"));
                            }
                            else
                            {
                                typeColumn = new XElement("td", new XElement("a", new XAttribute("href", linkValue), typeValue));
                            }
                        }


                        var nullableColumn = new XElement("td", (returnNode.Attribute("Nullable") == null) ? "true" : returnNode.Attribute("Nullable").Value);
                        var descriptionColumn = new XElement("td", (ReturnTypeDescription == null) ? "TODO" : ReturnTypeDescription);


                        row.Add(typeColumn);
                        row.Add(nullableColumn);
                        row.Add(descriptionColumn);

                        table.Add(row);
                        ap.Write(table);
                    }
                    if (Remarks != null)
                    {
                        ap.WriteLine("<h3>Remarks</h3>");
                        foreach (XElement para in Remarks.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ap.WriteLine(para.ToString());
                            }

                        }
                    }
                    if (Samples != null)
                    {
                        ap.WriteLine("<h3>Samples</h3>");
                        IEnumerable<XElement> samples =
                        from s in Samples.Descendants("Sample")
                        select s;
                        foreach (XElement sample in samples)
                        {
                            ap.WriteLine("<h4>" + sample.Descendants("Title").First().Value + "</h4>");
                            foreach (XElement para in sample.Descendants("Description").Elements())
                            {
                                if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                                {
                                    ap.WriteLine(para);
                                }
                            }

                            foreach (XElement snippet in sample.Descendants("Code"))
                            {
                                String platform;
                                String language;
                                switch (snippet.Attribute("platform").Value)
                                {
                                    case "dotNET":
                                        platform = ".NET";
                                        break;
                                    case "WebResource":
                                        platform = "CRM web resource";
                                        break;
                                    default:
                                        platform = snippet.Attribute("platform").Value;
                                        break;
                                }

                                switch (snippet.Attribute("language").Value)
                                {
                                    //In case we want to map attributes to different values
                                    default:
                                        language = snippet.Attribute("language").Value;
                                        break;
                                }
                                XElement table = new XElement("table");
                                XElement heading = new XElement("thead");
                                XElement headingRow = new XElement("tr");
                                XElement headingCell = new XElement("th");
                                XElement platformP = new XElement("p", String.Format("Platform: {0}", platform));
                                XElement languageP = new XElement("p", String.Format("Language: {0}", language));
                                headingCell.Add(platformP);
                                headingCell.Add(languageP);
                                headingRow.Add(headingCell);
                                heading.Add(headingRow);
                                table.Add(heading);
                                XElement body = new XElement("tbody");
                                XElement bodyRow = new XElement("tr");
                                XElement bodyCell = new XElement("td");
                                XElement code = new XElement("pre", snippet.Value);
                                bodyCell.Add(code);
                                bodyRow.Add(bodyCell);
                                body.Add(bodyRow);
                                table.Add(body);
                                ap.WriteLine(table);



                            }

                        }

                    }
                    if (Links != null)
                    {
                        ap.WriteLine("<h3>Links</h3>");
                        foreach (XElement para in Links.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ap.WriteLine(para.ToString());
                            }

                        }
                    }
                    ap.WriteLine("<br />");
                    ap.WriteLine("<a href='#'>Unbound Functions List</a><span>&nbsp;&nbsp;&nbsp;</span><a href='http://go.microsoft.com/fwlink/?LinkID=522513'>Microsoft Dynamics CRM Web API Preview</a>");
                    ap.WriteLine("</div>");
                }
                ap.WriteLine("</body>");
                ap.WriteLine("</html>");
                ap.Flush();
                Console.WriteLine(String.Format("Functions document written to {0}", fileLocation));
            }

        }

        private void writeActionsPage()
        {
            XDocument contentDoc = XDocument.Load("Resources\\ActionsContent.xml");

            String fileLocation = (outputPath == String.Empty) ? (actionsFileName + outputFileType) : (outputPath + "\\" + actionsFileName + outputFileType);
            using (TextWriter ap = new StreamWriter(fileLocation))
            {
                ap.WriteLine("<!DOCTYPE html>");
                ap.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
                ap.WriteLine("<head>");
                ap.WriteLine("    <meta charset=\"utf-8\" />");
                ap.WriteLine("    <title>Unbound Actions</title>");
                ap.Write(@" <style type='text/css'>
  body {
   font-family: 'Segoe UI';
   font-size:11px;
  }

  table, th, td {
   border: 1px solid black;
   border-collapse: collapse;
  }

  th, td {
   padding-left: 5px;
   padding-right: 5px;
  }

  .section {
   border-top: 1px solid black;
   margin-bottom: 10px;
  }
 .LinkListItem {
font-size:16px;
}
 </style>");
                ap.WriteLine("</head>");
                ap.WriteLine("<body>");


                ap.WriteLine("<h1>Unbound Actions</h1>");
                var actionLinkList = new XElement("ul");
                IEnumerable<XElement> actions =
                 from a in csdl.Root.Descendants(mscrm + "Action")
                 orderby a.Attribute("Name").Value
                 select a;
                //Build a list of links
                foreach (XElement a in actions)
                {
                    var listItem = new XElement("li");
                    listItem.Add(new XElement("a", a.Attribute("Name").Value, new XAttribute("href", "#" + a.Attribute("Name").Value), new XAttribute("class", "LinkListItem")));
                    actionLinkList.Add(listItem);
                }
                ap.Write(actionLinkList);

                // Documentation for each Action
                foreach (XElement a in actions)
                {

                    String ActionDescription = "TODO";
                    XElement ParameterDescriptions = null;
                    String ReturnTypeDescription = null;
                    XElement Remarks = null;
                    XElement Samples = null;
                    XElement Links = null;
                    //Get Content Node
                    IEnumerable<XElement> actionContent =
                    from ac in contentDoc.Root.Descendants("Action")
                    where (String)ac.Attribute("Name").Value == (String)a.Attribute("Name").Value
                    select ac;
                    if (actionContent.Count() > 0)
                    {
                        ActionDescription = actionContent.First().Descendants("Description").First().Value;
                        ParameterDescriptions = actionContent.First().Descendants("Parameters").First();
                        if (actionContent.First().Descendants("ReturnType").Count() > 0)
                        {
                            ReturnTypeDescription = actionContent.First().Descendants("ReturnType").First().Value;
                        }

                        if (actionContent.First().Descendants("Remarks").First().Descendants().Count() > 0)
                        {
                            Remarks = actionContent.First().Descendants("Remarks").First();
                        }
                        if (actionContent.First().Descendants("Samples").First().Descendants().Count() > 0)
                        {
                            Samples = actionContent.First().Descendants("Samples").First();
                        }
                        if (actionContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = actionContent.First().Descendants("Links").First();
                        }


                    }

                    ap.WriteLine("<div class=\"section\" >");
                    ap.WriteLine("<a name=\"{0}\" id=\"{0}\" ></a>", a.Attribute("Name").Value);
                    ap.WriteLine(new XElement("h2", a.Attribute("Name").Value + " Action"));
                    ap.WriteLine("<p>" + ActionDescription + "</p>");
                    ap.WriteLine("<h3>Parameters</h3>");

                    if (a.Descendants(mscrm + "Parameter").Count() == 0)
                    {
                        ap.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not have any parameters.</p>");
                    }
                    else
                    {
                        var table = new XElement("table", new XAttribute("style", "border:1px solid black; border-collapse:collapse;"));

                        var headerRow = new XElement("tr");
                        var nameHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Name");
                        var typeHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Type");
                        var nullableHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Nullable");
                        var unicodeHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Unicode");
                        var descriptionHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Description");
                        headerRow.Add(nameHeader);
                        headerRow.Add(typeHeader);
                        headerRow.Add(nullableHeader);
                        headerRow.Add(unicodeHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        IEnumerable<XElement> parameters =
                        from p in a.Descendants(mscrm + "Parameter")
                        select p;
                        foreach (XElement p in parameters)
                        {
                            String ParameterDescription = "TODO";
                            if (ParameterDescriptions != null)
                            {
                                IEnumerable<XElement> parameterDescriptions =
                                from pd in ParameterDescriptions.Descendants("Parameter")
                                where (String)pd.Attribute("Name").Value == (String)p.Attribute("Name").Value
                                select pd;
                                if (parameterDescriptions.Count() > 0)
                                {
                                    ParameterDescription = parameterDescriptions.First().Value;
                                }
                            }

                            var row = new XElement("tr");
                            var nameColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), p.Attribute("Name").Value);
                         

                            String typeValue = p.Attribute("Type").Value;
                            var typeColumn = new XElement("td", typeValue);
                            var linkValue = isLinkable(typeValue);
                            if (linkValue != "NO")
                            {
                                
                                if (typeValue.StartsWith("Collection(mscrm."))
                                {
                                    typeColumn = new XElement("td", new XText("Collection("), new XElement("a", new XAttribute("href", linkValue), new XText(typeValue.Replace("Collection(", "").Replace(")", ""))),new XText(")"));
                                }
                                else
                                {
                                    typeColumn = new XElement("td", new XElement("a", new XAttribute("href", linkValue), typeValue));
                                }

                            }



                            var nullableColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), (p.Attribute("Nullable") == null) ? "true" : p.Attribute("Nullable").Value);
                            var unicodeColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), (p.Attribute("Unicode") == null) ? "true" : p.Attribute("Unicode").Value);
                            var descriptionColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), ParameterDescription);
                            row.Add(nameColumn);
                            row.Add(typeColumn);
                            row.Add(nullableColumn);
                            row.Add(unicodeColumn);
                            row.Add(descriptionColumn);
                            table.Add(row);
                        }
                        ap.Write(table);
                    }
                    ap.WriteLine("<h3>ReturnType</h3>");
                    if (a.Descendants(mscrm + "ReturnType").Count() == 0)
                    {
                        ap.WriteLine("<p><b>" + a.Attribute("Name").Value + "</b> does not return a value.</p>");
                    }
                    else
                    {
                        XElement returnNode = a.Descendants(mscrm + "ReturnType").First();
                        var table = new XElement("table", new XAttribute("style", "border:1px solid black; border-collapse:collapse;"));
                        var headerRow = new XElement("tr");
                        var typeHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Type");
                        var nullableHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Nullable");
                        var descriptionHeader = new XElement("th", new XAttribute("style", "border:1px solid black;"), "Description");
                        headerRow.Add(typeHeader);
                        headerRow.Add(nullableHeader);
                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        var row = new XElement("tr");
                     

                        String typeValue = returnNode.Attribute("Type").Value;
                        var typeColumn = new XElement("td", typeValue);
                        var linkValue = isLinkable(typeValue);
                        if (linkValue != "NO")
                        {
                            if (typeValue.StartsWith("Collection(mscrm."))
                            {
                                typeColumn = new XElement("td", new XText("Collection("), new XElement("a", new XAttribute("href", linkValue), new XText(typeValue.Replace("Collection(", "").Replace(")", ""))), new XText(")"));
                            }
                            else
                            {
                                typeColumn = new XElement("td", new XElement("a", new XAttribute("href", linkValue), typeValue));
                            }
                        }


                        var nullableColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), (returnNode.Attribute("Nullable") == null) ? "true" : returnNode.Attribute("Nullable").Value);
                        var descriptionColumn = new XElement("td", new XAttribute("style", "border:1px solid black;"), (ReturnTypeDescription == null) ? "TODO" : ReturnTypeDescription);


                        row.Add(typeColumn);
                        row.Add(nullableColumn);
                        row.Add(descriptionColumn);

                        table.Add(row);
                        ap.Write(table);
                    }
                    if (Remarks != null)
                    {
                        ap.WriteLine("<h3>Remarks</h3>");
                        foreach (XElement para in Remarks.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ap.WriteLine(para.ToString());
                            }

                        }
                    }
                    if (Samples != null)
                    {
                        ap.WriteLine("<h3>Samples</h3>");
                        IEnumerable<XElement> samples =
                        from s in Samples.Descendants("Sample")
                        select s;
                        foreach (XElement sample in samples)
                        {
                            ap.WriteLine("<h4>" + sample.Descendants("Title").First().Value + "</h4>");
                            foreach (XElement para in sample.Descendants("Description").Elements())
                            {
                                if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                                {
                                    ap.WriteLine(para);
                                }
                            }

                            foreach (XElement snippet in sample.Descendants("Code"))
                            {
                                String platform;
                                String language;
                                switch (snippet.Attribute("platform").Value)
                                {
                                    case "dotNET":
                                        platform = ".NET";
                                        break;
                                    case "WebResource":
                                        platform = "CRM web resource";
                                        break;
                                    default:
                                        platform = snippet.Attribute("platform").Value;
                                        break;
                                }

                                switch (snippet.Attribute("language").Value)
                                {
                                    //In case we want to map attributes to different values
                                    default:
                                        language = snippet.Attribute("language").Value;
                                        break;
                                }
                                var table = new XElement("table", new XAttribute("style", "border:1px solid black; border-collapse:collapse;"));
                                XElement heading = new XElement("thead");
                                XElement headingRow = new XElement("tr");
                                XElement headingCell = new XElement("th", new XAttribute("style", "border:1px solid black;"));
                                XElement platformP = new XElement("p", String.Format("Platform: {0}", platform));
                                XElement languageP = new XElement("p", String.Format("Language: {0}", language));
                                headingCell.Add(platformP);
                                headingCell.Add(languageP);
                                headingRow.Add(headingCell);
                                heading.Add(headingRow);
                                table.Add(heading);
                                XElement body = new XElement("tbody");
                                XElement bodyRow = new XElement("tr");
                                XElement bodyCell = new XElement("td", new XAttribute("style", "border:1px solid black;"));
                                XElement code = new XElement("pre", snippet.Value);
                                bodyCell.Add(code);
                                bodyRow.Add(bodyCell);
                                body.Add(bodyRow);
                                table.Add(body);
                                ap.WriteLine(table);



                            }

                        }

                    }

                    if (Links != null)
                    {
                        ap.WriteLine("<h3>Links</h3>");
                        foreach (XElement para in Links.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                ap.WriteLine(para.ToString());
                            }

                        }
                    }

                    ap.WriteLine("<br />");
                    ap.WriteLine("<a href='#'>Unbound Actions List</a><span>&nbsp;&nbsp;&nbsp;</span><a href='http://go.microsoft.com/fwlink/?LinkID=522513'>Microsoft Dynamics CRM Web API Preview</a>");
                    ap.WriteLine("</div>");
                }
                ap.WriteLine("</body>");
                ap.WriteLine("</html>");
                ap.Flush();
                Console.WriteLine(String.Format("Actions document written to {0}", fileLocation));
            }
        }

        private void writeEntityTypePage()
        {
            XDocument contentDoc = XDocument.Load("Resources\\EntityTypesContent.xml");

            String fileLocation = (outputPath == String.Empty) ? (entityTypesFileName + outputFileType) : (outputPath + "\\" + entityTypesFileName + outputFileType);
            using (TextWriter etp = new StreamWriter(fileLocation))
            {
                etp.WriteLine("<!DOCTYPE html>");
                etp.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
                etp.WriteLine("<head>");
                etp.WriteLine("    <meta charset=\"utf-8\" />");
                etp.WriteLine("    <title>Entity Types</title>");
                etp.Write(@" <style type='text/css'>
  body {
   font-family: 'Segoe UI';
   font-size:11px;
  }

  table, th, td {
   border: 1px solid black;
   border-collapse: collapse;
  }

  th, td {
   padding-left: 5px;
   padding-right: 5px;
  }

  .section {
   border-top: 1px solid black;
   margin-bottom: 10px;
  }
 .LinkListItem {
font-size:16px;
}
 </style>");
                etp.WriteLine("</head>");
                etp.WriteLine("<body>");


                etp.WriteLine("<h1>Entity Types</h1>");
                var entityTypeLinkList = new XElement("ul");



                IEnumerable<XElement> entityTypes =
                 from a in csdl.Root.Descendants(mscrm + "EntityType")
                 orderby a.Attribute("Name").Value
                 select a;
                //Build a list of links
                foreach (XElement a in entityTypes)
                {
                    String entityName = a.Attribute("Name").Value;
                    var listItem = new XElement("li");
                    listItem.Add(new XElement("a", entityName, new XAttribute("href", "#" + entityName), new XAttribute("class", "LinkListItem")));
                    entityTypeLinkList.Add(listItem);
                }
                etp.Write(entityTypeLinkList);

                // Documentation for each Entity
                foreach (XElement a in entityTypes)
                {
                    String entityName = a.Attribute("Name").Value;
                    Boolean isActivity = (a.Attribute("BaseType") != null && a.Attribute("BaseType").Value == "mscrm.activitypointer");

                    //Need to get these from Annotations
                    String EntityTypeDescription = "NotFoundInAnnotations";
                    String EntityTypeDisplayName = "NotFoundInAnnotations";



                    IEnumerable<XElement> entityContainer =
               from ec in csdl.Root.Descendants(mscrm + "EntityContainer").Elements(mscrm + "EntitySet")
               where ec.Attribute("EntityType").Value == "mscrm." + entityName
               select ec;

                    String entitypath = null;
                    try
                    {
                        entitypath = "[organization URI]/api/data/" + entityContainer.First().Attribute("Name").Value;
                    }
                    catch (Exception)
                    {
                        //let entitypath remain null
                    }

                    if (entityName == "crmbaseentity")
                    {
                        EntityTypeDescription = "An abstract base entity which all entities inherit from.";
                        EntityTypeDisplayName = "(none)";
                    }
                    else
                    {
                        IEnumerable<XElement> entityDescriptions =
                   from ed in csdl.Root.Descendants(mscrm + "Annotations")
                   where ed.Attribute("Target").Value == "mscrm." + entityName
                   select ed;
                        foreach (XElement annotation in entityDescriptions.Elements(mscrm + "Annotation"))
                        {

                            if (annotation.Attribute("Term").Value == "Org.OData.Core.V1.Description")
                            {
                                EntityTypeDescription = annotation.Attribute("String").Value;
                            }
                            if (annotation.Attribute("Term").Value == "mscrm.SchemaDisplayName")
                            {
                                EntityTypeDisplayName = annotation.Attribute("String").Value;
                            }
                        }

                    }


                    XElement Remarks = null;
                    XElement Samples = null;
                    XElement Links = null;
                    // Get Content
                    IEnumerable<XElement> entityTypeContent =
                    from ac in contentDoc.Root.Descendants("EntityType")
                    where (String)ac.Attribute("Name").Value == entityName
                    select ac;
                    if (entityTypeContent.Count() > 0)
                    {


                        if (entityTypeContent.First().Descendants("Remarks").First().Descendants().Count() > 0)
                        {
                            Remarks = entityTypeContent.First().Descendants("Remarks").First();
                        }
                        if (entityTypeContent.First().Descendants("Samples").First().Descendants().Count() > 0)
                        {
                            Samples = entityTypeContent.First().Descendants("Samples").First();
                        }
                        if (entityTypeContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = entityTypeContent.First().Descendants("Links").First();
                        }
                        if (entityTypeContent.First().Descendants("Links").First().Descendants().Count() > 0)
                        {
                            Links = entityTypeContent.First().Descendants("Links").First();
                        }

                    }

                    etp.WriteLine("<div class=\"section\" >");
                    etp.WriteLine("<a name=\"{0}\" id=\"{0}\" ></a>", a.Attribute("Name").Value);
                    etp.WriteLine(new XElement("h2", entityName + " Entity"));
                    if (a.Attribute("Abstract") != null)
                    {
                        etp.WriteLine("<p>Abstract entity</p>");
                    }
                    if (entityName != "crmbaseentity")
                    {
                        String baseType = a.Attribute("BaseType").Value.Replace("mscrm.", "");
                        etp.WriteLine("<p><b>Base Type</b>: <a href=\"#" + baseType + "\">" + baseType + "</a></p>");
                    }
                    etp.WriteLine("<p>" + EntityTypeDescription + "</p>");
                    if (entitypath != null)
                    {
                        etp.WriteLine("<p><b>Collection URL</b>: " + entitypath + "</p>");
                    }

                    etp.WriteLine("<p><b>Display Name</b>: " + EntityTypeDisplayName + "</p>");
                    String primarykey = String.Empty;
                    if (a.Elements(mscrm + "Key").Count() == 1) //crmbaseentity doesn't have a key, multiple keys not supported for Web API preview
                    {
                        primarykey = a.Elements(mscrm + "Key").First().Elements(mscrm + "PropertyRef").First().Attribute("Name").Value;
                        etp.WriteLine("<p><b>PrimaryKey</b>: " + primarykey + "</p>");
                    }

                    etp.WriteLine("<ul>");
                    etp.WriteLine("<li><a href=\"#" + entityName + "properties\" >" + entityName + " Properties</a></li>");
                    etp.WriteLine("<li><a href=\"#" + entityName + "lookups\" >" + entityName + " Single-Valued Navigation Properties</a></li>");
                    etp.WriteLine("<li><a href=\"#" + entityName + "collections\" >" + entityName + " Collection-Valued  Navigation Properties</a></li>");
                    etp.WriteLine("<li><a href=\"#" + entityName + "functions\" >" + entityName + " Functions</a></li>");
                    if (Remarks != null)
                    {
                        etp.WriteLine("<li><a href=\"#" + entityName + "remarks\" >Remarks</a></li>");
                    }
                    if (Samples != null)
                    {
                        etp.WriteLine("<li><a href=\"#" + entityName + "samples\" >Samples</a></li>");
                    }
                    etp.WriteLine("</ul>");

                    etp.WriteLine("<h3 id=\"" + entityName + "properties\" >" + entityName + " Properties</h3>");

                    if (a.Descendants(mscrm + "Property").Count() == 0)
                    {
                        //This should never happen except for abstract entities
                        etp.WriteLine("<p><b>" + entityName + "</b> does not have any properties.</p>");
                    }
                    else
                    {
                        XElement table = new XElement("table");
                        XElement headerRow = new XElement("tr");
                        XElement nameHeader = new XElement("th", "Name");
                        XElement displayNameHeader = new XElement("th", "Display Name");
                        XElement typeHeader = new XElement("th", "Type");
                        XElement computedHeader = new XElement("th", "Computed");
                        XElement permissionHeader = new XElement("th", "Permission");
                        XElement descriptionHeader = new XElement("th", "Description");
                        headerRow.Add(nameHeader);
                        headerRow.Add(displayNameHeader);
                        headerRow.Add(typeHeader);
                        headerRow.Add(computedHeader);
                        headerRow.Add(permissionHeader);

                        headerRow.Add(descriptionHeader);
                        table.Add(headerRow);

                        if (isActivity)
                        {
                            IEnumerable<XElement> combinedEntity =
                       from ce in csdl.Root.Descendants(mscrm + "EntityType")
                       where ce.Attribute("Name").Value.Equals("activitypointer") ||
                       ce.Attribute("Name").Value.Equals(entityName)
                       select ce;

                            IEnumerable<XElement> properties =
                            from p in combinedEntity.Descendants(mscrm + "Property")
                            orderby p.Attribute("Name").Value
                            select p;


                            foreach (XElement p in properties)
                            {
                                String PropertyName = p.Attribute("Name").Value;
                                String PropertyDescription = "TODO";
                                String PropertyDisplayName = "TODO";
                                String PropertyComputed = "TODO";
                                String PropertyPermissions = "TODO";

                                IEnumerable<XElement> propertyDescriptions =
                           from ed in csdl.Root.Descendants(mscrm + "Annotations")
                           where ed.Attribute("Target").Value == "mscrm." + entityName + "/" + PropertyName ||
                           ed.Attribute("Target").Value == "mscrm.activitypointer/" + PropertyName
                           select ed;
                                foreach (XElement annotation in propertyDescriptions.Elements(mscrm + "Annotation"))
                                {
                                    switch (annotation.Attribute("Term").Value)
                                    {
                                        case "Org.OData.Core.V1.Description":
                                            PropertyDescription = annotation.Attribute("String").Value;
                                            break;
                                        case "mscrm.SchemaDisplayName":
                                            PropertyDisplayName = annotation.Attribute("String").Value;
                                            break;
                                        case "Org.OData.Core.V1.Computed":
                                            PropertyComputed = annotation.Attribute("Bool").Value;
                                            break;
                                        case "Org.OData.Core.V1.Permissions":
                                            //Assuming that there can be only one EnumMember child element
                                            var enumMember = annotation.Elements(mscrm + "EnumMember").First();
                                            PropertyPermissions = enumMember.Value.Replace("Org.OData.Core.V1.PermissionType/", "");
                                            break;
                                        default:
                                            break;
                                    }

                                }


                                XElement row = new XElement("tr");

                                XElement nameDataCell;
                                if(PropertyName == primarykey)
                                {
                                    nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + PropertyName), new XElement("strong", String.Format("{0} (PK)",PropertyName)));
                                }
                                else{
                               nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + PropertyName), PropertyName);
                                }
                               
                                XElement displayNameDataCell = new XElement("td", PropertyDisplayName);
                                XElement typeDataCell = new XElement("td", p.Attribute("Type").Value);
                                XElement computedDataCell = new XElement("td", PropertyComputed);
                                XElement permissionsDataCell = new XElement("td", PropertyPermissions);

                                XElement descriptionDataCell = new XElement("td", PropertyDescription);
                                row.Add(nameDataCell);
                                row.Add(displayNameDataCell);
                                row.Add(typeDataCell);
                                row.Add(computedDataCell);
                                row.Add(permissionsDataCell);

                                row.Add(descriptionDataCell);
                                table.Add(row);
                            }

                        }
                        else
                        {
                            //NOT an activity
                            IEnumerable<XElement> properties =
                            from p in a.Descendants(mscrm + "Property")
                            orderby p.Attribute("Name").Value
                            select p;

                            foreach (XElement p in properties)
                            {
                                String PropertyName = p.Attribute("Name").Value;
                                String PropertyDescription = "TODO";
                                String PropertyDisplayName = "TODO";
                                String PropertyComputed = "TODO";
                                String PropertyPermissions = "TODO";



                                IEnumerable<XElement> propertyDescriptions =
                           from ed in csdl.Root.Descendants(mscrm + "Annotations")
                           where ed.Attribute("Target").Value == "mscrm." + entityName + "/" + PropertyName
                           select ed;
                                foreach (XElement annotation in propertyDescriptions.Elements(mscrm + "Annotation"))
                                {
                                    switch (annotation.Attribute("Term").Value)
                                    {
                                        case "Org.OData.Core.V1.Description":
                                            PropertyDescription = annotation.Attribute("String").Value;
                                            break;
                                        case "mscrm.SchemaDisplayName":
                                            PropertyDisplayName = annotation.Attribute("String").Value;
                                            break;
                                        case "Org.OData.Core.V1.Computed":
                                            PropertyComputed = annotation.Attribute("Bool").Value;
                                            break;
                                        case "Org.OData.Core.V1.Permissions":
                                            //Assuming that there can be only one EnumMember child element
                                            var enumMember = annotation.Elements(mscrm + "EnumMember").First();
                                            PropertyPermissions = enumMember.Value.Replace("Org.OData.Core.V1.PermissionType/", "");
                                            break;
                                        default:
                                            break;
                                    }

                                }


                                XElement row = new XElement("tr");
                               // XElement nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + PropertyName), PropertyName);

                                XElement nameDataCell;
                                if (PropertyName == primarykey)
                                {
                                    nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + PropertyName), new XElement("strong", String.Format("{0} (PK)", PropertyName)));
                                }
                                else
                                {
                                    nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + PropertyName), PropertyName);
                                }


                                XElement displayNameDataCell = new XElement("td", PropertyDisplayName);
                                XElement typeDataCell = new XElement("td", p.Attribute("Type").Value);
                                XElement computedDataCell = new XElement("td", PropertyComputed);
                                XElement permissionsDataCell = new XElement("td", PropertyPermissions);

                                XElement descriptionDataCell = new XElement("td", PropertyDescription);
                                row.Add(nameDataCell);
                                row.Add(displayNameDataCell);
                                row.Add(typeDataCell);
                                row.Add(computedDataCell);
                                row.Add(permissionsDataCell);

                                row.Add(descriptionDataCell);
                                table.Add(row);
                            }
                        }

                        etp.Write(table);
                    }


                    IEnumerable<XElement> navProps =
                from np in a.Elements(mscrm + "NavigationProperty")
                orderby np.Attribute("Name").Value
                select np;

                    //Lookups
                    etp.WriteLine("<h3 id=\"" + entityName + "lookups\" >" + entityName + " Single-Valued  Navigation Properties</h3>");

                    Boolean NoLookups = true;

                    XElement lTable = new XElement("table");
                    XElement lHeaderRow = new XElement("tr");
                    XElement lNameHeader = new XElement("th", "Name");
                    XElement lTypeHeader = new XElement("th", "Type");
                    XElement lPartnerHeader = new XElement("th", "Self-referential Partner");
                    XElement lDescriptionHeader = new XElement("th", "Description");
                    lHeaderRow.Add(lNameHeader);
                    lHeaderRow.Add(lTypeHeader);
                    lHeaderRow.Add(lPartnerHeader);
                    lHeaderRow.Add(lDescriptionHeader);
                    lTable.Add(lHeaderRow);

                    IEnumerable<XElement> entityLookups =
                     from rd in relationshipMetadata.Root.Descendants("Entity")
                     where rd.Element("Name").Value.Equals(entityName)
                     select rd.Element("ManyToOneRelationships");

                    foreach (XElement np in navProps)
                    {



                        String rawType = np.Attribute("Type").Value;
                        if (!(rawType.Length > "Collection".Length && rawType.Substring(0, "Collection".Length) == "Collection"))
                        {
                            String cName = np.Attribute("Name").Value;
                            String testName;
                            if (cName.StartsWith("Referencing"))
                            {
                                testName = cName.Substring("Referencing".Length);

                            }
                            else
                            {
                                testName = cName;
                            }
                            String readProperty = String.Empty;

                            foreach (XElement rel in entityLookups.Elements("Relationship"))
                            {
                                if (rel.Element("SchemaName").Value == testName)
                                {
                                    readProperty = rel.Element("ReferencingAttribute").Value;
                                }
                            }
                            String description = String.Empty;
                            if (readProperty != String.Empty)
                            {

                                IEnumerable<XElement> propertyDescriptions =
                               from ed in csdl.Root.Descendants(mscrm + "Annotations")
                               where ed.Attribute("Target").Value == "mscrm." + entityName + "/" + readProperty
                               select ed;
                                foreach (XElement annotation in propertyDescriptions.Elements(mscrm + "Annotation"))
                                {
                                    switch (annotation.Attribute("Term").Value)
                                    {
                                        case "Org.OData.Core.V1.Description":
                                            description = annotation.Attribute("String").Value;
                                            break;
                                    
                                        default:
                                            break;
                                    }

                                }
                            
                            }



                            String cType = rawType.Replace("mscrm.", "").Replace(")", "");
                            String cPartner = np.Attribute("Partner").Value;

                            XElement row = new XElement("tr");
                            XElement nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + cName), cName);
                          
                            XElement typeDataCell = new XElement("td", new XText("mscrm."), new XElement("a", new XAttribute("href", "#" + cType), cType));
                            XElement partnerDataCell = new XElement("td", (cName == cPartner) ? "(none)" : cPartner);
                            String formattedDescription = String.Empty;
                            if (!(readProperty == String.Empty && description == String.Empty))
                            {
                                
                                if (readProperty != String.Empty && description != String.Empty)
                                {
                                    formattedDescription = String.Format("Read Property: {0} {1}", readProperty, description);
                                }
                                else
                                {
                                    if (readProperty == String.Empty)
                                    {
                                        formattedDescription = description;
                                    }
                                    if (description == String.Empty)
                                    {
                                        formattedDescription = String.Format("Read Property: {0}", readProperty);
                                    } 
                                }
                            }
                            XElement descriptionDataCell = new XElement("td", formattedDescription);
                            row.Add(nameDataCell);
                         
                            row.Add(typeDataCell);
                            row.Add(partnerDataCell);
                            row.Add(descriptionDataCell);
                            lTable.Add(row);
                            NoLookups = false;
                        }
                    }
                    if (NoLookups)
                    {
                        etp.WriteLine("<b>" + entityName + "</b> has no single-valued navigation properties.");
                    }
                    else
                    {
                        etp.Write(lTable);
                    }


                    //Collections
                    etp.WriteLine("<h3 id=\"" + entityName + "collections\" >" + entityName + " Collection-Valued  Navigation Properties</h3>");

                    Boolean NoCollections = true;

                    XElement cTable = new XElement("table");
                    XElement cHeaderRow = new XElement("tr");
                    XElement cNameHeader = new XElement("th", "Name");
                    XElement cTypeHeader = new XElement("th", "Type");
                    XElement cPartnerHeader = new XElement("th", "Self-referential Partner");
                    XElement cDescription = new XElement("th", "Description");
                    cHeaderRow.Add(cNameHeader);
                    cHeaderRow.Add(cTypeHeader);
                    cHeaderRow.Add(cPartnerHeader);
                    cHeaderRow.Add(cDescription);
                    cTable.Add(cHeaderRow);

                    foreach (XElement np in navProps)
                    {
                        String rawType = np.Attribute("Type").Value;
                        if (rawType.Length > "Collection".Length && rawType.Substring(0, "Collection".Length) == "Collection")
                        {
                            String cName = np.Attribute("Name").Value;
                            String cType = rawType.Replace("Collection(mscrm.", "").Replace(")", "");
                            String cPartner = np.Attribute("Partner").Value;
                            String Description = String.Empty;

                            IEnumerable<XElement> entityOneToManyRelationships =
                     from rd in relationshipMetadata.Root.Descendants("Entity")
                     where rd.Element("Name").Value.Equals(entityName)
                     select rd.Element("OneToManyRelationships");

                            String readProperty = String.Empty;


                            foreach (XElement rel in entityOneToManyRelationships.Elements("Relationship"))
                            {
                                if (rel.Element("SchemaName").Value == ((cName.StartsWith("Referenced") ? (cName.Substring("Referenced".Length)) : cName))) 
                                {
                                    readProperty = rel.Element("ReferencingAttribute").Value;
                                    String descriptionText = String.Empty;

                                    IEnumerable<XElement> relationshipPropertyDescription =
                              from ed in csdl.Root.Descendants(mscrm + "Annotations")
                              where ed.Attribute("Target").Value == "mscrm." + cType + "/" + readProperty ||
                           ed.Attribute("Target").Value == "mscrm.activitypointer/" + readProperty
                              select ed;
                                    foreach (XElement annotation in relationshipPropertyDescription.Elements(mscrm + "Annotation"))
                                    {
                                        switch (annotation.Attribute("Term").Value)
                                        {
                                            case "Org.OData.Core.V1.Description":
                                                descriptionText = annotation.Attribute("String").Value;
                                                break;

                                            default:
                                                break;
                                        }

                                    }

                                    Description = String.Format("Read Property: {0}.{1} {2}", cType, readProperty,descriptionText);
                                }
                            }
                            if (Description == String.Empty && cName.EndsWith("_association"))
                            {
                                Description = String.Format("Many-to-Many relationship with {0}", cType);
                            }


                            XElement row = new XElement("tr");
                            XElement nameDataCell = new XElement("td", new XAttribute("id", entityName + "-" + cName), cName);
                            XElement typeDataCell = new XElement("td", "Collection(mscrm.", new XElement("a", new XAttribute("href", "#" + cType), cType), ")");
                            XElement partnerDataCell = new XElement("td", (cName == cPartner) ? "(none)" : cPartner);
                            XElement descriptionDataCell = new XElement("td", Description);
                            row.Add(nameDataCell);
                            row.Add(typeDataCell);
                            row.Add(partnerDataCell);
                            row.Add(descriptionDataCell);
                            cTable.Add(row);
                            NoCollections = false;
                        }
                    }
                    if (NoCollections)
                    {
                        etp.WriteLine("<b>" + entityName + "</b> has no collection-valued navigation properties.");
                    }
                    else
                    {
                        etp.Write(cTable);
                    }


                    //Functions
                    etp.WriteLine("<h3 id=\"" + entityName + "functions\" >" + entityName + " Functions</h3>");

                    IEnumerable<XElement> boundFunctions =
               from bf in csdl.Root.Descendants(mscrm + "Function")
               where (bf.Attribute("IsBound") != null)
               orderby bf.Attribute("Name").Value
               select bf;

                    XElement bfTable = new XElement("table");
                    XElement bfHeaderRow = new XElement("tr");
                    XElement bfNameHeader = new XElement("th", "Name");
                    XElement bfParameterNameHeader = new XElement("th", "Parameter Name");
                    XElement bfReturnTypeHeader = new XElement("th", "Return Type");
                    bfHeaderRow.Add(bfNameHeader);
                    bfHeaderRow.Add(bfParameterNameHeader);
                    bfHeaderRow.Add(bfReturnTypeHeader);
                    bfTable.Add(bfHeaderRow);

                    Boolean boundFunctionsFound = false;
                    foreach (XElement bf in boundFunctions)
                    {
                        String boundType = bf.Element(mscrm + "Parameter").Attribute("Type").Value.Replace("Collection(mscrm.", "").Replace(")", "");
                        if (entityName == boundType)
                        {
                            String cType = bf.Element(mscrm + "ReturnType").Attribute("Type").Value.Replace("Collection(mscrm.", "").Replace(")", "");
                            XElement row = new XElement("tr");
                            XElement nameDataCell = new XElement("td", String.Format("mscrm.{0}", bf.Attribute("Name").Value));
                            XElement parameterNameDataCell = new XElement("td", bf.Element(mscrm + "Parameter").Attribute("Name").Value);
                            XElement returnTypeDataCell = new XElement("td", "Collection(mscrm.", new XElement("a", new XAttribute("href", "#" + cType), cType), ")");


                            row.Add(nameDataCell);
                            row.Add(parameterNameDataCell);
                            row.Add(returnTypeDataCell);
                            bfTable.Add(row);
                            boundFunctionsFound = true;
                        }

                    }
                    if (boundFunctionsFound)
                    {
                        etp.Write(bfTable);
                    }
                    else
                    {
                        etp.WriteLine("<p><b>{0}</b> does not have any bound functions.</p>", entityName);
                    }

                    //Remarks
                    if (Remarks != null)
                    {
                        etp.WriteLine("<h3 id=\"" + entityName + "remarks\" >" + entityName + " Remarks</h3>");
                        foreach (XElement para in Remarks.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                etp.WriteLine(para.ToString());
                            }

                        }
                    }
                    //Samples
                    if (Samples != null)
                    {
                        etp.WriteLine("<h3 id=\"" + entityName + "samples\" >" + entityName + " Samples</h3>");
                        IEnumerable<XElement> samples =
                        from s in Samples.Descendants("Sample")
                        select s;
                        foreach (XElement sample in samples)
                        {
                            etp.WriteLine("<h4>" + sample.Descendants("Title").First().Value + "</h4>");
                            foreach (XElement para in sample.Descendants("Description").Elements())
                            {
                                if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                                {
                                    etp.WriteLine(para);
                                }
                            }

                            foreach (XElement snippet in sample.Descendants("Code"))
                            {
                                String platform;
                                String language;
                                switch (snippet.Attribute("platform").Value)
                                {
                                    case "dotNET":
                                        platform = ".NET";
                                        break;
                                    case "WebResource":
                                        platform = "CRM web resource";
                                        break;
                                    default:
                                        platform = snippet.Attribute("platform").Value;
                                        break;
                                }

                                switch (snippet.Attribute("language").Value)
                                {
                                    //In case we want to map attributes to different values
                                    default:
                                        language = snippet.Attribute("language").Value;
                                        break;
                                }
                                XElement table = new XElement("table");
                                XElement heading = new XElement("thead");
                                XElement headingRow = new XElement("tr");
                                XElement headingCell = new XElement("th");
                                XElement platformP = new XElement("p", String.Format("Platform: {0}", platform));
                                XElement languageP = new XElement("p", String.Format("Language: {0}", language));
                                headingCell.Add(platformP);
                                headingCell.Add(languageP);
                                headingRow.Add(headingCell);
                                heading.Add(headingRow);
                                table.Add(heading);
                                XElement body = new XElement("tbody");
                                XElement bodyRow = new XElement("tr");
                                XElement bodyCell = new XElement("td");
                                XElement code = new XElement("pre", snippet.Value);
                                bodyCell.Add(code);
                                bodyRow.Add(bodyCell);
                                body.Add(bodyRow);
                                table.Add(body);
                                etp.WriteLine(table);



                            }

                        }

                    }
                    if (Links != null)
                    {
                        etp.WriteLine("<h3>Links</h3>");
                        foreach (XElement para in Links.Elements())
                        {
                            if (para.Name == "p" || para.Name == "ul" || para.Name == "ol" || para.Name == "table")
                            {
                                etp.WriteLine(para.ToString());
                            }

                        }
                    }
                    etp.WriteLine("<br />");
                    etp.WriteLine("<a href='#'>Entity Types List</a><span>&nbsp;&nbsp;&nbsp;</span><a href='http://go.microsoft.com/fwlink/?LinkID=522513'>Microsoft Dynamics CRM Web API Preview</a>");
                    etp.WriteLine("</div>");
                }
                etp.WriteLine("</body>");
                etp.WriteLine("</html>");
                etp.Flush();
                Console.WriteLine(String.Format("Entity Types document written to {0}", fileLocation));
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
