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

 

namespace JusticeLocalSolution.Plugins.Lib.TPProfile
{
    class AutoGenerateChildren
    {
        IOrganizationService service;
        Entity entity;
        OrganizationServiceContext ctx;
        
        public AutoGenerateChildren(IOrganizationService serv, Entity tpProfile, OrganizationServiceContext context)
        {            
            this.service = serv;
            this.entity = tpProfile;
            this.ctx = context;
        }


        /*** PUBLIC METHODS ***/

        public Boolean generateAnnualBudgetLines(DateTime startDate, DateTime endDate, Money amountBudget)
        {
            FaultException ex1 = new FaultException();

            
     
            int[] fiscalYears = new FiscalYearHelper(startDate, endDate).getFiscalYears();
            
            QueryExpression existingTPProfileBudgets = new QueryExpression
            {
                EntityName = "egcs_tp_annualbudget",
                ColumnSet = new ColumnSet("egcs_tp_profile", "egcs_fiscalyear"),
                Criteria = new FilterExpression
                {
                    Conditions = {
                                    new ConditionExpression {
                                    AttributeName = "egcs_tp_profile",
                                    Operator = ConditionOperator.Equal,
                                    Values = { entity.Id }
                                    }
                                }
                }
            };

           

            DataCollection<Entity> tpProfileAnnualBudgetsToDelete = service.RetrieveMultiple(existingTPProfileBudgets).Entities;


            if (tpProfileAnnualBudgetsToDelete.Count > 0)
            {
                //var currentYears = tpProfileAnnualBudgetsToDelete.Select(s => (int)s.GetAttributeValue<OptionSetValue>("egcs_fiscalyear").Value).ToArray();
                //var newYears = fiscalYears.ToArray();
                //var illegalYears = currentYears.Except(newYears);

                foreach (Entity fcb in tpProfileAnnualBudgetsToDelete)
                {
                    service.Delete("egcs_tp_annualbudget", fcb.Id);
                    
                    //ctx.DeleteObject(fcb);
                    //ctx.SaveChanges();
                }
            }
           // } else {
                Array values = Enum.GetValues(typeof(goal_fiscalyear));
                string[] fys = new string[fiscalYears.Count()];
                int index = 0;

                foreach (int year in fiscalYears)
                {
                    if (!generateBudgetRecords(year, amountBudget))
                    {
                        return false;
                    };
                    index++;
                }
            
           // }

            return true;
        }
        

        /*** PRIVATE METHODS ***/

        private Boolean generateBudgetRecords(int year, Money amountBudget)
        {
           // if (amountBudget.Value > 0)
           // {                                       
                try
                {
                    Entity tpAnnualBudget = new Entity("egcs_tp_annualbudget");
                    // fundCentreBudget.Id = Guid.NewGuid();
                    tpAnnualBudget["egcs_amount"] = amountBudget;
                    // fys[index] = (string)Enum.GetName(typeof(goal_fiscalyear), year);
                    OptionSetValue fy = new OptionSetValue();
                    fy.Value = year;
                    tpAnnualBudget["egcs_fiscalyear"] = fy;

                    EntityReference tpProfile = new EntityReference("egcs_tp_profile", entity.Id);
                    tpAnnualBudget["egcs_tp_profile"] = tpProfile;
                   // tpAnnualBudget["egcs_name"] = fy.Value.ToString();
                    // ctx.Attach(fundCentreBudget)
                    ctx.AddObject(tpAnnualBudget);
                    ctx.SaveChanges();
                    return true;
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    return false;

                }
                   
                  
          //  }

         //   return false;

            
        }

    }
}
