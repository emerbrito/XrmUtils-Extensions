using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Services;
using XrmUtils.Services.Metadata;

namespace XrmUtils.Extensions
{


    public static class OrganizationServiceExtensions
    {

        /// <summary>
        /// Removes all related entities based on navigation property.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the navigation property.</param>
        /// <param name="navigationProperty">Name of the navigation property.</param>
        /// <returns>Total number of deleted records.</returns>        
        /// <exception cref="FaultException{OrganizationServiceFault}">Throw when a <see cref="OrganizationServiceFault"/> is returned as a result of one or more delete requests.</exception>
        public static int DeleteRelatedEntities(this IOrganizationService orgSvc, EntityReference entity, string navigationProperty)
        {

            int count = 0;
            EntityCollection relEntities;

            var requests = new List<OrganizationRequest>();

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (string.IsNullOrWhiteSpace(navigationProperty))
            {
                throw new ArgumentNullException(nameof(navigationProperty), string.Format(Resources.Messages.ArgumentNull, nameof(navigationProperty)));
            }

            relEntities = GetRelatedEntities(orgSvc, entity, new Relationship(navigationProperty), new ColumnSet(null));

            if (relEntities == null || relEntities.Entities.Count == 0)
            {
                return count;
            }

            count = relEntities.Entities.Count;

            foreach (var item in relEntities.Entities)
            {
                var delReq = new DeleteRequest { Target = item.ToEntityReference() };
                requests.Add(delReq);
            }

            ExecuteMultiple(orgSvc, requests.ToArray());

            return count;


        }

        /// <summary>
        ///  Executes one or more message requests as a single batch operation and optionally return a collection of results.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="requests">The collection of message requests to execute.</param>
        /// <param name="continueProcessingOnError">Whether further execution of message requests in the Requests collection should continue if a fault is returned for the current request being processed.</param>
        /// <param name="returnResponse"><c>True</c> if a response for each message request processed should be returned.</param>
        /// <param name="throwIfResponseHasFault">When a response is returned, <c>True</c> to throw a <see cref="FaultException{OrganizationServiceFault}"/> if at least one response is faulted.</param>
        /// <returns></returns>
        public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService orgSvc, OrganizationRequest[] requests, bool continueProcessingOnError = false, bool returnResponse = true, bool throwIfResponseHasFault = true)
        {

            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests), string.Format(Resources.Messages.ArgumentNull, nameof(requests)));
            }

            if(requests.Length == 0)
            {
                throw new ArgumentException(nameof(requests), string.Format(Resources.Messages.CollectionCannotBeEmpty, nameof(requests)));
            }

            ExecuteMultipleRequest req;
            ExecuteMultipleResponse resp;

            req = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = continueProcessingOnError,
                    ReturnResponses = returnResponse
                },
                Requests = new OrganizationRequestCollection()
            };

            req.Requests.AddRange(requests);

            resp = (ExecuteMultipleResponse) orgSvc.Execute(req);

            if(returnResponse && resp.Responses != null)
            {

                foreach (var responseItem in resp.Responses)
                {
                    if (throwIfResponseHasFault && responseItem.Fault != null)
                    {
                        throw new FaultException<OrganizationServiceFault>(responseItem.Fault);
                    }
                }

            }

            return resp;

        }

        /// <summary>
        /// Executes a workflow.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>        
        /// <param name="processId">The ID of the workflow to execute.</param>
        /// <param name="entityId">The ID of the record on which the workflow executes.</param>
        public static void ExecuteProcess(this IOrganizationService orgSvc, Guid processId, Guid entityId)
        {

            ExecuteWorkflowRequest req;
            ExecuteWorkflowResponse resp;

            req = new ExecuteWorkflowRequest()
            {
                WorkflowId = processId,
                EntityId = entityId
            };

            resp = (ExecuteWorkflowResponse) orgSvc.Execute(req);

        }

        /// <summary>
        /// Executes a workflow on related entities by navigation property.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>     
        /// <param name="processId">The ID of the workflow to execute.</param>
        /// <param name="entity">The entity containing the navigation property.</param>
        /// <param name="navigationProperty">Navigation property (relationship) logical name.</param>
        /// <param name="continueOnError">When set to <c>true</c>, the inner <see cref="ExecuteMultipleRequest"/> will be set to continue execution even if one of the process return an error.</param>
        public static int ExecuteProcessOnChildRecords(this IOrganizationService orgSvc, Guid processId, EntityReference entity, string navigationProperty, bool continueOnError)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (string.IsNullOrWhiteSpace(navigationProperty))
            {
                throw new ArgumentNullException(nameof(navigationProperty), string.Format(Resources.Messages.ArgumentNull, nameof(navigationProperty)));
            }

            var targets = GetRelatedEntities(orgSvc, entity, navigationProperty);

            if (targets == null)
            {
                return 0;
            }

            var requests = new List<OrganizationRequest>();

            foreach (var target in targets.Entities)
            {

                ExecuteWorkflowRequest req;

                req = new ExecuteWorkflowRequest()
                {
                    WorkflowId = processId,
                    EntityId = target.Id
                };

                requests.Add(req);

            }

            var resp = ExecuteMultiple(orgSvc, requests.ToArray(), 
                continueProcessingOnError: continueOnError, 
                returnResponse: true, 
                throwIfResponseHasFault: !continueOnError);

            return resp.Responses.Count;

        }

        /// <summary>
        /// Convert a query in FetchXML to a QueryExpression.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="fetchXml">The query to convert.</param>
        /// <returns>The results of the query conversion.</returns>
        public static QueryExpression FetchXmlToQueryExpression(this IOrganizationService orgSvc, string fetchXml)
        {

            if (string.IsNullOrWhiteSpace(fetchXml))
            {
                throw new ArgumentNullException(nameof(fetchXml), string.Format(Resources.Messages.ArgumentNull, nameof(fetchXml)));
            }

            var req = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXml
            };

            var resp = (FetchXmlToQueryExpressionResponse) orgSvc.Execute(req);

            return resp.Query;

        }

        /// <summary>
        /// Uses a <see cref="Relationship"/> to identify the logical name of a entity in a relationship.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the navigation property.</param>
        /// <param name="relationship">The <see cref="Relationship"/>.</param>
        /// <returns>The logical name of the entit in the relationship.</returns>
        public static string GetRelatedEntityName(this IOrganizationService orgSvc, EntityReference entity, Relationship relationship)
        {

            RetrieveRelationshipRequest retrieveRelationshipRequest;
            RetrieveRelationshipResponse retrieveRelationshipResponse;
            OneToManyRelationshipMetadata relationshipMetadata;
            ManyToManyRelationshipMetadata manyToManyRelationshipMetadatum;

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship), string.Format(Resources.Messages.ArgumentNull, nameof(relationship)));
            }

            if (relationship.PrimaryEntityRole.HasValue)
            {
                return entity.LogicalName;
            }

            retrieveRelationshipRequest = new RetrieveRelationshipRequest()
            {
                Name = relationship.SchemaName
            };

            retrieveRelationshipResponse = orgSvc.Execute(retrieveRelationshipRequest) as RetrieveRelationshipResponse;
            relationshipMetadata = retrieveRelationshipResponse.RelationshipMetadata as OneToManyRelationshipMetadata;

            if (relationshipMetadata != null)
            {

                if (relationshipMetadata.ReferencingEntity != entity.LogicalName)
                {
                    return relationshipMetadata.ReferencingEntity;
                }

                return relationshipMetadata.ReferencedEntity;
            }

            manyToManyRelationshipMetadatum = retrieveRelationshipResponse.RelationshipMetadata as ManyToManyRelationshipMetadata;

            if (manyToManyRelationshipMetadatum == null)
            {
                object[] schemaName = new object[] { relationship.SchemaName };
                throw new InvalidOperationException(string.Format(Resources.Messages.UnableToLoadRelationship, schemaName));
            }

            if (manyToManyRelationshipMetadatum.Entity1LogicalName != entity.LogicalName)
            {
                return manyToManyRelationshipMetadatum.Entity1LogicalName;
            }

            return manyToManyRelationshipMetadatum.Entity2LogicalName;

        }

        /// <summary>
        /// Retrieves a collection of related entities using a collection-valued navigation property (one-to-many or many-to-many relationships).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the collection-valued navigation property.</param>
        /// <param name="navigationProperty">Name of the navigation property.</param>
        /// <returns>An <see cref="EntityCollection"/> containing the related entities.</returns>
        public static EntityCollection GetRelatedEntities(this IOrganizationService OrgSvc, EntityReference entity, string navigationProperty)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (string.IsNullOrWhiteSpace(navigationProperty))
            {
                throw new ArgumentNullException(nameof(navigationProperty), string.Format(Resources.Messages.ArgumentNull, nameof(navigationProperty)));
            }

            return GetRelatedEntities(OrgSvc, entity, new Relationship(navigationProperty), new ColumnSet(true));
        }

        /// <summary>
        /// Retrieves a collection of related entities using a collection-valued navigation property (one-to-many or many-to-many relationships).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the collection-valued navigation property.</param>
        /// <param name="relationship">The <see cref="Relationship"/>.</param>
        /// <returns>An <see cref="EntityCollection"/> containing the related entities.</returns>
        public static EntityCollection GetRelatedEntities(this IOrganizationService orgSvc, EntityReference entity, Relationship relationship)
        {
            return GetRelatedEntities(orgSvc, entity, relationship, new ColumnSet(true));
        }

        /// <summary>
        /// Retrieves a collection of related entities using a collection-valued navigation property (one-to-many or many-to-many relationships).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the collection-valued navigation property.</param>
        /// <param name="relationship">The <see cref="Relationship"/>.</param>
        /// <param name="columnSet">The set of columns or attributes to retrieve. Pass an instance of <see cref="ColumnSet"/> with a null argument to retrieve only the primary key.</param>
        /// <returns>An <see cref="EntityCollection"/> containing the related entities.</returns>
        public static EntityCollection GetRelatedEntities(this IOrganizationService orgSvc, EntityReference entity, Relationship relationship, ColumnSet columnSet)
        {

            RelationshipQueryCollection relationshipQueryCollection;
            RelatedEntityCollection relatedEntities;
            EntityReference entityReference;
            RetrieveRequest retrieveRequest;

            string relatedEntityName;

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship), string.Format(Resources.Messages.ArgumentNull, nameof(relationship)));
            }

            if (columnSet == null)
            {
                throw new ArgumentNullException(nameof(columnSet), string.Format(Resources.Messages.ArgumentNull, nameof(columnSet)));
            }

            relatedEntityName = orgSvc.GetRelatedEntityName(entity, relationship);
            relationshipQueryCollection = new RelationshipQueryCollection();

            QueryExpression queryExpression = new QueryExpression(relatedEntityName)
            {
                ColumnSet = columnSet
            };

            relationshipQueryCollection.Add(relationship, queryExpression);
            entityReference = new EntityReference(entity.LogicalName, entity.Id);

            retrieveRequest = new RetrieveRequest()
            {
                Target = entityReference,
                ColumnSet = new ColumnSet(),
                RelatedEntitiesQuery = relationshipQueryCollection
            };

            relatedEntities = (orgSvc.Execute(retrieveRequest) as RetrieveResponse).Entity.RelatedEntities;

            if (!relatedEntities.Contains(relationship))
            {
                return new EntityCollection();
            }

            return relatedEntities[relationship];
        }

        /// <summary>
        /// Retrieves an entity based on a single-valued navigation property (many-to-one relationships).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the single-valued navigation property.</param>
        /// <param name="navigationProperty">Name of the navigation property.</param>
        /// <returns>The corresponding entity.</returns>
        public static Entity GetRelatedEntity(this IOrganizationService orgSvc, EntityReference entity, string navigationProperty)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (string.IsNullOrWhiteSpace(navigationProperty))
            {
                throw new ArgumentNullException(nameof(navigationProperty), string.Format(Resources.Messages.ArgumentNull, nameof(navigationProperty)));
            }

            return GetRelatedEntity(orgSvc, entity, new Relationship(navigationProperty));

        }

        /// <summary>
        /// Retrieves an entity based on a single-valued navigation property (many-to-one relationships).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entity">The entity containing the single-valued navigation property.</param>
        /// <param name="relationship">The <see cref="Relationship"/>.</param>
        /// <returns>The corresponding entity.</returns>
        public static Entity GetRelatedEntity(this IOrganizationService orgSvc, EntityReference entity, Relationship relationship)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship), string.Format(Resources.Messages.ArgumentNull, nameof(relationship)));
            }

            var results = GetRelatedEntities(orgSvc, entity, relationship);

            if (results == null)
            {
                return null;
            }

            if (results.Entities.Count > 1)
            {
                throw new ArgumentException(Resources.Messages.MultipleRecordsReturned);
            }

            return results.Entities.FirstOrDefault();

        }

        /// <summary>
        /// Uses a security role name to retrieve its ID.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="roleName">The security role name.</param>
        /// <returns>The security role unique identifier or an emoty Guid if security role cannot be found.</returns>
        public static Guid GetSecurityRoleId(this IOrganizationService orgSvc, string roleName)
        {

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName), string.Format(Resources.Messages.ArgumentNull, nameof(roleName)));
            }

            var query = new QueryExpression("role")
            {
                ColumnSet = new ColumnSet("roleid"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, roleName)
                    }
                }
            };

            var results = orgSvc.RetrieveMultiple(query);

            if (results.Entities == null || results.Entities.Count == 0)
            {
                return Guid.Empty;
            }

            return results.Entities.First().GetAttributeValue<Guid>("roleid");

        }

        /// <summary>
        /// Convert a query, which is represented as a QueryExpression class, to its equivalent query, which is represented as FetchXML.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="query">The query to convert.</param>
        /// <returns>The results of the query conversion.</returns>
        public static string QueryExpressionToFetchXml(this IOrganizationService orgSvc, QueryExpression query)
        {

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), string.Format(Resources.Messages.ArgumentNull, nameof(query)));
            }

            var req = new QueryExpressionToFetchXmlRequest
            {
                Query = query
            };

            var resp = (QueryExpressionToFetchXmlResponse) orgSvc.Execute(req);

            return resp.FetchXml;

        }

        /// <summary>
        /// Determine whether user has a given privelege on a particular entity.
        /// </summary>
        /// <param name="instance">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="privilegeType">Privilege type.</param>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <returns></returns>
        public static bool HasPrivilege(this IOrganizationService instance, Guid userId, PrivilegeType privilegeType, string entityLogicalName)
        {

            bool hasPrivilege = false;
            string prvName = $"prv{Enum.GetName(typeof(PrivilegeType), privilegeType)}{entityLogicalName}";

            var privilegeQuery = new QueryExpression("privilege")
            {
                ColumnSet = new ColumnSet(true)
            };


            var privilegeLink1 = new LinkEntity("privilege", "roleprivileges", "privilegeid", "privilegeid", JoinOperator.Inner);
            var privilegeLink2 = new LinkEntity("roleprivileges", "role", "roleid", "roleid", JoinOperator.Inner);
            var privilegeLink3 = new LinkEntity("role", "systemuserroles", "roleid", "roleid", JoinOperator.Inner);
            var privilegeLink4 = new LinkEntity("systemuserroles", "systemuser", "systemuserid", "systemuserid", JoinOperator.Inner);

            var userCondition = new ConditionExpression("systemuserid", ConditionOperator.Equal, userId);
            var privilegeCondition = new ConditionExpression("name", ConditionOperator.Equal, prvName); // name of the privilege

            privilegeLink4.LinkCriteria.AddCondition(userCondition);
            FilterExpression privilegeFilter = new FilterExpression(LogicalOperator.And);
            privilegeFilter.Conditions.Add(privilegeCondition);
            privilegeQuery.Criteria = privilegeFilter;

            privilegeLink3.LinkEntities.Add(privilegeLink4);
            privilegeLink2.LinkEntities.Add(privilegeLink3);
            privilegeLink1.LinkEntities.Add(privilegeLink2);
            privilegeQuery.LinkEntities.Add(privilegeLink1);

            var resp = instance.RetrieveMultiple(privilegeQuery);
            if (resp.Entities.Count > 0)
            {
                hasPrivilege = true;
            }

            return hasPrivilege;
        }

        /// <summary>
        /// Retrieves a record.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entityName">The logical name of the entity that is specified in the entityId parameter.</param>
        /// <param name="id">The ID of the record that you want to retrieve.</param>
        /// <param name="columnSet">The set of columns or attributes to retrieve. Pass an instance of <see cref="ColumnSet"/> with a null argument to retrieve only the primary key.</param>
        /// <param name="allowNull">When set to true, a null value is returned if the entity is not found. Otherwise an exception will be thrown.</param>
        /// <returns>The specified record.</returns>
        public static Entity Retrieve(this IOrganizationService orgSvc, string entityName, Guid id, ColumnSet columnSet, bool allowNull)
        {

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName), string.Format(Resources.Messages.ArgumentNull, nameof(entityName)));
            }

            if (allowNull == false)
            {
                return orgSvc.Retrieve(entityName, id, columnSet);
            }

            var mds = new MetadataService(orgSvc);
            var md = mds.GetEntityMetadata(entityName);

            var query = new QueryExpression(entityName)
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(md.PrimaryIdAttribute, ConditionOperator.Equal, id)
                    }
                }
            };

            var results = orgSvc.RetrieveMultiple(query);

            return results.Entities.FirstOrDefault();
            
        }

        /// <summary>
        /// Retrieve all entities returned from a query. WARNING: a call to this method can potentially take several minutes to complete and return a large volumne of records (not limited to the inital 5000 records of RetrieveMultiple).
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="query">The query criteria for the retrieval.</param>
        /// <returns>The collection of entities returned from the query.</returns>
        public static IEnumerable<Entity> RetrieveAll(this IOrganizationService orgSvc, QueryExpression query)
        {

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), string.Format(Resources.Messages.ArgumentNull, nameof(query)));
            }

            var results = new List<Entity>();
            int pageSize = 50;
            int pageNumber = 1;
            string pagingCookie = null;
            bool moreRecords = true;

            while (moreRecords)
            {

                var collection = orgSvc.RetrievePage(query, pageSize, pageNumber, pagingCookie);

                if(collection.Entities != null)
                {
                    results.AddRange(collection.Entities);
                }

                if (collection.MoreRecords)
                {
                    pagingCookie = collection.PagingCookie;
                    pageNumber++;
                }

                moreRecords = collection.MoreRecords;

            }

            return results;

        }

        /// <summary>
        /// Retrieves a collection of records.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="fetchXml">The FetchXml query string.</param>
        /// <returns>The collection of entities returned from the query.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService orgSvc, string fetchXml)
        {

            if (string.IsNullOrWhiteSpace(fetchXml))
            {
                throw new ArgumentNullException(nameof(fetchXml), string.Format(Resources.Messages.ArgumentNull, nameof(fetchXml)));
            }

            var query = new FetchExpression(fetchXml);
            var resp = orgSvc.RetrieveMultiple(query);

            return resp;

        }

        /// <summary>
        /// Retrieves a collection of records.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entityName">Logical name of the entity to retrieve.</param>
        /// <param name="conditions">A list of <see cref="ConditionExpression"/> used to filter the request. All conditions will be combined with an And operator.</param>
        /// <param name="columnSet">The set of columns or attributes to retrieve. Pass an instance of <see cref="ColumnSet"/> with a null argument to retrieve only the primary key.</param>
        /// <returns>The collection of entities returned from the query.</returns>
        public static EntityCollection RetrieveMultiple(this IOrganizationService orgSvc, string entityName, List<ConditionExpression> conditions, ColumnSet columnSet)
        {

            FilterExpression criteria;
            QueryExpression query;

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName), string.Format(Resources.Messages.ArgumentNull, nameof(entityName)));
            }

            if (conditions == null)
            {
                throw new ArgumentNullException(nameof(conditions), string.Format(Resources.Messages.ArgumentNull, nameof(conditions)));
            }

            if (conditions.Count == 0)
            {
                throw new ArgumentException(nameof(conditions), string.Format(Resources.Messages.CollectionCannotBeEmpty, nameof(conditions)));
            }

            if (columnSet == null)
            {
                throw new ArgumentNullException(nameof(columnSet), string.Format(Resources.Messages.ArgumentNull, nameof(columnSet)));
            }

            criteria = new FilterExpression(LogicalOperator.And);

            foreach (var item in conditions)
            {
                criteria.Conditions.Add(item);
            }

            query = new QueryExpression(entityName)
            {
                ColumnSet = columnSet,
                Criteria = criteria
            };

            return orgSvc.RetrieveMultiple(query);

        }

        /// <summary>
        /// Retrieves a collection of records based on a query and allows for pagination on large data sets. You should Inspect the <see cref="EntityCollection.MoreRecords"/> to determine whether or not additional pages are available.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="query">The query criteria for the retrieval.</param>
        /// <param name="pageSize">The number of entity instances returned per page,</param>
        /// <param name="pageNumber">The number of the page returned from the query.</param>
        /// <param name="pagingCookie">The info used to page large result sets. This value is returned by the <see cref="EntityCollection"/>.</param>
        /// <returns>The collection of entities returned from the query.</returns>
        public static EntityCollection RetrievePage(this IOrganizationService orgSvc, QueryExpression query, int pageSize, int pageNumber, string pagingCookie)
        {

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), string.Format(Resources.Messages.ArgumentNull, nameof(query)));
            }

            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = pageSize;
            query.PageInfo.PageNumber = pageNumber;
            query.PageInfo.PagingCookie = pagingCookie;

            return orgSvc.RetrieveMultiple(query);

        }

        /// <summary>
        /// Forces an update/refresh of a rollup field.
        /// </summary>
        /// <param name="instance">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <param name="entityRef">Target entity reference.</param>
        /// <param name="fieldName">Rollup field name.</param>
        public static void UpdateRollupField(this IOrganizationService instance, EntityReference entityRef, string fieldName)
        {

            var req = new CalculateRollupFieldRequest
            {
                Target = entityRef,
                FieldName = fieldName
            };

            var resp = (CalculateRollupFieldResponse)instance.Execute(req);

            if (resp == null || resp.Entity == null)
            {
                throw new InvalidPluginExecutionException($"Unable to update rollup field {fieldName}");
            }
        }

        /// <summary>
        /// Creates a new record or update it if record already exists.
        /// </summary>
        /// <param name="orgSvc"></param>
        /// <param name="entity">An entity instance that contains the properties to set into a new or existing record.</param>
        /// <returns>The <see cref="UpsertResponse"/>.</returns>
        public static UpsertResponse Upsert(this IOrganizationService orgSvc, Entity entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Resources.Messages.ArgumentNull, nameof(entity)));
            }

            UpsertRequest request = new UpsertRequest()
            {
                Target = entity
            };

            var resp = (UpsertResponse)orgSvc.Execute(request);

            return resp;

        }

        /// <summary>
        /// Sends a <see cref="Microsoft.Crm.Sdk.Messages.WhoAmIRequest"/>.
        /// </summary>
        /// <param name="orgSvc">An instance of the <see cref="IOrganizationService"/>.</param>
        /// <returns>A <see cref="Microsoft.Crm.Sdk.Messages.WhoAmIResponse"/>.</returns>
        public static WhoAmIResponse WhoAmIRequest(this IOrganizationService orgSvc)
        {

            var req = new WhoAmIRequest();
            return (WhoAmIResponse) orgSvc.Execute(req);

        }        

    }

}
