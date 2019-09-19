using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature
{
    public abstract class BaseResponse<T> : VoidResponse, IBaseResponse<T>
    {
        public virtual T Result { get; private set; }

        public BaseResponse(T result, string successMessage)
            : base(successMessage)
        {
            Result = result;
        }

        public BaseResponse(bool hasErrors, string errorMessage)
            : base(hasErrors, errorMessage)
        {

        }

        public BaseResponse(bool hasErrors, string errorMessage, T result)
            : base(hasErrors, errorMessage)
        {
            Result = result;
        }

        public BaseResponse(bool hasErrors, string errorMessage, Exception exception)
            : base(hasErrors, errorMessage, exception)
        {

        }

        public BaseResponse(bool hasErrors, string errorMessage, T result, Exception exception)
            : base(hasErrors, errorMessage, exception)
        {
            Result = result;
        }

        public override bool HasErrors { get { return base.HasErrors; } }

        public virtual List<string> ListAllErrors
        {
            get
            {
                return base.Errors;
            }
            private set { }
        }

        public override string Message
        {
            get
            {
                return base.Message;
            }
        }

        public override string FormattedErrorString
        {
            get
            {
                return base.FormattedErrorString;
            }
        }

        public override Exception Exception
        {
            get
            {
                return base.Exception;
            }
        }
    }
}
