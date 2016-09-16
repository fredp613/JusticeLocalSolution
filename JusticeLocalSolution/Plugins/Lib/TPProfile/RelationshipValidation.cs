using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;



namespace JusticeLocalSolution.Plugins.Lib.TPProfile
{
    class RelationshipValidation
    {
        IOrganizationService service;
        Entity entity;
        EntityReference entityRef;
        OrganizationServiceContext ctx;
        FaultException ex1;
        String entityName;
        Guid entityId;
        Guid tpProfileId;


        public RelationshipValidation(IOrganizationService serv, Entity entityToProcess, EntityReference entityRef, Guid tpProfileId, OrganizationServiceContext context)
        {            
            this.service = serv;
            this.entity = entityToProcess;
            this.ctx = context;
            this.entityRef = entityRef;
            this.ex1 = new FaultException();
            this.tpProfileId = tpProfileId;
            if (this.entityRef != null)
            {
                this.entityName = entityRef.LogicalName;
                this.entityId = entityRef.Id;
            }
            if (this.entity != null)
            {
                this.entityName = entity.LogicalName;
                this.entityId = entity.Id;
            }
        }

        

        public Boolean singleMandatoryRelationshipRecordsMinCountOfOne(String fieldNameToUpdate)
        {
            
            if (this.hasAtLeastOneAssociatedRecords(entityName))
            {
                return this.updateRelationshipValidationFieldInTPProfile(fieldNameToUpdate, true);
            }
            return this.updateRelationshipValidationFieldInTPProfile(fieldNameToUpdate, false);
        }

        public Boolean mandatoryRelationshipRecordsMinCountOfOne()
        {
            String[] mandatoryRelationships = new String[] { "egcs_tp_objective", "egcs_tp_initiative", "egcs_tp_performanceindicator" };           

            RetrieveEntityRequest retrieveTPProfileEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Relationships,   
                LogicalName = entity.LogicalName
            };
            
            RetrieveEntityResponse retrieveTPPRofileEntityResponse = (RetrieveEntityResponse)service.Execute(retrieveTPProfileEntityRequest);
    
            var oneToManyRelationships = retrieveTPPRofileEntityResponse.EntityMetadata.OneToManyRelationships;
            String testStr = "";
            foreach (var rel in oneToManyRelationships)
            {
                var entityStr = rel.ReferencingEntity.ToString();
                if (mandatoryRelationships.Contains(entityStr))
                {
                    //perform necessary operations
                    if (hasAtLeastOneAssociatedRecords(entityStr))
                    {
                        testStr += rel.ReferencingEntity.ToString();
                    }
                    
                }                
            }
           
            return false;
        }

        /** PRIVATE METHODS **/
        private Boolean hasAtLeastOneAssociatedRecords(String relationshipEntityName)
        {
      
            QueryExpression qryRelEntity = new QueryExpression(entityName);
            
            qryRelEntity.Criteria.AddCondition("egcs_tp_profile", ConditionOperator.Equal, this.tpProfileId);
            //throw new InvalidPluginExecutionException(service.RetrieveMultiple(qryRelEntity).Entities.Count().ToString(), ex1);
            var results = service.RetrieveMultiple(qryRelEntity);
            if (results != null)
            {
                if (results.Entities.Count() >= 1)
                {
                    if (this.entityRef != null && results.Entities.Count() == 1)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
           
            return false;
        }

        private Boolean updateRelationshipValidationFieldInTPProfile(String fieldName, Boolean yesno)
        {

      
            var relatedTPProfile = service.Retrieve("egcs_tp_profile", this.tpProfileId, new ColumnSet("egcs_name", fieldName));
            
            if (relatedTPProfile != null)
            {
                             
                relatedTPProfile[fieldName] = yesno;
            
                ctx.Attach(relatedTPProfile);
                ctx.UpdateObject(relatedTPProfile);
                ctx.SaveChanges();
               
                return true;
            }
            return false;
        }

       
    }

  
}
