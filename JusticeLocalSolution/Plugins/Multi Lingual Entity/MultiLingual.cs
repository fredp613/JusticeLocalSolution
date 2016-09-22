﻿using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;
using JusticeLocalSolution.Plugins.Lib.Common;
using Microsoft.Xrm.Sdk.Metadata;
using fp.MultiLingual.Lib;

namespace JusticeLocalSolution.Plugins
{
    public class PostMultilingualRetrieveHelper : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            FaultException ex1 = new FaultException();
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            try
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                Entity outputEntity = (Entity)context.OutputParameters["BusinessEntity"];
               // var entityRef = (EntityReference)context.InputParameters["Target"];               

                if (outputEntity != null)
                {
                    if (context.Depth > 1) return;
                    //get current entity inside this context
                    QueryExpression qe = new QueryExpression(outputEntity.LogicalName);
                    qe.Criteria.AddCondition(outputEntity.LogicalName + "id", ConditionOperator.Equal, outputEntity.Id);
                    qe.ColumnSet = new ColumnSet(true);
                    Entity entity = service.RetrieveMultiple(qe).Entities[0];

                    var translator = new MultiLingual(service, tracingService, context, outputEntity, entity);
                    translator.translateOutput();



                }

                

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the multi lingual plug-in.", ex);
            }
        }
        

    }

    public class PostMultiLingualRetrieveMultipleHelper : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            FaultException ex1 = new FaultException();
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            try
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                EntityCollection targets = (EntityCollection)context.OutputParameters["BusinessEntityCollection"];

                var lang = new UserLocaleHelper(service, context).getUserLanguage();

                if (context.Depth <= 1)
                {
                    foreach (Entity outputEntity in targets.Entities)
                    {
                        //query the entity get columns
                        Entity entity = service.Retrieve(outputEntity.LogicalName, outputEntity.Id, new ColumnSet("egcs_nameen", "egcs_namefr"));
                        //e["egcs_name"] = entity.GetAttributeValue<string>("egcs_namefr");
                        var translator = new MultiLingual(service, tracingService, context, outputEntity, entity);
                        translator.translateOutput();
                    }

                }             

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the multi lingual plug-in.", ex);
            }

        }

    }

    public class PostMultiLingualUpdateHelper : IPlugin
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
                if (context.Depth > 1) { return; }
                
                Entity entity = (Entity)context.InputParameters["Target"];
                var entityName = entity.LogicalName;
                string nameToUse;
                string engName;
                string frName;

                MetadataHelper meta = new MetadataHelper(service, entity);
                var publisherPrefix = entityName.Split('_')[0];
                int? nameFieldLength;
                if (publisherPrefix == entityName)
                {
                    nameFieldLength = meta.getMaxLength("name");
                    nameToUse = "name";
                    engName = "nameen";
                    frName = "namefr";
                }
                else
                {
                    nameFieldLength = meta.getMaxLength(publisherPrefix + "_name");
                    nameToUse = publisherPrefix + "_name";
                    engName = publisherPrefix + "_nameen";
                    frName = publisherPrefix + "_namefr";
                }

                if (entity.Attributes.Contains(engName) || entity.Attributes.Contains(frName))
                {                    
                    try
                    {

                        if (nameFieldLength != null)
                        {
                           
                            var currentEntity = service.Retrieve(entityName, entity.Id, new ColumnSet(publisherPrefix + "_nameen", publisherPrefix + "_namefr"));
                            var frenchName = currentEntity.GetAttributeValue<string>(frName);
                            var englishName = currentEntity.GetAttributeValue<string>(engName);

                            StringHelper strHelper = new StringHelper();
                            var newStr = strHelper.concatenateAfterCalc(englishName, frenchName, nameFieldLength.GetValueOrDefault(), " / ");
                           
                            entity[nameToUse] = newStr;

                            if (context.MessageName.ToUpper() == "UPDATE")
                            {

                                if (context.Depth > 1) { return; }
                                service.Update(entity);
                            }

                            
                        }
                      
                    }
                    catch(Exception ex)
                    {
                        tracingService.Trace("Multi Lingual Helper Plugin: {0}", ex.ToString());
                        throw;
                    }
                }

            }
        }
    }

    public class PreMultiLingualCreateHelper : IPlugin
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
                Entity entity = (Entity)context.InputParameters["Target"];
                var entityName = entity.LogicalName;

                try
                {
                    var publisherPrefix = entityName.Split('_')[0];
                   
                   
                    MetadataHelper meta = new MetadataHelper(service, entity);
                    string englishName;
                    string frenchName;                    

                    int? nameFieldLength;
                    if (publisherPrefix == entityName)
                    {
                        frenchName = entity.GetAttributeValue<string>("namefr");
                        englishName = entity.GetAttributeValue<string>("nameen");
                        nameFieldLength = meta.getMaxLength("name");
                    }
                    else
                    {
                        frenchName = entity.GetAttributeValue<string>(publisherPrefix + "_namefr");
                        englishName = entity.GetAttributeValue<string>(publisherPrefix + "_nameen");
                        nameFieldLength = meta.getMaxLength(publisherPrefix + "_name");
                    }

                    StringHelper strHelper = new StringHelper();
                    var newStr = strHelper.concatenateAfterCalc(englishName, frenchName, nameFieldLength.GetValueOrDefault(), " / ");
                    if (publisherPrefix == entityName)
                    {
                        entity["name"] = newStr;
                    }
                    else
                    {
                        entity[publisherPrefix + "_name"] = newStr;
                    }

                    if (publisherPrefix == entityName)
                    {
                        entity["egcs_name"] = englishName + " / " + frenchName;
                    }
                    else
                    {
                        entity[publisherPrefix + "_name"] = englishName + " / " + frenchName;
                    }


                    

                }
                catch
                {
                    throw new InvalidPluginExecutionException("Something went wrong when creating - multi lingual plugin", ex1);
                }

            }
        }
    }

}


namespace fp.MultiLingual.Lib
{
    public class MultiLingual
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        ITracingService _tracingService;
        Entity _entity;
        Entity _outputEntity;
        string _entityName;
        
        public MultiLingual(IOrganizationService service, ITracingService tracingService, string entityName)
        {
            _service = service;
            _entityName = entityName;
            _tracingService = tracingService;
        }

        public MultiLingual(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, Entity outputEntity, Entity entity)
        {
            _service = service;
            _context = context;
            _entity = entity;
            _outputEntity = outputEntity;
            _tracingService = tracingService;
        }

        public bool generatePluginSteps()
        {
            FaultException ex1 = new FaultException();
            Guid plug = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PreMultiLingualCreateHelper", (int)CrmPluginStepStage.PreValidation,
                "create step for multi lingual plugin for entity: " + _entityName, SdkMessageName.Create.ToString(),
                "Create multi lingual", "concat en and fr fields").generatePluginStep();
          
            if (plug != null)
            {
                Guid plugUpdate = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PostMultiLingualUpdateHelper", (int)CrmPluginStepStage.PostOperation,
                "update step for multi lingual plugin for entity: " + _entityName, SdkMessageName.Update.ToString(),
                "Update multi lingual", "concat en and fr fields").generatePluginStep();
            
                if (plugUpdate != null)
                {
                    Guid plugRetrieve = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PostMultilingualRetrieveHelper", (int)CrmPluginStepStage.PostOperation,
                     "Retrieve step for multi lingual plugin for entity: " + _entityName, SdkMessageName.Retrieve.ToString(),
                     "Retrieve multi lingual entity", "Retrieve localized").generatePluginStep();

                    if (plugRetrieve != null)
                    {
                        Guid plugRetrieveMultiple = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PostMultiLingualRetrieveMultipleHelper", (int)CrmPluginStepStage.PostOperation,
                     "Retrieve Multiple step for multi lingual plugin for entity: " + _entityName, SdkMessageName.RetrieveMultiple.ToString(),
                     "Retrieve Multiple multi lingual entity", "Retrieve multiple localized").generatePluginStep();
                        if (plugRetrieveMultiple != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool destroyPluginSteps()
        {

            try
            {
                var plugReg = new PluginGenerator();
                var createStep = "create step for multi lingual plugin for entity: " + _entityName;
                var updateStep = "update step for multi lingual plugin for entity: " + _entityName;
                var retrieveStep = "Retrieve step for multi lingual plugin for entity: " + _entityName;
                var retrieveMultipleStep = "Retrieve Multiple step for multi lingual plugin for entity: " + _entityName;
                if (!plugReg.destroyStep(_service, _entityName, createStep))
                {
                    return false;
                };
                if (!plugReg.destroyStep(_service, _entityName, updateStep))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void translateOutput() 
        {
            var publisherPrefix = _entity.LogicalName.Split('_')[0] + "_";

            if (new UserLocaleHelper(_service, _context).getUserLanguage() == 1033)
            {
                if (_entity.Attributes.Contains(publisherPrefix + "nameen"))
                {
                    _outputEntity[publisherPrefix + "name"] = _entity.GetAttributeValue<string>(publisherPrefix + "nameen");
                }
            }
            else
            {
                if (_entity.Attributes.Contains(publisherPrefix + "namefr"))
                {
                    _outputEntity[publisherPrefix + "name"] = _entity.GetAttributeValue<string>(publisherPrefix + "namefr");
                }

            }
        }


    }
}
