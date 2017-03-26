Dynamics CRM SDK Extensions
===========================

[![Build status](https://ci.appveyor.com/api/projects/status/ue0akrlpfw8m5y9a?svg=true)](https://ci.appveyor.com/project/emerbrito/xrmutils-extensions)

This project contains a set of extension methods for different components of the Microsoft Dynamics CRM SDK. Also a set of wrappers and utilities created around the Organization Service Context, metadata and more. 

The goal of these extensions is to minimize the clutter created from boilerplate code while at the same time abstracting some of the complexity involved in performing such tasks.


Get it from Nuget:
```
    PM> Install-Package Install-Package XrmUtils.CrmSdk.Extensions 
```

Documentation
----------------

Documentation will be provide soon. Meanwhile look into the `SampleCode` project for usage details.


Extensions
----------------

Extention methods to the most commonly used SDK objects.
These are a few samples:

#### Entity Extensions
A set of extensions the `Microsoft.Xrm.Sdk.Entity` class.

```csharp
// add new attribute or update existing attribute.
entity.AddOrUpdateAttribute("fullname", "Joh Doe");

// attempts to get an attribute value from the entity,
// falls back to pre-image if attribute doesn't exist or isnull
entity.GetAttributeValue<string>("country", preImage);

// falls back to "USA" if "country" attribute doesn't exist on entity or pre-image
entity.GetAttributeValue<string>("country", preImage, "USA");

// determine if an attribute has changes by comparing it to pre-image
entity.AttributeHasChanged("creditlimit", preImage);
``` 

#### OrganizationService Extensions
A set of extensions the `Microsoft.Xrm.Sdk.IOrganizationService`.

```csharp
// executes a workflow on all child recrods based on relationship name
string rel = "contact_customers_account";
service.ExecuteProcessOnChildRecords(processId, account, rel, continueOnError: false);

// get all related entities based on relationship name
service.GetRelatedEntities(account, "contact_customers_account");
``` 

Services
--------
Services are wrappers to functionally available through the SDK.

#### Plugin Service
Allows for easy manipulation of plugin registration.
This example shows how to register a plugin step.

```csharp
    Guid stepId;
    PluginService service = new PluginService(orgSvc);

    PluginType pluginType = service.GetPluginType("XrmUtils.TestPlugin.MyPlugin", "XrmUtils.TestPlugin");
    PluginStep step = new PluginStep(pluginType);

    step.SdkMessage = "Update";
    step.PrimaryEntity = "contact";

    step.PipelineStage = PipelineStage.PostOperation;
    step.ExecutionMode = ExecutionMode.Asynchronous;

    // register step.
    stepId = service.RegisterStep(step);
```

#### Membership Services
Wraps functionality around system users and teams.

```csharp
    MembershipService service = new MembershipService(orgSvc);

    // whether user is system administrator
    service.IsSystemAdministrator(userId);

    // check if user has security role
    service.UserHasRole(userId, "Project Approver");
```

#### Membership Services
Retrieves entity and attribute metadata.

```csharp
MetadataService service = new MetadataService(orgSvc);

// retrieves attribute metadta
service.GetAttributeMetadata("account", "accountnumber");

// retrieves relationship metadata for the account entity
service.GetEntityMetadata("account", EntityFilters.Relationships);
```


License
--------
While this project os dostributed under MIT license, it may have dependencies on 3rd party software distributed under different licenses.