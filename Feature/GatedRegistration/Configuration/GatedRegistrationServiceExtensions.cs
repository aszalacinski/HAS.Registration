using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public static class GatedRegistrationServiceExtensions
    {
        public static IServiceCollection AddGatedRegstration(this IServiceCollection service)
        {
            service.AddSingleton<IGatedRegistrationRepository, GatedRegistrationRespository>();

            service.AddSingleton<IGatedRegistrationService, GatedRegistrationService>();

            return service;
        }
    }
}
