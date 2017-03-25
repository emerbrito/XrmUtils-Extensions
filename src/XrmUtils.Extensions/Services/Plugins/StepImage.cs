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
    ///     Copy of an entity's attributes before or after the core system operation.
    /// </summary>
    public class StepImage
    {

        internal Guid _id;
        internal Guid _stepId;
        private string _attributes;

        /// <summary>
        ///     Gets the image unique identifier.
        /// </summary>
        /// <value>
        ///     The image unique identifier.
        /// </value>
        public Guid Id { get { return _id; } }

        /// <summary>
        ///     Gets or sets the image type.
        /// </summary>
        ///
        /// <value>
        ///     The image type.
        /// </value>
        public ImageType ImageType { get; set; } = ImageType.PreImage;

        /// <summary>
        ///     Gets or sets the name of SdkMessage processing step image.
        /// </summary>
        /// <value>
        ///     The name image name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the key name used to access the pre-image or post-image property bags in a step.
        /// </summary>
        /// <value>
        ///     The image alias.
        /// </value>
        public string EntityAlias { get; set; }

        /// <summary>
        ///     Gets or sets a comma-separated list of attributes that are to be passed into the SDK message processing step image.
        /// </summary>
        /// <value>
        ///     The comma-separated list of attributes.
        /// </value>
        public string Attributes
        {
            get { return _attributes; }
            set { _attributes = value.Replace(" ", ""); }
        }

        public StepImage(Guid stepId)
        {
            _stepId = stepId;
        }

        public StepImage(PluginStep step)
        {
            _stepId = step.Id;
        }

        internal StepImage(Entity entity)
        {

            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity), string.Format(Messages.ArgumentNull, nameof(entity)));
            }

            _id = entity.Id;
            _stepId = entity.GetAttributeValue<EntityReference>("sdkmessageprocessingstepid").Id;

            Name = entity.GetAttributeValue<string>("name");
            ImageType = (ImageType) entity.GetAttributeValue<OptionSetValue>("imagetype").Value;
            EntityAlias = entity.GetAttributeValue<string>("entityalias");
            Attributes = entity.GetAttributeValue<string>("attributes");            

        }

        internal Entity ToEntity()
        {

            Entity entity;

            if (_id != Guid.Empty)
            {
                entity = new Entity("sdkmessageprocessingstepimage", _id);
            }
            else
            {
                entity = new Entity("sdkmessageprocessingstepimage");
            }

            entity.Attributes.Add("name", Name);
            entity.Attributes.Add("imagetype", new OptionSetValue((int) ImageType));            
            entity.Attributes.Add("entityalias", EntityAlias);
            entity.Attributes.Add("attributes", Attributes);
            entity.Attributes.Add("messagepropertyname", "Target");
            entity.Attributes.Add("sdkmessageprocessingstepid", new EntityReference("sdkmessageprocessingstep", _stepId));

            return entity;

        }


    }
}
