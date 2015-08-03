// <copyright file="PreValidateContactUpdate.cs" company="">
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author></author>
// <date>8/3/2015 5:38:18 AM</date>
// <summary>Implements the PreValidateContactUpdate Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
namespace CrmILMerge.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Common;

    /// <summary>
    /// PreValidateContactUpdate Plugin.
    /// Fires when the following attributes are updated:
    /// firstname,fullname,lastname,middlename
    /// </summary>    
    public class PreValidateContactUpdate: Plugin
    {
        private readonly string targetEntityAlias = "Target";

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValidateContactUpdate"/> class.
        /// </summary>
        public PreValidateContactUpdate()
            : base(typeof(PreValidateContactUpdate))
        {
            base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(10, "Update", "contact", new Action<LocalPluginContext>(ExecutePreValidateContactUpdate)));

            // Note : you can register for more events here if this plugin is not specific to an individual entity and message combination.
            // You may also need to update your RegisterFile.crmregister plug-in registration file to reflect any change.
        }

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="localContext">The <see cref="LocalPluginContext"/> which contains the
        /// <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationService"/>
        /// and <see cref="ITracingService"/>
        /// </param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        protected void ExecutePreValidateContactUpdate(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }

            // TODO: Implement your custom Plug-in business logic.
            var context = localContext.PluginExecutionContext;
            if (context.InputParameters.Contains(targetEntityAlias) && context.InputParameters[targetEntityAlias] is Entity)
            {
                var target = context.InputParameters[targetEntityAlias] as Entity;

                target["firstname"] = target["firstname"].ToString().RemoveSpaces();
            }
        }
    }
}