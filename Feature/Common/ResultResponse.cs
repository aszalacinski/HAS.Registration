using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature
{
    public class ResultResponse<T>
    {
        ResultResponse(T result, int statusCode)
        {
            Result = result;
            StatusCode = statusCode;
        }

        T Result { get; set; }
        int StatusCode { get; set; }

        public static ResultResponse<T> Create(T result, int statusCode) => new ResultResponse<T>(result, statusCode);
    }
}
