using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class ValidateRegistrationResponse<T> : BaseResponse<T>
    {
        public ValidateRegistrationResponse(T result, string successMessage)
            : base(result, successMessage)
        {

        }

        public ValidateRegistrationResponse(bool hasErrors, string errorMessage)
            : base(hasErrors, errorMessage)
        {

        }

        public ValidateRegistrationResponse(bool hasErrors, string errorMessage, T result)
            : base(hasErrors, errorMessage, result)
        {

        }

        public ValidateRegistrationResponse(bool hasErrors, string errorMessage, Exception exception)
            : base(hasErrors, errorMessage, exception)
        {

        }

        public ValidateRegistrationResponse(bool hasErrors, string errorMessage, T result, Exception exception)
            : base(hasErrors, errorMessage, result, exception)
        {

        }
    }
}
