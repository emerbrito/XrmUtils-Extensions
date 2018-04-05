using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions.Resources;

namespace XrmUtils.Extensions
{

    /// <summary>
    /// Extension methods for the CRM SDK <see cref="Microsoft.Xrm.Sdk.Entity"/> class.
    /// </summary>
    public static class EntityExtensions
    {

        /// <summary>
        /// Adds an attribute or update its value if attribute already exists.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        public static void AddOrUpdateAttribute(this Entity entity, string attributeName, object value)
        {

            entity.AssertIsNotNull(nameof(entity));
            attributeName.AssertIsNotNull(nameof(attributeName), Messages.AttributeNameRequired);

            if (!entity.Attributes.Contains(attributeName))
            {
                entity.Attributes.Add(attributeName, value);
            }
            else
            {
                entity.Attributes[attributeName] = value;
            }

        }

        /// <summary>
        /// Ensures the entity logical name is an expected value by validating it agains a list of know names. If current name is not on the list assertion fails and throws a <see cref="Microsoft.Xrm.Sdk.InvalidPluginExecutionException"/>. Look at <see cref="ValidateLogicalName"/> if you want to return a boolean value instead.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="expectedNames">The expected names.</param>
        public static void AssertLogicalName(this Entity entity, params string[] expectedNames)
        {

            expectedNames.AssertIsNotNull(nameof(expectedNames));

            string nameList = string.Join(", ", expectedNames);
            AssertLogicalName(entity, string.Format(Messages.InvalidEntityType, entity.LogicalName, nameList));

        }

        /// <summary>
        /// Ensures the entity logical name is an expected value by validating it agains a known list. If current name is not on the list assertion fails and throws a <see cref="Microsoft.Xrm.Sdk.InvalidPluginExecutionException"/>.  Look at <see cref="ValidateLogicalName"/> if you want to return a boolean value instead.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="errorMessage">The error message to be thrown if asertion fails.</param>
        /// <param name="expectedNames">The expected names.</param>
        public static void AssertLogicalName(this Entity entity, string errorMessage, string[] expectedNames)
        {

            entity.AssertIsNotNull(nameof(entity));
            errorMessage.AssertIsNotNull(nameof(errorMessage));
            expectedNames.AssertIsNotNull(nameof(expectedNames));

            if(!entity.ValidateLogicalName(expectedNames))
            {                
                throw new InvalidPluginExecutionException(errorMessage);
            }

        }

        /// <summary>
        /// Determine if an attribute has changed by comparing it to a base value from a pre entity image. Also return <c>false</c> if attribute is not found on the target entity. Attribute of type <see cref="Microsoft.Xrm.Sdk.EntityCollection"/> is not supported.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="logicalName">Attribute logical name.</param>
        /// <param name="preImage">Entity used as base for the comparison, usually a pre image.</param>
        /// <returns>True if attributed has changed.</returns>
        public static bool AttributeHasChanged(this Entity entity, string logicalName, Entity preImage)
        {

            entity.AssertIsNotNull(nameof(entity));
            logicalName.AssertIsNotNull(logicalName);
            preImage.AssertIsNotNull(nameof(preImage));            

            if(!entity.Attributes.Contains(logicalName))
            {
                return false;
            }

            if(!preImage.Contains(logicalName))
            {
                return false;
            }

            object targetAtt = entity.Attributes[logicalName];
            object baseAtt = preImage.Attributes[logicalName];

            if(targetAtt == null && baseAtt == null)
            {
                return false;
            }

            if(targetAtt == null || baseAtt == null)
            {
                return true;
            }

            if(targetAtt is EntityCollection)
            {
                throw new InvalidPluginExecutionException(string.Format(Messages.AttributeTypeNotSupported, "EntityCollection"));
            }

            if(targetAtt is BooleanManagedProperty)
            {
                return !((BooleanManagedProperty) targetAtt).Value.Equals(((BooleanManagedProperty)baseAtt).Value);
            }

            return !targetAtt.Equals(baseAtt);

        }

        ///<summary> 
        ///Gets the value of an aliased attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<returns>The attribute value.</returns> 
        public static T GetAliasedAttributeValue<T>(this Entity entity, string attributeLogicalName)
        {
            return GetAliasedAttributeValue<T>(entity, attributeLogicalName, null, defaultValue: default(T));
        }

        ///<summary> 
        ///Gets the value of an aliased attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<param name="image">Entity image (pre/post image usually available from the event pipeline).</param> 
        ///<returns>The attribute value.</returns> 
        public static T GetAliasedAttributeValue<T>(this Entity entity, string attributeLogicalName, Entity image)
        {
            return GetAliasedAttributeValue<T>(entity, attributeLogicalName, image, defaultValue: default(T));
        }

        ///<summary> 
        ///Gets the value of an aliased attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<param name="image">Entity image (pre/post image usually available from the event pipeline).</param> 
        ///<param name="defaultValue">The default value to be returned if attribute cannot be found.</param>
        ///<returns>The attribute value.</returns> 
        public static T GetAliasedAttributeValue<T>(this Entity entity, string attributeLogicalName, Entity image, T defaultValue)
        {

            entity.AssertIsNotNull(nameof(entity));
            entity.AssertIsNotNull(nameof(attributeLogicalName));

            var aliasedValue = entity.GetAttributeValue<AliasedValue>(attributeLogicalName, image);

            if(aliasedValue == null || aliasedValue.Value == null)
            {
                if(defaultValue == null)
                {
                    return default(T);
                }
                else
                {
                    return defaultValue;
                }
            }

            var value = (T) aliasedValue.Value;

            if (value == null)
            {
                if (defaultValue == null)
                {
                    return default(T);
                }
                else
                {
                    return defaultValue;
                }
            }

            return value;
        }

        ///<summary> 
        ///Gets the value of an attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<returns>The attribute value.</returns> 
        public static T GetAttributeValue<T>(this Entity entity, string attributeLogicalName)
        {
            return GetAttributeValue<T>(entity, attributeLogicalName, (Entity)null, default(T));
        }

        ///<summary> 
        ///Gets the value of an attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<param name="image">Entity image (pre/post image usually available from the event pipeline).</param> 
        ///<returns>The attribute value.</returns> 
        public static T GetAttributeValue<T>(this Entity entity, string attributeLogicalName, Entity image)
        {
            return GetAttributeValue<T>(entity, attributeLogicalName, image, default(T));
        }

        ///<summary> 
        ///Gets the value of an attribute from the target entity. If an image is specified and attribute cannot be found in the target entity, will try to retrieve it from the image instead.
        ///</summary>       
        ///<typeparam name="T">The attribute type</typeparam>         
        ///<param name="entity">The entity instance.</param> 
        ///<param name="attributeLogicalName">Logical name of the attribute</param> 
        ///<param name="image">Entity image (pre/post image usually available from the event pipeline).</param> 
        ///<param name="defaultValue">The default value to be returned if attribute cannot be found.</param>
        ///<returns>The attribute value.</returns> 
        public static T GetAttributeValue<T>(this Entity entity, string attributeLogicalName, Entity image, T defaultValue)
        {
            return entity.Contains(attributeLogicalName)
            ? entity.GetAttributeValue<T>(attributeLogicalName)
            : image != null && image.Contains(attributeLogicalName)
            ? image.GetAttributeValue<T>(attributeLogicalName)
            : defaultValue;
        }

        /// <summary>
        /// Gets the formatted value for an entity attributes. The underlying formatted values collection is only available during a retrieve operation.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributeLogicalName">Attribute logical name.</param>
        /// <returns>A <c>string</c> representing the the formatted if one is found or the default value if one is specified, otherwise or an an empty <c>string</c> if formatted value cannot be found.</returns>
        public static string GetFormattedValue(this Entity entity, string attributeLogicalName)
        {
            return GetFormattedValue(entity, attributeLogicalName, defaultValue: string.Empty);
        }

        /// <summary>
        /// Gets the formatted value for an entity attributes. The underlying formatted values collection is only available during a retrieve operation.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributeLogicalName">Attribute logical name.</param>
        /// <param name="defaultValue">Default value returned if the attribute is not present in the formatted values collection or its value is null.</param>
        /// <returns>A <c>string</c> representing the the formatted if one is found or the default value if one is specified, otherwise or an an empty <c>string</c> if formatted value cannot be found.</returns>
        public static string GetFormattedValue(this Entity entity, string attributeLogicalName, string defaultValue)
        {

            if (entity.FormattedValues == null || entity.FormattedValues.Count == 0)
            {
                return defaultValue;
            }

            if (!entity.FormattedValues.ContainsKey(attributeLogicalName))
            {
                return defaultValue;
            }

            string keyValue = entity.FormattedValues[attributeLogicalName];

            if (string.IsNullOrWhiteSpace(keyValue))
            {
                keyValue = defaultValue;
            }

            return keyValue;

        }

        /// <summary>
        /// Checks whether an attribute  value is null, empty or attribute doesn't exist in the collection.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributeLogicalName">Attribute logical name.</param>
        /// <returns><c>true</c> if attribute value is null, empty or attribute cannot be found, otherwise <c>false</c>.</returns>
        public static bool IsNullEmptyOrNotFound(this Entity entity, string attributeLogicalName)
        {

            entity.AssertIsNotNull(nameof(entity));
            attributeLogicalName.AssertIsNotNull(nameof(attributeLogicalName));

            if (!entity.Attributes.Contains(attributeLogicalName))
            {
                return true;
            }

            if (entity.Attributes[attributeLogicalName] == null)
            {
                return true;
            }
            else if (entity.Attributes[attributeLogicalName] is string && string.IsNullOrWhiteSpace((string) entity.Attributes[attributeLogicalName]))
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Checks whether an attribute  value is null, empty or attribute doesn't exist in the collection.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributeLogicalName">Attribute logical name.</param>
        /// <param name="image">A fall back entity image. Entity will be searched if attribute cannot be found on target image.</param>
        /// <returns>
        /// <returns><c>true</c> if attribute value is null, empty or attribute cannot be found, otherwise <c>false</c>.</returns>
        /// </returns>
        public static bool IsNullEmptyOrNotFound(this Entity entity, string attributeLogicalName, Entity image)
        {

            entity.AssertIsNotNull(nameof(entity));
            attributeLogicalName.AssertIsNotNull(nameof(attributeLogicalName));

            if (entity.IsNullEmptyOrNotFound(attributeLogicalName))
            {
                if (image != null && image.IsNullEmptyOrNotFound(attributeLogicalName))
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>
        /// Merge attributes into this entity instance. Replace values of existing attributes unless <c>replaceValues</c> is set to <c>false</c>.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="sourceEntity">The source entity.</param>
        public static void MergeAttributes(this Entity entity, Entity sourceEntity)
        {
            MergeAttributes(entity, sourceEntity, replaceValues: true);
        }

        /// <summary>
        /// Merge attributes into this entity instance. Replace values of existing attributes unless <c>replaceValues</c> is set to <c>false</c>.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="replaceValues">When set to <c>true</c>, existing attribute values will be replaced with values from <c>sourceEntity</c>.</param>
        public static void MergeAttributes(this Entity entity, Entity sourceEntity, bool replaceValues)
        {

            entity.AssertIsNotNull(nameof(entity));
            sourceEntity.AssertIsNotNull(nameof(sourceEntity));

            if (sourceEntity.Attributes == null)
            {
                return;
            }

            foreach (var att in sourceEntity.Attributes)
            {
                if(replaceValues)
                {
                    entity.AddOrUpdateAttribute(att.Key, att.Value);
                }
                else
                {
                    if(!entity.Contains(att.Key))
                    {
                        entity.Attributes.Add(att.Key, att.Value);
                    }
                }
                
            }

        }

        /// <summary>
        /// Serialize the entity instance into a JSON string.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <returns></returns>
        public static string ToJson(this Entity entity)
        {
            return ToJson(entity, attributesOnly: false);
        }

        /// <summary>
        /// Serialize the entity instance into a JSON string.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="attributesOnly">If set to <c>true</c> only the attribute dictionary will be serialized.</param>
        /// <returns></returns>
        public static string ToJson(this Entity entity, bool attributesOnly)
        {

            entity.AssertIsNotNull(nameof(entity));

            string json = string.Empty;

            if (attributesOnly && entity.Attributes != null)
            {
                json = SerializationUtility.ToJson<AttributeCollection>(entity.Attributes);
            }
            else
            {
                json = SerializationUtility.ToJson<Entity>(entity);
            }

            return json;

        }

        /// <summary>
        /// Validates the entity logical name against a list of expected values. See <see cref="AssertLogicalName"/> if you want to stop execution when validation fails.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="expectedEntityNames">List of expected entity names.</param>
        /// <returns>False if entity name is not a match.</returns>
        public static bool ValidateLogicalName(this Entity entity, params string[] expectedEntityNames)
        {

            entity.AssertIsNotNull(nameof(entity));
            expectedEntityNames.AssertIsNotNull(nameof(expectedEntityNames));

            if (!expectedEntityNames.Any())
            {
                return false;
            }

            string curName = entity.LogicalName;

            if (expectedEntityNames.Contains<string>(curName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// For internal use. Return a string representing the attribute value.
        /// </summary>
        /// <param name="attribute">The entity attribute.</param>
        /// <param name="attributeName">Name attribute logical name.</param>
        /// <param name="entity">The entity containing the attribute.</param>
        /// <returns></returns>
        private static string ParseAttributeToString(object attribute, string attributeName, Entity entity, bool printAttName = true)
        {

            attribute.AssertIsNotNull(nameof(attribute));
            attributeName.AssertIsNotNull(nameof(attributeName));
            entity.AssertIsNotNull(nameof(entity));

            string attName = printAttName ? $"{attributeName}: " : string.Empty;

            string result;

            if (attribute is Money)
            {
                result = $"{attName}{((Money) attribute).Value.ToString()}";
            }
            else if (attribute is OptionSetValue)
            {
                string label = entity.GetFormattedValue(attributeName);
                string optionValue = ((OptionSetValue) attribute).Value.ToString();

                if (string.IsNullOrWhiteSpace(label))
                    label = "<null>";

                result = $"{attName}(value: {optionValue}) label: {label}";
            }
            else if (attribute is EntityReference)
            {
                EntityReference eref = (EntityReference) attribute;
                string erefName = string.IsNullOrWhiteSpace(eref.Name) ? "<null>" : eref.Name;
                result = $"{attName}({eref.LogicalName} id: {eref.Id.ToString()}) name: {erefName}";
            }
            else if (attribute is AliasedValue)
            {
                AliasedValue alv = (AliasedValue) attribute;
                result = $"{attName}(aliased value: {alv.EntityLogicalName}){ParseAttributeToString(alv.Value, attributeName, entity, printAttName: false)}";
            }
            else
            {
                result = $"{attName}{attribute.ToString()}";
            }

            return result;

        }

    }
}
