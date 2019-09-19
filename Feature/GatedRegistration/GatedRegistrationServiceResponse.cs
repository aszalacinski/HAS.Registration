using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class GatedRegistrationServiceResponse<T> : BaseResponse<T>
    {
        public GatedRegistrationServiceResponse(T result, string successMessage)
            : base(result, successMessage)
        {

        }

        public GatedRegistrationServiceResponse(bool hasErrors, string errorMessage)
            : base(hasErrors, errorMessage)
        {

        }

        public GatedRegistrationServiceResponse(bool hasErrors, string errorMessage, T result)
            : base(hasErrors, errorMessage, result)
        {

        }

        public GatedRegistrationServiceResponse(bool hasErrors, string errorMessage, Exception exception)
            : base(hasErrors, errorMessage, exception)
        {

        }

        public GatedRegistrationServiceResponse(bool hasErrors, string errorMessage, T result, Exception exception)
            : base(hasErrors, errorMessage, result, exception)
        {

        }
    }
}
