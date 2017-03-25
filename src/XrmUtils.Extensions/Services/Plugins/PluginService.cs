using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions;
using XrmUtils.Extensions.Resources;

namespace XrmUtils.Services.Plugins
{
    /// <summary>
    ///     A plugin service.
    /// </summary>
    public class PluginService
    {

        private IOrganizationService _orgSvc;
        private string[] assemblyCols = new string[] { "name", "culture", "version", "publickeytoken", "sourcetype", "isolationmode" };
        private string[] typeCols = new string[] { "pluginassemblyid", "typename", "name", "friendlyname", "description" };

        public PluginService(IOrganizationService orgService)
        {

            if (orgService == null)
            {
                throw new ArgumentNullException(nameof(orgService), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(orgService)));
            }

            _orgSvc = orgService;

        }

        /// <summary>
        ///     Sets the status of the SDK message processing step.
        /// </summary>
        /// <param name="stepId">The step unique identifier.</param>
        /// <param name="enabled">True to enable, false to disable.</param>
        public void SetStepStatus(Guid stepId, bool enabled)
        {

            var entity = new Entity("sdkmessageprocessingstep", stepId);
            entity.Attributes.Add("statecode", new OptionSetValue(enabled ? 0 : 1));

            _orgSvc.Update(entity);

        }

        /// <summary>
        ///     Gets an assembly registration.
        /// </summary>
        /// <param name="name">The assembly name (usually same as file name without extensions).</param>
        /// <returns>
        ///     The assembly registration.
        /// </returns>
        public PluginAssembly GetAssembly(string name)
        {
            return GetAssembly(name, includeContent: false);
        }

        /// <summary>
        ///     Gets an assembly registration.
        /// </summary>
        /// <param name="name">The assembly name (usually same as file name without extensions).</param>
        /// <param name="includeContent">If True, property <see cref="PluginAssembly.Content"/> will be populated with the actual assembly bytes as a base64 string. Otherwise <see cref="PluginAssembly.Content"/> property will be empty.</param>
        /// <returns>
        ///     The assembly registration.
        /// </returns>
        public PluginAssembly GetAssembly(string name, bool includeContent)
        {

            PluginAssembly assembly = null;

            var conditions = new List<ConditionExpression> { new ConditionExpression("name", ConditionOperator.Equal, name) };
            var columns = new ColumnSet(assemblyCols);

            if (includeContent) columns.AddColumn("content");

            var results = _orgSvc.RetrieveMultiple("pluginassembly", conditions, columns);

            if (results != null && results.Entities != null)
            {
                var entity = results.Entities.FirstOrDefault();
                if(entity != null)
                {
                    assembly = new PluginAssembly(entity);
                }                
            }

            return assembly;

        }

        public PluginAssembly GetAssembly(Guid assemblyId)
        {
            return GetAssembly(assemblyId, includeContent: false);
        }

        /// <summary>
        ///     Gets an assembly registration.
        /// </summary>
        /// <param name="assemblyId">The assembly registration ID.</param>
        /// <param name="includeContent">If True, property <see cref="PluginAssembly.Content"/> will be populated with the actual assembly bytes as a base64 string. Otherwise <see cref="PluginAssembly.Content"/> property will be empty.</param>
        /// <returns>
        ///     The assembly registration.
        /// </returns>
        public PluginAssembly GetAssembly(Guid assemblyId, bool includeContent)
        {

            PluginAssembly assembly = null;

            var columns = new ColumnSet(assemblyCols);
            if (includeContent) columns.AddColumn("content");
           
            var entity = _orgSvc.Retrieve("pluginassembly", assemblyId, columns, allowNull: true);

            if(entity != null)
            {
                assembly = new PluginAssembly(entity);
            }

            return assembly;

        }

        /// <summary>
        ///     Gets a collection of SDK message processing steps.
        /// </summary>
        /// <param name="sdkMessage">The SDK message. </param>
        /// <param name="entity">The entity logical name.</param>
        /// <param name="pluginTypeId">Identifier for the plugin type.</param>
        /// <returns>
        ///     A collection of Step registration.
        /// </returns>
        public IEnumerable<PluginStep> GetSteps(string sdkMessage, string entity, Guid pluginTypeId)
        {

            List<PluginStep> steps = new List<PluginStep>();

            var columns = new ColumnSet(true);
            var query = this.BuildPluginStepQueryExpression();

            query.Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                {
                    new ConditionExpression("plugintypeid", ConditionOperator.Equal, pluginTypeId),
                    new ConditionExpression("sdkmessage", "name", ConditionOperator.Equal, sdkMessage),
                    new ConditionExpression("sdkmessagefilter", "primaryobjecttypecode", ConditionOperator.Equal, entity),
                }
            };

            var results = _orgSvc.RetrieveMultiple(query);

            if (results != null && results.Entities != null && results.Entities.Count > 0)
            {
                foreach (var item in results.Entities)
                {
                    steps.Add(new PluginStep(item));
                }
            }

            return steps;

        }

        /// <summary>
        ///     Gets a SDK message processing step registration.
        /// </summary>
        /// <param name="stepId">The step unique identifier.</param>
        /// <returns>
        ///     The SDK message processing step registration.
        /// </returns>
        public PluginStep GetStep(Guid stepId)
        {

            PluginStep step = null;
            var columns = new ColumnSet(true);
            var query = this.BuildPluginStepQueryExpression();

            query.Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                    {
                        new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.Equal, stepId)
                    }
            };

            var results = _orgSvc.RetrieveMultiple(query);

            if (results != null && results.Entities != null && results.Entities.Count > 0)
            {
                step = new PluginStep(results.Entities.First());
            }

            return step;

        }

        /// <summary>
        ///     Gets a plugin type registration.
        /// </summary>
        /// <param name="typeName">The Fully qualified type name of the plug-in type.</param>
        /// <param name="assemblyName">The name of the assembly containing the plugin type (usually file same as file name without extensions).</param>
        /// <returns>
        ///     The plugin type registration.
        /// </returns>
        public PluginType GetPluginType(string typeName, string assemblyName)
        {

            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentNullException(nameof(typeName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(typeName)));
            }

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentNullException(nameof(assemblyName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(assemblyName)));
            }

            EntityCollection results;
            List<ConditionExpression> conditions;
            PluginType pluginType = null;
            PluginAssembly assembly = GetAssembly(assemblyName, includeContent: false); 

            if (assembly == null)
            {
                throw new KeyNotFoundException(string.Format(Extensions.Resources.Messages.PluginAssemblyNotFound, nameof(assemblyName)));
            }

            conditions = new List<ConditionExpression>
            {
                new ConditionExpression("typename", ConditionOperator.Equal, typeName),
                new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assembly.Id)
            };

            results = _orgSvc.RetrieveMultiple("plugintype", conditions, new ColumnSet(typeCols));

            if(results != null && results.Entities != null)
            {
                Entity entity = results.Entities.FirstOrDefault();
                if(entity != null)
                {
                    pluginType = new PluginType(entity);
                }
            }

            return pluginType;

        }

        /// <summary>
        ///     Gets a plugin type registration.
        /// </summary>
        /// <param name="pluginTypeId">The unique identifier for the plugin type registration.</param>
        /// <returns>
        ///     The plugin type registration.
        /// </returns>
        public PluginType GetPluginType(Guid pluginTypeId)
        {

            PluginType pluginType = null;
            var columns = new ColumnSet(typeCols);

            var entity = _orgSvc.Retrieve("plugintype", pluginTypeId, columns, allowNull: true);

            if (entity != null)
            {
                pluginType = new PluginType(entity);
            }

            return pluginType;

        }

        /// <summary>
        ///     Registers the assembly containing  one or more plugins.
        /// </summary>
        /// <param name="assembly">The assembly to be registered.</param>
        /// <returns>
        ///     Unique identifier of the registered assembly.
        /// </returns>
        public Guid RegisterAssembly(PluginAssembly assembly)
        {

            Guid newId;

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(assembly)));
            }

            newId = _orgSvc.Create(assembly.ToEntity());

            assembly._id = newId;

            return newId;

        }

        /// <summary>
        ///     Registers the plugin type contained by an already registered assembly.
        /// </summary>
        /// <param name="pluginType">The plu-in type to register.</param>
        /// <returns>
        ///     Unique identifier of the registered plug-in type.
        /// </returns>
        public Guid RegisterPluginType(PluginType pluginType)
        {

            Guid newId;

            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(pluginType)));
            }

            newId = _orgSvc.Create(pluginType.ToEntity());

            pluginType._id = newId;

            return newId;

        }

        /// <summary>
        ///     Registers the SDK message processing step.
        /// </summary>
        /// <param name="pluginStep">The step to be registerd.</param>
        /// <returns>
        ///     Unique identifier of the registered step.
        /// </returns>
        public Guid RegisterStep(PluginStep pluginStep)
        {

            Guid newId;

            if (pluginStep == null)
            {
                throw new ArgumentNullException(nameof(pluginStep), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(pluginStep)));
            }

            pluginStep.ResolveProperties(this);

            newId = _orgSvc.Create(pluginStep.ToEntity());

            pluginStep._id = newId;

            return newId;       

        }

        /// <summary>
        ///     Registers the step image.
        /// </summary>
        /// <param name="image">The image to be registered.</param>
        /// <returns>
        ///     Unique identifier of the registered image.
        /// </returns>
        public Guid RegisterImage(StepImage image)
        {

            Guid newId;

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(image)));
            }

            newId = _orgSvc.Create(image.ToEntity());

            image._id = newId;

            return newId;

        }

        /// <summary>
        ///     Unregisters the assembly described by assemblyId.
        /// </summary>
        /// <param name="assemblyId">The assembly registration ID. </param>
        public void UnregisterAssembly(Guid assemblyId)
        {
            _orgSvc.Delete("pluginassembly", assemblyId);
        }

        /// <summary>
        ///     Unregisters the plugin type described by typeId.
        /// </summary>
        /// <param name="typeId">Identifier for the type to unregister.</param>
        public void UnregisterPluginType(Guid typeId)
        {
            _orgSvc.Delete("plugintype", typeId);
        }

        /// <summary>
        ///     Unregisters the SDK message processing step described by stepId.
        /// </summary>
        /// <param name="stepId"> The unique identifier for the step to unregister.</param>
        public void UnregisterStep(Guid stepId)
        {
            _orgSvc.Delete("sdkmessageprocessingstep", stepId);
        }

        /// <summary>
        ///     Unregisters the image described by imageId.
        /// </summary>
        /// <param name="imageId">Identifier for the image to be unregistered.</param>
        public void UnregisterImage(Guid imageId)
        {
            _orgSvc.Delete("sdkmessageprocessingstepimage", imageId);
        }

        internal EntityReference GetSdkMessage(string sdkMessageName)
        {

            if (string.IsNullOrWhiteSpace(sdkMessageName))
            {
                throw new ArgumentNullException(string.Format(Messages.ArgumentNull, nameof(sdkMessageName), nameof(sdkMessageName)));
            }

            var cols = new ColumnSet("sdkmessageid", "name");
            var conditions = new List<ConditionExpression>()
                {
                    new ConditionExpression("name", ConditionOperator.Equal, sdkMessageName)
                };
            
            var resp = _orgSvc.RetrieveMultiple("sdkmessage", conditions, cols);

            if (resp.Entities == null || resp.Entities.Count == 0)
            {
                throw new KeyNotFoundException(string.Format(Messages.SdkMessageNotFound, sdkMessageName));
            }

            return resp.Entities.First().ToEntityReference();

        }

        internal Entity GetSdkMessageFilter(Guid sdkMessageId, string entityName)
        {

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(string.Format(Messages.ArgumentNull, nameof(entityName), nameof(entityName)));
            }

            var columns = new ColumnSet("sdkmessagefilterid", "primaryobjecttypecode");
            var conditions = new List<ConditionExpression>()
                {
                    new ConditionExpression("primaryobjecttypecode", ConditionOperator.Equal, entityName),
                    new ConditionExpression("sdkmessageid", ConditionOperator.Equal, sdkMessageId)
                };

            var resp = _orgSvc.RetrieveMultiple("sdkmessagefilter", conditions, columns);

            if (resp.Entities == null || resp.Entities.Count == 0)
            {
                throw new KeyNotFoundException(string.Format(Messages.SdkMessageFilterNotFound, sdkMessageId.ToString(), entityName));
            }

            return resp.Entities.First();

        }

        private QueryExpression BuildPluginStepQueryExpression()
        {

            var query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet(true),
            };

            var messageLink = new LinkEntity("sdkmessageprocessingstep", "sdkmessage", "sdkmessageid", "sdkmessageid", JoinOperator.Inner);
            var messageFilterLink = new LinkEntity("sdkmessageprocessingstep", "sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid", JoinOperator.Inner);

            messageLink.Columns = new ColumnSet("name");
            messageLink.EntityAlias = "sdkmessage";
            messageFilterLink.Columns = new ColumnSet("primaryobjecttypecode");
            messageFilterLink.EntityAlias = "sdkmessagefilter";

            query.LinkEntities.Add(messageLink);
            query.LinkEntities.Add(messageFilterLink);

            return query;

        }

    }
}
