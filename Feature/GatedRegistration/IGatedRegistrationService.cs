using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public interface IGatedRegistrationService
    {
        Task<GatedRegistrationServiceResponse<ResultResponse<bool>>> AttemptToRegister(string emailAddress, string entryCode);
    }
}
