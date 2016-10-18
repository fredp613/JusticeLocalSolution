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

namespace JusticeLocalSolution.Plugins
{

    public class PopulatorPlugin : IPlugin
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

                try
                {

                    Entity entity = (Entity)context.InputParameters["Target"];
                    var entityName = entity.LogicalName;
                    var publisherPrefix = entityName.Split('_')[0];
                    tracingService.Trace("pub prefix {0}", publisherPrefix);
                    QueryExpression qe = new QueryExpression();
                    qe.EntityName = "fp_fieldnamepopulator";
                    qe.ColumnSet = new ColumnSet();
                    qe.ColumnSet.AddColumns("fp_name", "fp_fieldnames");

                    qe.Criteria.AddCondition("fp_name", ConditionOperator.Equal, entityName.ToString().ToLower());
                    var results = service.RetrieveMultiple(qe);
            
                    if (results.Entities.ToArray().Count() > 0)
                    {


                        Entity referenceEntity = results.Entities.First();
                        string fieldNames = referenceEntity.GetAttributeValue<string>("fp_fieldnames");
                        char[] comma = { ',' };
                        
                        var fieldNameArr = fieldNames.Split(comma).ToList();
                        if (fieldNameArr.Count() > 2) throw new InvalidPluginExecutionException("You can only add 2 fields", ex1);

                        string nameToUse;

                        if (publisherPrefix == entityName)
                        {
                            nameToUse = "name";
                        }
                        else
                        {
                            nameToUse = publisherPrefix + "_name";
                        }
                      

                        if (string.IsNullOrWhiteSpace(fieldNames))
                        {
                            throw new InvalidPluginExecutionException("null", ex1);
                        }

                        string refName = "";
                        string refName1 = "";

                        Entity entityToUse;

                        if (context.MessageName.ToUpper() == "UPDATE")
                        {
                            var columns = new ColumnSet();
                            foreach(string fieldName in fieldNameArr) {
                                string fn = fieldName.Trim();
                                columns.AddColumn(fn);
                            }
                            entityToUse = service.Retrieve(entityName, entity.Id, columns);
                        }
                        else
                        {
                            entityToUse = entity;
                        }
                        foreach (string fieldName in fieldNameArr)
                        {
                            string fn = fieldName.Trim();
                            if (entityToUse.Attributes.Contains(fn)) {
                              
                                if (entityToUse.Attributes[fn] is EntityReference)
                                {
                                    var refId = entityToUse.GetAttributeValue<EntityReference>(fn).Id;
                                    var nameToUseOfContext = nameToUse;
                                    if (fn == "egcs_contact")
                                    {
                                        fn = "contact";
                                        nameToUseOfContext = "fullname";                                        
                                    }
                                    
                                    if (fn == "egcs_account")
                                    {
                                        fn = "account";
                                        nameToUseOfContext = "name";
                                    }
                                   
                                    var valToUse = service.Retrieve(fn, refId, new ColumnSet(nameToUseOfContext)).GetAttributeValue<string>(nameToUseOfContext);
                                    
                                    if (string.IsNullOrEmpty(refName)) {
                                        refName = valToUse;
                                    } else {
                                        refName1 = valToUse;
                                    
                                    };
                                   fn = fieldName.Trim();
                                  
                                }                              
                                if (entityToUse.Attributes[fn] is string)
                                {
                                    var valToUse = entityToUse.GetAttributeValue<string>(fn);
                                    if (string.IsNullOrEmpty(refName))
                                    {
                                        refName = valToUse;
                                    }
                                    else
                                    {
                                        refName1 = valToUse;
                                    };

                                }
                                if (entityToUse.Attributes[fn] is OptionSetValue)
                                {
                                    var val = new OptionSetHelper();
 
                                    OptionSetHelper labelFromInput = new OptionSetHelper();
                                    var valToUse = labelFromInput.getLabelFromField(entityToUse, fn, service);
                                    //var valToUse = entityToUse.GetAttributeValue<OptionSetValue>(fn).Value.ToString();
                                    
                                   //var valToUse1 = entityToUse.FormattedValues[fn];
                                    //throw new InvalidPluginExecutionException(label, ex1);
                                    
                                    if (string.IsNullOrEmpty(refName))
                                    {
                                        refName = valToUse;
                                    }
                                    else
                                    {                                        
                                        refName1 = valToUse;
                                    };
                                }
                                if (entityToUse.Attributes[fn] is bool)
                                {
                                    var valToUse = entityToUse.GetAttributeValue<bool>(fn).ToString();
                                    if (string.IsNullOrEmpty(refName))
                                    {
                                        refName = valToUse;
                                    }
                                    else
                                    {
                                        refName1 = valToUse;
                                    };
                                }
                            }                                
                            else
                            {

                                //add to TRACE log
                                // throw new InvalidPluginExecutionException("none2", ex1);
                                tracingService.Trace("(inside try block) Field Name Populator Helper Plugin: {0}", ex1.ToString());
                            }

                        }
                        MetadataHelper meta = new MetadataHelper(service, entity);
                        int fieldLength = meta.getMaxLength(nameToUse).GetValueOrDefault();
                        StringHelper helper = new StringHelper();
                        var newStr = helper.concatenateAfterCalc(refName, refName1, fieldLength, " ");
                        if (publisherPrefix == entityName)
                        {
                            entity["name"] = newStr;
                        }
                        else
                        {
                            entity[publisherPrefix + "_name"] = newStr;
                        }

                        if (context.MessageName.ToUpper() == "UPDATE")
                        {
                            if (context.Depth > 1) { return; }
                            service.Update(entity);
                        }

                    }

                }
                catch (Exception ex)
                {
                    tracingService.Trace("Field Name Populator Helper Plugin: {0}", ex.ToString());
                    throw;
                }
                
            }
        }
    }

}

namespace FredPearson.Common
{
    class Populator
    {
        IOrganizationService _service;
        string _entityName;

        
        public Populator(IOrganizationService service, string entityName)
        {
            this._service = service;
            this._entityName = entityName;
            
        }

        public bool generatePluginSteps() {
            FaultException ex1 = new FaultException();
            
            Guid plug = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PopulatorPlugin", (int)CrmPluginStepStage.PreValidation,
                "create step for name populator plugin for entity: " + _entityName, SdkMessageName.Create.ToString(),
                "Create plugin populator", "update name field on create").generatePluginStep();
            //throw new InvalidPluginExecutionException(plug.ToString(), ex1);
            if (plug != null)
            {
                Guid plugUpdate = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PopulatorPlugin", (int)CrmPluginStepStage.PostOperation,
                "update step for name populator plugin for entity: " + _entityName, SdkMessageName.Update.ToString(),
                "Update plugin populator", "update name field on update").generatePluginStep();
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
                var createStep = "create step for name populator plugin for entity: " + _entityName;
                var updateStep = "update step for name populator plugin for entity: " + _entityName;
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

        private Guid findPlugin(Guid id)
        {
            return new Guid();
        }

        private Guid getFieldNameToConcatenate()
        {
            return new Guid();
        }
    }
}
