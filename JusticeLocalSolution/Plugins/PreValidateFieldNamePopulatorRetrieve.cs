// <copyright file="PreValidateTPProfileRetrieve.cs" company="Department of Justice/ministaire de la Justice">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Department of Justice/ministaire de la Justice</author>
// <date>9/20/2016 1:52:35 PM</date>
// <summary>Implements the PreValidateTPProfileRetrieve Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
namespace JusticeLocalSolution.Plugins
{
    using System;
    using System.ServiceModel;
    using JusticeLocalSolution.Plugins.Lib.Common;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PreValidateTPProfileRetrieve Plugin.
    /// </summary>    

    public class PreValidateFieldNamePopulatorRetrieve : IPlugin
    {


        FaultException ex = new FaultException();

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

                Entity entity;
                var entityRef = (EntityReference)context.InputParameters["Target"];
                if (entityRef != null)
                {
                    //if (context.Depth > 1) return;

                    //get current entity inside this context
                    QueryExpression qe = new QueryExpression(entityRef.LogicalName);
                    qe.Criteria.AddCondition("fp_fieldnamepopulatorid", ConditionOperator.Equal, entityRef.Id);
                    qe.ColumnSet = new ColumnSet(true);
                    entity = service.RetrieveMultiple(qe).Entities[0];

                    //get the objecttypecode in order to get the lastviewedformxml
                    var meta = new MetadataHelper(service, entity);
                    var objecttypecode = meta.getObjectTypeCode();

                    //get the lastviewedformxml
                    if (objecttypecode != null)
                    {
                        var query = new QueryExpression("userentityuisettings");//UserEntityUISettings.EntityLogicalName);
                        query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, context.UserId);
                        query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, objecttypecode);
                        query.ColumnSet = new ColumnSet("lastviewedformxml");
                        var settings = service.RetrieveMultiple(query).Entities;

                        string lastViewedFormXml;
                        if (!(settings == null || settings.Count != 1))
                        {
                            var userSetting = settings[0];
                            lastViewedFormXml = userSetting.GetAttributeValue<string>("lastviewedformxml");
                            string formToUse;
                            if (entity.GetAttributeValue<string>("fp_name").Contains("_fc_"))
                            {
                                //main form
                                formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", "8a9f3715-3938-46a8-a26a-9d08dfca8389");
                            }
                            else
                            {
                                //new form
                                formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", "ffb4ad6e-e40e-49c7-896a-949045afa3e4");
                            }


                            userSetting["lastviewedformxml"] = formToUse;
                            service.Update(userSetting);

                            //switch ((accountType)account.cf_AccountType.Value)
                            //{
                            //    case accountType.Customer:
                            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", CustomerAccountFormId);
                            //        break;
                            //    case accountType.Vendor:
                            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", VendorAccountFormId);
                            //        break;
                            //    case accountType.Partner:
                            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", PartnerAccountFormId);
                            //        break;
                            //    case accountType.Other:
                            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", GenericAccountFormId);
                            //        break;
                            //    default:
                            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", GenericAccountFormId);
                            //        return;
                            //}

                            tracingService.Trace("object type code is: {0}--last viewed for xml is {1}", objecttypecode, lastViewedFormXml);
                        }

                    }


                    //set the lastviewedformxml



                    // Some users such as SYSTEM have no UserEntityUISettings, so skip.
                    //


                    //  throw new InvalidPluginExecutionException("asdfasdf", ex1);
                }

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("error is {0}", "asdfsadfsdf in plugin");
                throw new InvalidPluginExecutionException("An error occurred in the multi lingual plug-in.", ex);
            }


        }

        private void SetForm(Entity entity, IOrganizationService service, Guid userId)
        {
            var query = new QueryExpression("userentityuisettings");
            query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, userId);
            // query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, Account.EntityTypeCode);
            query.ColumnSet = new ColumnSet("lastviewedformxml");
            var settings = service.RetrieveMultiple(query).Entities;

            // Some users such as SYSTEM have no UserEntityUISettings, so skip.
            //   if (settings == null || settings.Count != 1 || account.cf_AccountType == null) return;

            //  var setting = settings[0].ToEntity<UserEntityUISettings>();
            //string formToUse;
            //switch ((accountType)account.cf_AccountType.Value)
            //{
            //    case accountType.Customer:
            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", CustomerAccountFormId);
            //        break;
            //    case accountType.Vendor:
            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", VendorAccountFormId);
            //        break;
            //    case accountType.Partner:
            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", PartnerAccountFormId);
            //        break;
            //    case accountType.Other:
            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", GenericAccountFormId);
            //        break;
            //    default:
            //        formToUse = String.Format("<MRUForm><Form Type=\"Main\" Id=\"{0}\" /></MRUForm>", GenericAccountFormId);
            //        return;
            //}

            // Only update if the last viewed form is not the one required for the given opportunity type
            // if (!formToUse.Equals(setting.LastViewedFormXml, StringComparison.InvariantCultureIgnoreCase))
            //  {
            //     var s = new UserEntityUISettings { Id = setting.Id, LastViewedFormXml = formToUse };
            //     service.Update(s);
            // }
        }
    }



}
