using Microsoft.Xrm.Sdk;
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
    ///     Represents a stage in the execution pipeline that a plug-in is to execute.
    /// </summary>
    public class PluginStep
    {

        internal Guid _id;
        private Guid _messageId;
        private Guid _pluginTypeId;
        private Guid _sdkMessageFilterId;
        private PluginType _pluginType;

        /// <summary>
        ///     Get the unique identifier of the SDK message processing step entity.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public Guid Id { get { return _id; } }

        /// <summary>
        ///     Indicates whether the asynchronous system job is automatically deleted on completion.
        /// </summary>
        /// <value>
        ///     True if asynchronous automatic delete, false if not.
        /// </value>
        public bool AsyncAutoDelete { get; set; }

        /// <summary>
        ///     Description of the SDK message processing step.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Deployment that the SDK message processing step should be executed on; server, client, or both.
        /// </summary>
        /// <value>
        ///     The supported deployment.
        /// </value>
        public SupportedDeployment SupportedDeployment { get; set; } = SupportedDeployment.ServerOnly;

        /// <summary>
        ///     Run-time mode of execution, for example, synchronous or asynchronous.
        /// </summary>
        /// <value>
        ///     The execution mode.
        /// </value>
        public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Synchronous;

        /// <summary>
        ///      Processing order within the stage.
        /// </summary>
        /// <value>
        ///     The execution order.
        /// </value>
        public int ExecutionOrder { get; set; } = 1;

        /// <summary>
        ///     Comma-separated list of attributes. If at least one of these attributes is modified, the plug-in should execute.
        /// </summary>
        /// <value>
        ///     The filtering attributes.
        /// </value>
        public string FilteringAttributes { get; set; }

        /// <summary>
        ///     Gets or sets the SDK message name, for example: Update.
        /// </summary>
        /// <value>
        ///     The SDK message name.
        /// </value>
        public string SdkMessage { get; set; }

        /// <summary>
        ///      Stage in the execution pipeline that the SDK message processing step is in.
        /// </summary>
        /// <value>
        ///     The pipeline stage.
        /// </value>
        public PipelineStage PipelineStage { get; set; } = PipelineStage.PostOperation;

        /// <summary>
        ///     Gets or sets the primary entity.
        /// </summary>
        /// <value>
        ///     The primary entity.
        /// </value>
        public string PrimaryEntity { get; set; }

        /// <summary>
        ///     Name of SdkMessage processing step.
        /// </summary>
        /// <value>
        ///     The SdkMessage processing step name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     The user to impersonate context when step is executed.
        /// </summary>
        /// <value>
        ///     The user to impersonate context when step is executed.
        /// </value>
        public Guid ImpersonatingUserId { get; set; }

        /// <summary>
        ///     Step-specific unsecure configuration for the plug-in type. Passed to the plug-in constructor at run time.
        /// </summary>
        /// <value>
        ///     The unsecure configuration.
        /// </value>
        public string UnsecureConfig { get; set; }

        public PluginStep(PluginType pluginType)
        {

            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(pluginType)));
            }

            if (pluginType.Id == Guid.Empty)
            {
                throw new ArgumentException("pluginType.Id", string.Format(Extensions.Resources.Messages.ArgumentNull, "pluginType.Id"));
            }

            _pluginType = pluginType;
            _pluginTypeId = pluginType.Id;

        }

        public PluginStep(Guid pluginTypeId)
        {

            if (pluginTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(pluginTypeId), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(pluginTypeId)));
            }

            _pluginTypeId = pluginTypeId;

        }

        internal PluginStep(Entity entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(entity)));
            }

            _id = entity.Id;
            _messageId = entity.GetAttributeValue<EntityReference>("sdkmessageid").Id;
            _pluginTypeId = entity.GetAttributeValue<EntityReference>("plugintypeid").Id;
            _sdkMessageFilterId = entity.GetAttributeValue<EntityReference>("sdkmessagefilterid").Id;

            ExecutionMode = (ExecutionMode) entity.GetAttributeValue<OptionSetValue>("mode").Value;

            if (ContainAndIsNotNull("asyncautodelete", entity))
            {
                AsyncAutoDelete = entity.GetAttributeValue<bool>("asyncautodelete");
            }

            if (ContainAndIsNotNull("impersonatinguserid", entity))
            {
                ImpersonatingUserId = entity.GetAttributeValue<EntityReference>("impersonatinguserid").Id;
            }

            if (ContainAndIsNotNull("supporteddeployment", entity))
            {
                this.SupportedDeployment = (SupportedDeployment) entity.GetAttributeValue<OptionSetValue>("supporteddeployment").Value;
            }

            Name = entity.GetAttributeValue<string>("name");
            Description = entity.GetAttributeValue<string>("description");
            FilteringAttributes = entity.GetAttributeValue<string>("filteringattributes");
            ExecutionOrder = entity.GetAttributeValue<int>("rank");
            UnsecureConfig = entity.GetAttributeValue<string>("configuration");
            PipelineStage = (PipelineStage) entity.GetAttributeValue<OptionSetValue>("stage").Value;

            this.SdkMessage = (string) entity.GetAttributeValue<AliasedValue>("sdkmessage.name").Value;
            this.PrimaryEntity = (string) entity.GetAttributeValue<AliasedValue>("sdkmessagefilter.primaryobjecttypecode").Value;

        }

        internal Entity ToEntity()
        {

            Entity entity;

            if(_id == Guid.Empty)
            {
                entity = new Entity("sdkmessageprocessingstep");
            }
            else
            {
                entity = new Entity("sdkmessageprocessingstep", _id);
            }

            Validate();

            if (PipelineStage == PipelineStage.PostOperation)
            {
                entity.Attributes.Add("mode", new OptionSetValue((int) ExecutionMode));
            }
            else
            {
                entity.Attributes.Add("mode", new OptionSetValue((int) ExecutionMode.Synchronous));
            }

            if (ExecutionMode == ExecutionMode.Asynchronous)
            {
                entity.Attributes.Add("asyncautodelete", AsyncAutoDelete);
            }
            else
            {
                entity.Attributes.Add("asyncautodelete", false);
            }

            if (ImpersonatingUserId == Guid.Empty)
            {
                entity.Attributes.Add("impersonatinguserid", null);
            }
            else
            {
                entity.Attributes.Add("impersonatinguserid", new EntityReference("systemuser", ImpersonatingUserId));
            }

            entity.Attributes.Add("name", Name);
            entity.Attributes.Add("description", Description);
            entity.Attributes.Add("filteringattributes", FilteringAttributes);
            entity.Attributes.Add("rank", ExecutionOrder);
            entity.Attributes.Add("configuration", this.UnsecureConfig);
            entity.Attributes.Add("supporteddeployment", new OptionSetValue((int) SupportedDeployment));

            entity.Attributes.Add("stage", new OptionSetValue((int) PipelineStage));

            entity.Attributes.Add("sdkmessageid", new EntityReference("sdkmessage", _messageId));
            entity.Attributes.Add("plugintypeid", new EntityReference("plugintype", _pluginTypeId));

            entity.Attributes.Add("sdkmessagefilterid", new EntityReference("sdkmessagefilter", _sdkMessageFilterId));

            //TODO: Implement support for secure configuration
            //entity.Attributes.Add("sdkmessageprocessingstepsecureconfigid", new EntityReference("sdkmessageprocessingstepsecureconfig"), secureConfigId);

            return entity;

        }

        internal void ResolveProperties(PluginService service)
        {

            if (_messageId == Guid.Empty)
            {
                _messageId = service.GetSdkMessage(SdkMessage).Id;
            }

            if (_sdkMessageFilterId == Guid.Empty)
            {
                _sdkMessageFilterId = service.GetSdkMessageFilter(_messageId, PrimaryEntity).Id;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {

                if (_pluginType == null || string.IsNullOrWhiteSpace(_pluginType.TypeName))
                {

                    _pluginType = service.GetPluginType(_pluginTypeId);

                    if (_pluginType == null)
                    {
                        throw new KeyNotFoundException(string.Format(Messages.PluginTypeNotFound, _pluginTypeId.ToString()));
                    }

                }

                this.Name = string.Format("{0}: {1} of {2}", _pluginType.TypeName, SdkMessage, PrimaryEntity);

            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                Description = this.Name;
            }

        }

        private void Validate()
        {

            if (ExecutionMode == ExecutionMode.Asynchronous && PipelineStage != PipelineStage.PostOperation)
            {
                throw new NotSupportedException(Messages.ExecutionModeAsyncNotSupported);
            }

            if (ExecutionMode != ExecutionMode.Asynchronous && AsyncAutoDelete == true)
            {
                throw new NotSupportedException(Messages.AsyncAutoDeleteNotSupported);
            }

        }

        private bool ContainAndIsNotNull(string key, Entity entity)
        {
            return entity.Attributes.ContainsKey(key) && entity.Attributes[key] != null;
        }

    }
}
