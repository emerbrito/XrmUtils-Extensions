using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Services.Plugins
{
    /// <summary>
    ///     Represents an assembly that contains one or more plug-in types.
    /// </summary>
    public class PluginAssembly
    {

        internal Guid _id;
        private SourceType _sourceType = SourceType.Database;

        /// <summary>
        ///     Gets the assembly unique identifier.
        /// </summary>
        ///
        /// <value>
        ///     The assembly unique identifier.
        /// </value>
        public Guid Id { get { return _id; } }

        /// <summary>
        ///     Name of the plug-in assembly.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Culture code for the plug-in assembly.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        ///     Version number of the assembly. The value can be obtained from the assembly through reflection.
        /// </summary>
        /// <value>
        ///     The version number of the assembly.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        ///      Public key token of the assembly. This value can be obtained from the assembly by using reflection.
        /// </summary>
        /// <value>
        ///     The public key token.
        /// </value>
        public string PublicKeyToken { get; set; }

        /// <summary>
        ///     Information about how the plugin assembly is to be isolated at execution time; None / Sandboxed.
        /// </summary>
        /// <value>
        ///     The isolation mode.
        /// </value>
        public IsolationMode IsolationMode { get; set; }

        /// <summary>
        ///     Bytes of the assembly, in Base64 format.
        /// </summary>
        /// <value>
        ///     The bytes of the assembly, in Base64 format.
        /// </value>
        public string Content { get; set; }

        public PluginAssembly()
        {
            _id = Guid.Empty;
        }

        internal PluginAssembly(Guid assemblyId)
        {
            _id = assemblyId;
        }

        internal PluginAssembly(Entity entity)
        {

            _id = entity.Id;

            this.Name = entity.GetAttributeValue<string>("name");
            this.Culture = entity.GetAttributeValue<string>("culture");
            this.Version = entity.GetAttributeValue<string>("version");
            this.PublicKeyToken = entity.GetAttributeValue<string>("publickeytoken");
            this._sourceType = (SourceType) entity.GetAttributeValue<OptionSetValue>("sourcetype").Value;
            this.IsolationMode = (IsolationMode) entity.GetAttributeValue<OptionSetValue>("isolationmode").Value;
            this.Content = entity.GetAttributeValue<string>("content");

        }

        internal Entity ToEntity()
        {

            Entity entity;

            if(_id != Guid.Empty)
            {
                entity = new Entity("pluginassembly", _id);
            }
            else
            {
                entity = new Entity("pluginassembly");
            }

            entity.Attributes.Add("name", this.Name);
            entity.Attributes.Add("culture", this.Culture);
            entity.Attributes.Add("version", this.Version);
            entity.Attributes.Add("publickeytoken", this.PublicKeyToken);
            entity.Attributes.Add("sourcetype", new OptionSetValue((int) _sourceType));
            entity.Attributes.Add("isolationmode", new OptionSetValue((int) this.IsolationMode));

            if(!string.IsNullOrWhiteSpace(this.Content))
            {
                entity.Attributes.Add("content", this.Content);
            }            

            return entity;

        }

    }
}
