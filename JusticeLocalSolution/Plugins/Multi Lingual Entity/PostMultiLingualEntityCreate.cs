// <copyright file="PostTPEligibleActivityCreate.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>7/15/2016 3:28:46 PM</date>
// <summary>Implements the PostTPEligibleActivityCreate Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
namespace JusticeLocalSolution.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Discovery;
    using Microsoft.Xrm.Sdk.Messages;
    using System.Collections.Generic;
    using System.Linq;
    using Lib.TPProfile;
    using Lib.Common;
    using FredPearson.Common;
    using fp.MultiLingual.Lib;

    /// <summary>
    /// PostTPProfileCreate Plugin.
    /// </summary>    
    public class PostMultiLingualEntityCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext ctx = new OrganizationServiceContext(service);
            FaultException ex1 = new FaultException();
          
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName != "fp_multilingualentity")
                    return;

                try
                {
                    var entityForPlugin = entity.GetAttributeValue<string>("fp_name").ToString();

                    if (!new MultiLingual(service, tracingService, entityForPlugin).generatePluginSteps())
                    {
                        tracingService.Trace("WTF--------------");
                        throw new InvalidPluginExecutionException("something went wrong", ex1);
                    }
                    tracingService.Trace("PASS--------------");

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    tracingService.Trace("NO PASS--------------");
                    throw new InvalidPluginExecutionException("An error occurred in the multi lingual entity plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Multi Lingual Entity Plugin: {0}", ex.ToString());
                    throw;
                }

            }


        }
    }
}
