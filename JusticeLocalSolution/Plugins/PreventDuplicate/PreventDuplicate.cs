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
    public class PreventDuplicatePlugin : IPlugin
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

                QueryExpression qe = new QueryExpression("fp_preventduplicatefield");
                qe.Criteria.AddCondition("fp_name", ConditionOperator.Equal, entityName);
                qe.ColumnSet = new ColumnSet("fp_fieldnames");
                var fieldNames = service.RetrieveMultiple(qe).Entities.First().GetAttributeValue<string>("fp_fieldnames");
                
                char[] comma = {','};
                var fieldNameArr = fieldNames.Split(comma).ToList();

                //var publisherPrefix = entityName.Split('_')[0] + "_";
                QueryExpression qe1 = new QueryExpression();
                qe1.EntityName = entityName;
                qe1.ColumnSet = new ColumnSet();
                foreach(string fieldName in fieldNameArr) {
                    string fn = fieldName.Trim();
                    qe1.ColumnSet.AddColumn(fn);
                    
                    //MUST ADD IMAGE AT SOME POINT                   

                  
                    if (context.MessageName.ToUpper() == "UPDATE")
                    {

                        var thisEntity = service.Retrieve(entityName, entity.Id, new ColumnSet(fn));
                       
                       // throw new InvalidPluginExecutionException(thisEntity.Attributes[fn].ToString(), ex1);
                        if (thisEntity.Attributes[fn] is EntityReference)
                        {
                            qe1.Criteria.AddCondition(fn, ConditionOperator.Equal, thisEntity.GetAttributeValue<EntityReference>(fn).Id);
                        }
                        if (thisEntity.Attributes[fn] is string)
                        {
                            qe1.Criteria.AddCondition(fn, ConditionOperator.Equal, thisEntity.GetAttributeValue<string>(fn));
                        }
                    }
                    else
                    {
                     
                        if (entity.Attributes[fn] is EntityReference)
                        {
                            qe1.Criteria.AddCondition(fn, ConditionOperator.Equal, entity.GetAttributeValue<EntityReference>(fn).Id);
                        }
                        if (entity.Attributes[fn] is string)
                        {
                            qe1.Criteria.AddCondition(fn, ConditionOperator.Equal, entity.GetAttributeValue<string>(fn));
                        }
                    }
                    
                   
                  
                  
                    
                }

                               
                if (service.RetrieveMultiple(qe1).Entities.Count() > 1)
                {
                    throw new InvalidPluginExecutionException("You cannot add a duplicate record", ex1);
                }

            }
        }
    }

}

namespace fp.PreventDuplicate.Common {
    class PreventDuplicate
    {
        IOrganizationService _service;
        string _entityName;

        public PreventDuplicate(IOrganizationService service, string entityName)
        {
            _service = service;
            _entityName = entityName;
        }
        public bool generatePluginSteps()
        {
            FaultException ex1 = new FaultException();
            Guid plug = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PreventDuplicatePlugin", (int)CrmPluginStepStage.PostOperation,
                "create step for prevent duplicate plugin for entity: " + _entityName, SdkMessageName.Create.ToString(),
                "Create plugin prevent duplicate", "validate no duplicate").generatePluginStep();
            //throw new InvalidPluginExecutionException(plug.ToString(), ex1);
            if (plug != null)
            {
                Guid plugUpdate = new PluginGenerator(_service, "JusticeLocalSolution.Plugins", _entityName, "JusticeLocalSolution.Plugins.PreventDuplicatePlugin", (int)CrmPluginStepStage.PostOperation,
                "update step for prevent duplicate plugin for entity: " + _entityName, SdkMessageName.Update.ToString(),
                "Update plugin prevent duplicate", "validate no duplicate").generatePluginStep();
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
                var createStep = "create step for prevent duplicate plugin for entity: " + _entityName;
                var updateStep = "update step for prevent duplicate plugin for entity: " + _entityName;
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
    

