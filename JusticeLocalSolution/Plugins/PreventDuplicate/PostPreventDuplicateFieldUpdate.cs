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
    using fp.PreventDuplicate.Common;

    /// <summary>
    /// PostTPProfileCreate Plugin.
    /// </summary>    
    public class PostPreventDuplicateFieldUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext ctx = new OrganizationServiceContext(service);
            FaultException ex1 = new FaultException();
            //throw new InvalidPluginExecutionException("asdfsd", ex1);


            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                Entity postEntity = (Entity)context.PostEntityImages["fp_name"];

                if (entity.LogicalName != "fp_preventduplicatefield")
                    return;

                try
                {
                    var entityForPlugin = postEntity.GetAttributeValue<string>("fp_name").ToString();
                       

                    if (!new PreventDuplicate(service, entityForPlugin).destroyPluginSteps())
                    {
                        throw new InvalidPluginExecutionException("something went wrong (delete)", ex1);
                    }
                    else
                    {
                        if (!new PreventDuplicate(service, entityForPlugin).generatePluginSteps())
                        {
                            throw new InvalidPluginExecutionException("something went wrong", ex1);
                        }
                    }


                    

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the prevent duplicate plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("prevent duplicate Plugin: {0}", ex.ToString());
                    throw;
                }

            }


        }
    }
}
