﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XrmUtils.Extensions.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("XrmUtils.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument {0} is null, empty or invalid..
        /// </summary>
        internal static string ArgumentNull {
            get {
                return ResourceManager.GetString("ArgumentNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Async auto delete is only supported when execution mode is set to asynchronous..
        /// </summary>
        internal static string AsyncAutoDeleteNotSupported {
            get {
                return ResourceManager.GetString("AsyncAutoDeleteNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute name is required..
        /// </summary>
        internal static string AttributeNameRequired {
            get {
                return ResourceManager.GetString("AttributeNameRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attribute type is not supported: {0}..
        /// </summary>
        internal static string AttributeTypeNotSupported {
            get {
                return ResourceManager.GetString("AttributeTypeNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Collection {0} cannot be empty..
        /// </summary>
        internal static string CollectionCannotBeEmpty {
            get {
                return ResourceManager.GetString("CollectionCannotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A search for Entity Type Code {0} didn&apos;t return any results..
        /// </summary>
        internal static string EntityTypeCodeNotFound {
            get {
                return ResourceManager.GetString("EntityTypeCodeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Asynchronous execution mode is only supported by plugins registered in the post-operation pipeline stage..
        /// </summary>
        internal static string ExecutionModeAsyncNotSupported {
            get {
                return ResourceManager.GetString("ExecutionModeAsyncNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid entity type: {0}. Expected entitie(s): {1}..
        /// </summary>
        internal static string InvalidEntityType {
            get {
                return ResourceManager.GetString("InvalidEntityType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple records were returned when only one was expected..
        /// </summary>
        internal static string MultipleRecordsReturned {
            get {
                return ResourceManager.GetString("MultipleRecordsReturned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find plugin assembly: {0}..
        /// </summary>
        internal static string PluginAssemblyNotFound {
            get {
                return ResourceManager.GetString("PluginAssemblyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find plugin type: {0}..
        /// </summary>
        internal static string PluginTypeNotFound {
            get {
                return ResourceManager.GetString("PluginTypeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property {0} is not decocorated with attribute {1}..
        /// </summary>
        internal static string PropertyMissingAttribute {
            get {
                return ResourceManager.GetString("PropertyMissingAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A property named &apos;{0}&apos; could not be found..
        /// </summary>
        internal static string PropertyNotFound {
            get {
                return ResourceManager.GetString("PropertyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A SdkMessageFilter for Message {0} and entity {1} could not be found. Please check message and entity name and try again. It is also possible that this message is not support by the specified entity..
        /// </summary>
        internal static string SdkMessageFilterNotFound {
            get {
                return ResourceManager.GetString("SdkMessageFilterNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find Sdk Message: {0}..
        /// </summary>
        internal static string SdkMessageNotFound {
            get {
                return ResourceManager.GetString("SdkMessageNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to load the &apos;{0}&apos; relationship..
        /// </summary>
        internal static string UnableToLoadRelationship {
            get {
                return ResourceManager.GetString("UnableToLoadRelationship", resourceCulture);
            }
        }
    }
}
