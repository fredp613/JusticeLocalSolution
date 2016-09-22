namespace JusticeLocalSolution.Plugins
{
    using System;
    using System.ServiceModel;
    using JusticeLocalSolution.Plugins.Lib.Common;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;


    public class PostFCRiskReviewCreate : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

           
            FaultException ex = new FaultException();

            try
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "egcs_fc_riskreview") return;

                Entity template = service.Retrieve("egcs_tp_profile_fc_risktemplate", entity.GetAttributeValue<EntityReference>("egcs_tp_profile_fc_risktemplate").Id, new ColumnSet("egcs_fc_risktemplate", "statuscode"));

                QueryExpression qe = new QueryExpression("egcs_fc_risktemplate_fc_riskelement");
                qe.Criteria.AddCondition("egcs_fc_risktemplate", ConditionOperator.Equal, template.GetAttributeValue<EntityReference>("egcs_fc_risktemplate").Id);
                qe.ColumnSet = new ColumnSet(true);
                var results = service.RetrieveMultiple(qe);
                
                var templateStatus = new OptionSetHelper();
                
                var status = templateStatus.getLabelFromField(template, "statuscode", service);

                if (status != "Active")
                {
                    throw new InvalidPluginExecutionException("The risk template associated with this funding case is incomplete", ex);
                }

                if (results.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException("The risk template associated with this funding case is incomplete. Please contact the system administrator to complete the template", ex);
                }
               
                foreach(Entity result in results.Entities) {
                    
                    //Guid riskElementId = result.GetAttributeValue<EntityReference>("egcs_fc_riskelement").Id;                    

                    var reRef = result.GetAttributeValue<EntityReference>("egcs_fc_riskelement");
                    var rtRef = template.GetAttributeValue<EntityReference>("egcs_fc_risktemplate");

                    Entity newAttachedRiskElement = new Entity("egcs_fc_riskreviewelement");               
                    newAttachedRiskElement["egcs_fc_riskelement"] = reRef;
                    newAttachedRiskElement["egcs_fc_risktemplate_fc_riskelement"] = new EntityReference(result.LogicalName, result.Id);
                    newAttachedRiskElement["egcs_fc_riskreview"] = new EntityReference(entity.LogicalName,entity.Id);
                    newAttachedRiskElement["egcs_fc_risktemplate"] = rtRef;           
                    service.Create(newAttachedRiskElement);
                                     
                    
                };


            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }

}
