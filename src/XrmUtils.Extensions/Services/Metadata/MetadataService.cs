using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Services.Metadata
{
    /// <summary>
    ///     A metadata service.
    /// </summary>
    public class MetadataService
    {

        private IOrganizationService _orgSvc;

        public MetadataService(IOrganizationService orgService)
        {

            if(orgService == null)
            {
                throw new ArgumentNullException(nameof(orgService), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(orgService)));
            }

            _orgSvc = orgService;

        }

        /// <summary>
        ///     Checks whether an entity exist.
        /// </summary>
        /// <param name="entityName">Entity logical name</param>
        /// <returns>
        ///     True if it entity exists, otherwhise false.
        /// </returns>
        public bool EntityExist(string entityName)
        {
            
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(entityName)));
            }

            bool entityExist = false;

            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = new MetadataFilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityName)
                    }
                },
                Properties = new MetadataPropertiesExpression("ObjectTypeCode")
            };

            var req = new RetrieveMetadataChangesRequest
            {
                Query = entityQueryExpression
            };

            var resp = (RetrieveMetadataChangesResponse) _orgSvc.Execute(req);

            if (resp.EntityMetadata != null && resp.EntityMetadata.Count > 0)
            {
                entityExist = true;
            }

            return entityExist;

        }

        /// <summary>
        ///     Gets an attribute metadata.
        /// </summary>
        /// <param name="entityName">Entity logical name. </param>
        /// <param name="attributeName">Name of the attribute to retrieve.</param>
        /// <returns>
        ///     The attribute metadata.
        /// </returns>
        public AttributeMetadata GetAttributeMetadata(string entityName, string attributeName)
        {

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(entityName)));
            }

            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentNullException(nameof(attributeName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(attributeName)));
            }

            var req = new RetrieveAttributeRequest()
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName
            };

            var resp = (RetrieveAttributeResponse) _orgSvc.Execute(req);

            return resp.AttributeMetadata;

        }

        /// <summary>
        ///     Gets entity metadata.
        /// </summary>
        /// <param name="entityName">Entity logical name.</param>
        /// <returns>
        ///     The entity metadata.
        /// </returns>
        public EntityMetadata GetEntityMetadata(string entityName)
        {
            return GetEntityMetadata(entityName, EntityFilters.Entity);
        }

        /// <summary>
        ///     Gets entity metadata.
        /// </summary>
        /// <param name="entityName">Entity logical name.</param>
        /// <param name="entityFilters">Describes the type of entity metadata to receive.Default value: EntityFilters.Entity</param>
        /// <returns>
        ///     The entity metadata.
        /// </returns>
        public EntityMetadata GetEntityMetadata(string entityName, EntityFilters entityFilters)
        {

            if(string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(entityName)));
            }

            EntityMetadata entityMetadata = null;

            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityName,
                EntityFilters = entityFilters
            };

            var resp = (RetrieveEntityResponse) _orgSvc.Execute(req);                                                                        
            entityMetadata = resp.EntityMetadata;

            return entityMetadata;

        }

        /// <summary>
        ///     Gets entity metadata.
        /// </summary>
        /// <param name="entityTypeCode">The entity type code.</param>
        /// <param name="entityFilters">Describes the type of entity metadata to receive.Default value: EntityFilters.Entity</param>
        /// <returns>
        ///     The entity metadata.
        /// </returns>
        public EntityMetadata GetEntityMetadata(int entityTypeCode, EntityFilters entityFilters)
        {

            string logicalName = GetEntityLogicalName(entityTypeCode);

            if(string.IsNullOrWhiteSpace(logicalName))
            {
                throw new KeyNotFoundException(string.Format(Extensions.Resources.Messages.EntityTypeCodeNotFound, entityTypeCode.ToString()));
            }

            return GetEntityMetadata(logicalName, entityFilters);

        }

        /// <summary>
        ///     Gets entity logical name from a type code.
        /// </summary>
        /// <param name="entityTypeCode">The entity type code.</param>
        /// <returns>
        ///     The entity logical name.
        /// </returns>
        public string GetEntityLogicalName(int entityTypeCode)
        {

            string logicalName = null;
            
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = new MetadataFilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, entityTypeCode)
                    }                    
                },
                Properties = new MetadataPropertiesExpression("DisplayName", "ObjectTypeCode", "PrimaryIdAttribute", "PrimaryNameAttribute")
            };

            var req = new RetrieveMetadataChangesRequest
            {
                Query = entityQueryExpression
            };

            var resp = (RetrieveMetadataChangesResponse) _orgSvc.Execute(req);

            if(resp.EntityMetadata != null && resp.EntityMetadata.Count > 0)
            {
                logicalName = resp.EntityMetadata.First().LogicalName;
            }

            return logicalName;

        }

        /// <summary>
        /// Gets metadata for all global option sets.
        /// </summary>
        public OptionSetMetadataBase[] GetGlobalOptionSets()
        {

            var req = new RetrieveAllOptionSetsRequest();                        
            var resp = (RetrieveAllOptionSetsResponse) _orgSvc.Execute(req);
            return resp.OptionSetMetadata;

        }


    }
}
