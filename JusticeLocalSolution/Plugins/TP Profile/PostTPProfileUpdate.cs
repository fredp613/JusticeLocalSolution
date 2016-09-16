// <copyright file="PostTPProfileUpdate.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>6/27/2016 12:45:43 PM</date>
// <summary>Implements the PostTPProfileUpdate Plugin.</summary>
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
    /// PreTPProfileCreate Plugin.
    /// </summary>    
    public class PostTPProfileUpdate : IPlugin
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
           
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                
                //Entity entity = (Entity)context.InputParameters["Target"];
                Entity postEntity = (Entity)context.PostEntityImages["tpProfile"];
                Entity entity = (Entity)context.InputParameters["Target"];
                //RelationshipValidation rel = new RelationshipValidation(service, entity, ctx);
                //rel.mandatoryRelationshipRecordsMinCountOfOne();
                //if (context.InputParameters.Contains("Relationship") &&
                //(context.InputParameters["Relationship"] is Relationship))
                //{
                    
                   // return;
                //}
                            

                if (entity.LogicalName != "egcs_tp_profile")
                    return;


                //var newRec = entity.Clone(false);
                //service.Create(newRec);

                if (entity.Attributes.Contains("egcs_amtannualplannedbudget") || entity.Attributes.Contains("egcs_datestart") || entity.Attributes.Contains("egcs_dateend"))
                {
                    try
                    {
                        DateTime startDate = postEntity.GetAttributeValue<DateTime>("egcs_datestart");
                        DateTime endDate = postEntity.GetAttributeValue<DateTime>("egcs_dateend");
                        Money amt = postEntity.GetAttributeValue<Money>("egcs_amtannualplannedbudget");
                        AutoGenerateChildren generateBudgetLines = new AutoGenerateChildren(service, entity, ctx);

                        if (!generateBudgetLines.generateAnnualBudgetLines(startDate, endDate, amt))
                        {
                            throw new InvalidPluginExecutionException("An error4 occurred in the TP Profile Update plug-in. Contact administrator", ex1);
                        } 
                    }

                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in the TP Profile plug-in.", ex);
                    }

                    catch (Exception ex)
                    {
                        tracingService.Trace("TP Profile Update Plugin: {0}", ex.ToString());
                        throw;
                    }


                }

            }
        }

    }
}