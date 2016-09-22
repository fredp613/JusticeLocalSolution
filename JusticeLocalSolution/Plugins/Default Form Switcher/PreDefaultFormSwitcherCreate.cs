
using System;
using System.ServiceModel;
using JusticeLocalSolution.Plugins.Lib.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;





namespace JusticeLocalSolution.Plugins
{
    using System;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Discovery;
    using Microsoft.Xrm.Sdk.Messages;
    using System.Collections.Generic;
    using System.Linq;
    using Lib.TPProfile;
    using Lib.Common;
    using FredPearson.Common;


    public class PreDefaultFormSwitcherCreate : IPlugin
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
            //throw new InvalidPluginExecutionException("asdfsd", ex1);


            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName != "fp_defaultformswitcher")
                    return;
                try
                {
                    var entityForPlugin = entity.GetAttributeValue<string>("fp_name");
                    tracingService.Trace("got this far {0}", entityForPlugin);
                    if (!new DefaultFormSwitcher(service, tracingService, entityForPlugin).generatePluginSteps())
                    {
                        tracingService.Trace("some went wrong in the form switcher class");
                        throw new InvalidPluginExecutionException("something went wrong", ex1);
                    }
                    else
                    {
                        tracingService.Trace("got this far 1");
                    }
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the Default Form Switcher plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Default Form Switcher Plugin: {0}", ex.ToString());
                    throw;
                }

            }


        }
    }
}
