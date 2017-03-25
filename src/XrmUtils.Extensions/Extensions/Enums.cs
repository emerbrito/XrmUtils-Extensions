using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Extensions
{

    public enum ImageType
    {
        PreImage = 0,
        PostImage = 1,
        Both = 2
    }


    public enum ExecutionMode
    {
        Asynchronous = 1,
        Synchronous = 0
    }

    public enum PipelineStage
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
    }

}
