using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Services.Plugins
{

    public enum IsolationMode
    {
        None = 1,
        Sandbox = 2
    }

    public enum SdkMessageProcessingStepState
    {
        Enabled = 0,
        Disabled = 1,
    }

    public enum SourceType
    {
        Database = 0,
        Disk = 1,
        Normal = 2
    }

    public enum SupportedDeployment
    {
        ServerOnly = 0,
        OfflineOnly = 1,
        Both = 2
    }

}
