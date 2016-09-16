using System;
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

namespace JusticeLocalSolution.Plugins
{
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
    class MultiLingual
    {
        IOrganizationService _service;
        string _entityName;
        
        public MultiLingual(IOrganizationService service, string entityName)
        {
            _service = service;
            _entityName = entityName;
        }

        public bool generatePluginSteps()
        {
            FaultException ex1 = new FaultException();
            Guid plug = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PreMultiLingualCreateHelper", (int)CrmPluginStepStage.PreValidation,
                "create step for multi lingual plugin for entity: " + _entityName, SdkMessageName.Create.ToString(),
                "Create multi lingual", "concat en and fr fields").generatePluginStep();
            //throw new InvalidPluginExecutionException(plug.ToString(), ex1);
            if (plug != null)
            {
                Guid plugUpdate = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PostMultiLingualUpdateHelper", (int)CrmPluginStepStage.PostOperation,
                "update step for multi lingual plugin for entity: " + _entityName, SdkMessageName.Update.ToString(),
                "Update multi lingual", "concat en and fr fields").generatePluginStep();
                //throw new InvalidPluginExecutionException(plug.ToString(), ex1);
                if (plugUpdate != null)
                {
                    return true;
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


    }
}
