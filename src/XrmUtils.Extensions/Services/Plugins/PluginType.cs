using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Services.Plugins
{
    /// <summary>
    ///     Represents the type that inherits from the IPlugin interface and is contained within a plug-in assembly.
    /// </summary>
    public class PluginType
    {

        internal string _assemblyName;
        internal string _name;
        internal Guid _assemblyId;
        internal Guid _id;

        /// <summary>
        ///     Gets the unique identifier of the plug-in type.
        /// </summary>
        /// <value>
        ///     The unique identifier of the plug-in type.
        /// </value>
        public Guid Id { get { return _id; } }

        /// <summary>
        ///     Gets or sets the description of the plug-in type.
        /// </summary>
        /// <value>
        ///     The description of the plug-in type.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     User friendly name for the plug-in.
        /// </summary>
        /// <value>
        ///     The user friendly name for the plug-in.
        /// </value>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     Fully qualified type name of the plug-in type.
        /// </summary>
        /// <value>
        ///     The fully qualified type name of the plug-in type.
        /// </value>
        public string TypeName { get; set; }

        public PluginType(PluginAssembly assembly)
        {

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(assembly)));
            }

            if (assembly.Id == Guid.Empty)
            {
                throw new ArgumentException("assemblyId", string.Format(Extensions.Resources.Messages.ArgumentNull, "assemblyId"));
            }

            _assemblyId = assembly.Id;

        }

        public PluginType(string assemblyName)
        {

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentNullException(nameof(assemblyName), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(assemblyName)));
            }

            _assemblyName = assemblyName;

        }

        public PluginType(Guid assemblyId)
        {

            if (assemblyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(assemblyId), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(assemblyId)));
            }

            _assemblyId = assemblyId;

        }

        internal PluginType(Entity entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Extensions.Resources.Messages.ArgumentNull, nameof(entity)));
            }

            _id = entity.Id;
            _assemblyId =  entity.GetAttributeValue<EntityReference>("pluginassemblyid").Id;
            _name = entity.GetAttributeValue<string>("name");

            this.TypeName = entity.GetAttributeValue<string>("typename");            
            this.FriendlyName = entity.GetAttributeValue<string>("friendlyname");
            this.Description = entity.GetAttributeValue<string>("description");

        }

        internal Entity ToEntity()
        {

            Entity entity;

            if (_id != Guid.Empty)
            {
                entity = new Entity("plugintype", _id);
            }
            else
            {
                entity = new Entity("plugintype");
            }

            entity.Attributes.Add("pluginassemblyid", new EntityReference("pluginassembly", _assemblyId));
            entity.Attributes.Add("typename", this.TypeName);
            entity.Attributes.Add("name", _name ?? this.TypeName);
            entity.Attributes.Add("friendlyname", this.FriendlyName ?? Guid.NewGuid().ToString());
            entity.Attributes.Add("description", this.Description);

            return entity;

        }


    }

}
