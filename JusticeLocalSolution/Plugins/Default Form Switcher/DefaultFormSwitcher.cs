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

    public class DefaultFormSwitcherPluginRetrieve : IPlugin
    {
        FaultException ex = new FaultException();

        public void Execute(System.IServiceProvider serviceProvider)
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

                //Entity entity;
                var entityRef = (EntityReference)context.InputParameters["Target"];
                //if (entityRef != null)
                //{
                //    //if (context.Depth > 1) return;

                //    //get current entity inside this context
                //    QueryExpression qe = new QueryExpression(entityRef.LogicalName);
                //    qe.Criteria.AddCondition(entityRef.LogicalName + "id", ConditionOperator.Equal, entityRef.Id);
                //    qe.ColumnSet = new ColumnSet(true);
                //    entity = service.RetrieveMultiple(qe).Entities[0];
                    
                //    //get the objecttypecode in order to get the lastviewedformxml
                //    var meta = new MetadataHelper(service, entity);
                //    var objecttypecode = meta.getObjectTypeCode();

                //    //get the lastviewedformxml
                //    if (objecttypecode != null)
                //    {
                //        var query = new QueryExpression("userentityuisettings");//UserEntityUISettings.EntityLogicalName);
                //        query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, context.UserId);
                //        query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, objecttypecode);
                //        query.ColumnSet = new ColumnSet("lastviewedformxml");
                //        var settings = service.RetrieveMultiple(query).Entities;

                //        var switcherEntityQuery = new QueryExpression("fp_defaultformswitcher");
                //        switcherEntityQuery.Criteria.AddCondition("fp_name", ConditionOperator.Equal, entity.LogicalName);
                //        switcherEntityQuery.ColumnSet = new ColumnSet(true);
                //        var switcherEntityResults = service.RetrieveMultiple(switcherEntityQuery).Entities;

                //        if (switcherEntityResults.Count > 0)
                //        {
                //            Entity switcherEntity = switcherEntityResults[0];
                //            string lastViewedFormXml;
                //            if (!(settings == null || settings.Count != 1))
                //            {
                //                var userSetting = settings[0];
                //                lastViewedFormXml = userSetting.GetAttributeValue<string>("lastviewedformxml");


                //                string formToUse;
                //                string fieldToProcess = switcherEntity.GetAttributeValue<string>("fp_fieldname").ToString();
                //                if (entity.GetAttributeValue<string>(fieldToProcess).Contains("_fc_")) //this is where you loop though the switcherResults get the values and upon finding the value render the form
                //                {
                //                    //main form
                                   
                //                    formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", "8a9f3715-3938-46a8-a26a-9d08dfca8389");
                //                }
                //                else
                //                {
                //                    //new form                                    
                //                    formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", "ffb4ad6e-e40e-49c7-896a-949045afa3e4");
                //                }
                                
                //                userSetting["lastviewedformxml"] = formToUse;
                //                service.Update(userSetting);
                //                tracingService.Trace("object type code is: {0}--last viewed for xml is {1}", objecttypecode, lastViewedFormXml);
                //            }
                //        }


                        

                //    }

                //}

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("error is {0}", "asdfsadfsdf in plugin");
                throw new InvalidPluginExecutionException("An error occurred in the retrieve plug-in.", ex);
            }


        }

    }
}

namespace FredPearson.Common
{
    class DefaultFormSwitcher
    {
        IOrganizationService _service;
        ITracingService _tracingService;
        string _entityName;
       

        public DefaultFormSwitcher(IOrganizationService service, ITracingService tracingService, string entityName)
        {
            this._service = service;
            this._entityName = entityName;
            this._tracingService = tracingService;
            
        }

        public bool generatePluginSteps() {
            FaultException ex1 = new FaultException();
            _tracingService.Trace("in generate plugin steps method {0}", _entityName);
            Guid plug = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.DefaultFormSwitcherPluginRetrieve", (int)CrmPluginStepStage.PreValidation,
                "Pre Validation Retrieve for default form switcher plugin for entity: " + _entityName, SdkMessageName.Retrieve.ToString(),
                "Create plugin default form switcher", "Update form on retrieve").generatePluginStep();
            if (plug != null)
            {
                _tracingService.Trace("plugin good");
                Guid plug1 = new PluginGenerator(_service, _tracingService, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.DefaultFormSwitcherPluginRetrieve", (int)CrmPluginStepStage.PreValidation,
              "Pre Validation Retrieve Multiple for default form switcher plugin for entity: " + _entityName, SdkMessageName.RetrieveMultiple.ToString(),
              "Create plugin default form switcher multiple", "Update form on retrieve multiple").generatePluginStep();
                if (plug1 != null)
                {
                    return true;
                }                
            }
            _tracingService.Trace("some went wrong in the form switcher class");
           // return true;
            return false;
        }

        public bool destroyPluginSteps()
        {
            try
            {
                var plugReg = new PluginGenerator();
                var retrieveStep = "retrieve step for default form switcher plugin for entity: " + _entityName;
                               
                if (!plugReg.destroyStep(_service, _entityName, retrieveStep))
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
