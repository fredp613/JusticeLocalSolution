// <copyright file="PreValidateTPObjectiveDelete.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>7/15/2016 12:19:59 PM</date>
// <summary>Implements the PreValidateTPObjectiveDelete Plugin.</summary>
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

    /// <summary>
    /// PostTPProfileCreate Plugin.
    /// </summary>    
    public class PreValidateTPProfileTPObjectiveDelete : IPlugin
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



            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is EntityReference)
            {

                // Obtain the target entity from the input parameters.

                EntityReference entityRef = (EntityReference)context.InputParameters["Target"];

                if (entityRef.LogicalName != "egcs_tp_profile_tp_objective")
                    return;

                try
                {
                    Entity entity1 = (Entity)context.PreEntityImages["parent_profile"];
                    if (entity1 != null)
                    {
                        if (entity1.Contains("egcs_tp_profile"))
                        {
                            RelationshipValidation validator = new RelationshipValidation(service, null, entityRef, entity1.GetAttributeValue<EntityReference>("egcs_tp_profile").Id, ctx);

                            if (!validator.singleMandatoryRelationshipRecordsMinCountOfOne("jus_yesnohasatleastoneobjective"))
                            {
                                throw new InvalidPluginExecutionException("There must be at least 1 record for active program", ex1);
                            }
                        }
                    }
                    

                   

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the Pre Validation TP Objective plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("TP Objective Plugin: {0}", ex.ToString());
                    throw;
                }

            }


        }
    }
}