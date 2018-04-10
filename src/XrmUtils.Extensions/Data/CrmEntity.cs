using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions;
using XrmUtils.Extensions.Resources;

namespace XrmUtils.Extensions.Data
{
    /// <summary>
    /// Base for proxy classes.
    /// </summary>
    public abstract class CrmEntity : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {

        #region Public Declarations

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region Constructors

        public CrmEntity(string entityLogicalName) : base(entityLogicalName)
        {
        }

        public CrmEntity(string entityLogicalName, Guid id) : base(entityLogicalName, id)
        {
        }

        #endregion

        #region Public Methods

        public string GetAttributeName(string propertyName)
        {

            IEnumerable<AttributeLogicalNameAttribute> attributes;
            AttributeLogicalNameAttribute attribute;
            PropertyInfo property = this.GetType().GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException(string.Format(Messages.PropertyNotFound, propertyName));
            }

            attributes = property.GetCustomAttributes(inherit: false).OfType<AttributeLogicalNameAttribute>();
            if (attributes == null || attributes.Count() == 0)
            {
                throw new ArgumentException(string.Format(Messages.PropertyMissingAttribute, propertyName, nameof(AttributeLogicalNameAttribute)));
            }

            attribute = attributes.First();

            return attribute.LogicalName;

        }

        public ColumnSet GetColumnSet()
        {

            IEnumerable<AttributeLogicalNameAttribute> attributes;
            var columns = new ColumnSet();
            
            attributes = this.GetType().GetCustomAttributes(inherit: false).OfType<AttributeLogicalNameAttribute>();
            if (attributes == null || attributes.Count() == 0)
            {
                return columns;
            }

            columns.AddColumns(attributes.Select(a => a.LogicalName).ToArray());

            return columns;

        }

        #endregion

        #region Protected Methods

        protected T GetPropertyValue<T>(string propertyName)
        {

            string attLogicalName = this.GetAttributeName(propertyName);
            return this.GetAttributeValue<T>(propertyName);

        }

        protected T GetPropertyValue<T>(string propertyName, Entity image)
        {

            string attLogicalName = this.GetAttributeName(propertyName);
            return this.GetAttributeValue<T>(propertyName, image);
        }

        protected T GetPropertyValue<T>(string propertyName, Entity image, T defaultValue)
        {            
            string attLogicalName = this.GetAttributeName(propertyName);
            return this.GetAttributeValue<T>(propertyName, image, defaultValue);
        }

        protected TEnum? GetEnumAttributeValue<TEnum>(string attributeLogicalName, TEnum? defaultValue = null) where TEnum : struct, IConvertible
        {

            var att = GetAttributeValue<OptionSetValue>(attributeLogicalName);

            if (att == null)
            {
                if (defaultValue.HasValue)
                {
                    return defaultValue;
                }
                else
                {
                    return null;
                }
            }

            return (TEnum)(object)att.Value;
        }

        protected void SetEnumPropertyValue<TEnum>(string propertyName, string attributeName, TEnum? value, TEnum? defaultValue = null)
            where TEnum : struct, IConvertible
        {

            var opvalue = default(OptionSetValue);

            if (value.HasValue)
            {
                opvalue = new OptionSetValue((int)(object)value);
            }
            else if (defaultValue.HasValue)
            {
                opvalue = new OptionSetValue((int)(object)defaultValue);
            }

            SetPropertyValue(nameof(propertyName), attributeName, opvalue);
        }

        protected void SetIdPropertyValue(string propertyName, string attributeName, Guid? value)
        {
            SetPropertyValue(propertyName, attributeName, value);
            base.Id = value ?? Guid.Empty;
        }

        protected void SetPropertyValue(string propertyName, string attributeName, object value)
        {
            this.OnPropertyChanging(propertyName);
            this.AddOrUpdateAttribute(attributeName, value);
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion

    }

}
