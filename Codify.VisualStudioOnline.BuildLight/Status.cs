using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    public enum Status
    {
        Unknown,
        InProgress,
        PartiallySucceeded,
        Succeeded,
        Failed,
        Cancelled,
        RetrievalError
    }
}
