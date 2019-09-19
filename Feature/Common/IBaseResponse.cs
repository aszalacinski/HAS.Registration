using System;
using System.Collections.Generic;

namespace HAS.Registration.Feature
{
    public interface IBaseResponse<T>
    {
        string FormattedErrorString { get; }
        bool HasErrors { get; }
        List<string> ListAllErrors { get; }
        string Message { get; }
        T Result { get; }
        Exception Exception { get; }
    }
}
