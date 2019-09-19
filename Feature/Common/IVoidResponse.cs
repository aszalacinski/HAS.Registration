using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature
{
    public interface IVoidResponse
    {
        List<string> Errors { get; }
        string FormattedErrorString { get; }
        bool HasErrors { get; }
        string Message { get; }

        void AddErrorMessage(string message);

        Exception Exception { get; }
    }
}
