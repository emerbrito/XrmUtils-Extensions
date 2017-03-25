using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions;
using XrmUtils.Services.Plugins;

namespace SampleCode
{
    class PluginRegistrationSamples
    {

        public void RegisterPluginAssembly(IOrganizationService orgSvc)
        {

            // This example register an assembly from file "XrmUtils.TestPlugin.dll".

            Guid assemblyId;

            // Full path of the assembly to be registered.
            string path = Path.Combine(Environment.CurrentDirectory, "XrmUtils.TestPlugin.dll");
           
            // Loads the specified assembly and retrieve the assembly name information.
            Assembly sourceAssembly = Assembly.LoadFile(path);            
            AssemblyName sourceName = sourceAssembly.GetName();
            
            // Instantiates object which describes the assembly to be registered.
            PluginAssembly regInfo = new PluginAssembly();

            regInfo.Name = sourceName.Name;
            regInfo.Culture = sourceName.CultureName;
            regInfo.Version = sourceName.Version.ToString();
            regInfo.PublicKeyToken = RetrievePublicToken(sourceAssembly);
            regInfo.IsolationMode = IsolationMode.Sandbox;
            regInfo.Content = Convert.ToBase64String(File.ReadAllBytes(path));

            // Instantiate the plugin service which will perform the assembly registration.
            PluginService service = new PluginService(orgSvc);

            // register the plugin assembly.
            assemblyId = service.RegisterAssembly(regInfo);

        }

        public void RegisterPluginType(IOrganizationService orgSvc)
        {

            // This example shows how to register a plugin type.
            // An assembly can have or more plugin types.
            // If working outside CRM, reflection may be used to determine available types.
            // In this example assume the type is XrmUtils.TestPlugin.MyPlugin

            Guid typeId;

            // Service which will perform the registration.
            PluginService service = new PluginService(orgSvc);

            // Retrieves the assembly registration (optional if the assembly Id is known at this time)
            PluginAssembly assembly = service.GetAssembly("XrmUtils.TestPlugin");

            // Instantiate the object that describes the plugin type
            // passing an assembly or assembly ID
            PluginType pluginType = new PluginType(assembly);

            pluginType.TypeName = "XrmUtils.TestPlugin.MyPlugin";
            pluginType.FriendlyName = "XrmUtils.TestPlugin.MyPlugin"; 

            // Register the plugin type.
            typeId = service.RegisterPluginType(pluginType);

        }

        public void RegisterPluginStep(IOrganizationService orgSvc)
        {

            // This example shows how to register a plugin step.

            Guid stepId;

            // service which will perform the registration.
            PluginService service = new PluginService(orgSvc);

            // Retrieves the plugin type using the type and assembly name.
            // (optional if at the plugin type Id is known at this time)
            PluginType pluginType = service.GetPluginType("XrmUtils.TestPlugin.MyPlugin", "XrmUtils.TestPlugin");
            
            // Instantiate the object describing the plugin step
            // passing a PluginType or plugin type ID.
            PluginStep step = new PluginStep(pluginType);

            // Identify SDK message and entity
            step.SdkMessage = "Update";
            step.PrimaryEntity = "contact";

            // These next properties are the ones most likely to be used during a step registration.
            // The values passed bellow are the same default values that will be applied if the properies were omitted.
            step.PipelineStage = PipelineStage.PostOperation;
            step.ExecutionMode = ExecutionMode.Asynchronous;
            step.AsyncAutoDelete = false;
            step.SupportedDeployment = SupportedDeployment.ServerOnly;

            // Additional (optional) parameters also available:
            
            // step.FilteringAttributes;
            // step.Name
            // step.ImpersonatingUserId;
            // step.ExecutionOrder;
            // step.Description;
            // step.UnsecureConfig;

            // register step.
            stepId = service.RegisterStep(step);

        }

        public void RegisterStepImage(IOrganizationService orgSvc)
        {

            // This example shows how to register a step image.

            // Service which will perform the registration.
            PluginService service = new PluginService(orgSvc);

            // The ID of the plugin step.
            Guid pluginStepId = new Guid("075ae778-7eda-e611-8108-c4346bad41fc");

            // Instantiate the object describing the plugin image.
            StepImage image = new StepImage(pluginStepId);

            image.ImageType = ImageType.PreImage;
            image.Name = "PreImage";
            image.EntityAlias = "PreImage";
            image.Attributes = "firstname,lastname";

            // Register the image.
            service.RegisterImage(image);

        }

        private string RetrievePublicToken(Assembly assembly)
        {

            byte[] token = assembly.GetName().GetPublicKeyToken();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < token.GetLength(0); i++)
            {
                sb.Append(String.Format("{0:x2}", token[i]));
            }

            return sb.ToString();

        }

    }
}
