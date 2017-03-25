using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions;

namespace SampleCode
{
    class OrganizationServiceSamples
    {

        public void ExecuteProcessSingleEntity(IOrganizationService orgSvc)
        {

            // wow to execute a workflow on a single entity

            Guid workflowId = new Guid("109C382F-4CB5-42CF-8E30-5EAE63E7573B");
            Guid contactId = new Guid("8E5BAC99-90E9-4D57-B9AD-B597D3AD69A7");

            orgSvc.ExecuteProcess(workflowId, contactId);

        }

        public void ExecuteProcessOnChildRecords(IOrganizationService orgSvc)
        {

            // how to execute a worglwo on multiple child records based on a relationship

            Guid workflowId = new Guid("109C382F-4CB5-42CF-8E30-5EAE63E7573B");
            EntityReference account = new EntityReference("account", new Guid("8E5BAC99-90E9-4D57-B9AD-B597D3AD69A7"));
            string relationship = "contact_customers_account"; // contacts under this account (parent account == this)

            orgSvc.ExecuteProcessOnChildRecords(workflowId, account, relationship, continueOnError: false);

        }

    }
}
