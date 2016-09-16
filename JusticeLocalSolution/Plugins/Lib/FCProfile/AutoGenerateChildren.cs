using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;

namespace JusticeLocalSolution.Plugins.Lib.FCProfile
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

        /** assessment record generating **/
        public Boolean generateRiskAssessment() 
        {
            return false;
        }        
        public Boolean generateEligibilityAssessment()
        {
            return false;
        }
        public Boolean generateProgramAssessment()
        {
            return false;
        }

        public Boolean generateCustomAssessment()
        {
            return false;
        }
        /** financials **/
        public Boolean generateCashFlow(String[] args)
        {
            return false;
        }
        


    }
}
