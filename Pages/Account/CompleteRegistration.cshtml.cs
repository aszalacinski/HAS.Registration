using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HAS.Registration.Feature.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.GatedRegistration.ValidateRegistration;
using static HAS.Registration.Feature.UseCase.OnboardUser;

namespace HAS.Registration.Pages.Account
{
    public class CompleteRegistrationModel : PageModel
    {
        private readonly IMediator _mediator;

        public CompleteRegistrationModel(IMediator mediator)
        {
            _mediator = mediator;
        }


        public void OnGet()
        {
            // summarize and confirm on submit
        }

        public void OnPost()
        {
            
        }

    }
}