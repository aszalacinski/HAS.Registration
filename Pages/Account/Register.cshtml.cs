using FluentValidation;
using HAS.Registration.Feature.Identity;
using HAS.Registration.Models;
using IdentityModel.Client;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.GatedRegistration.ValidateRegistration;
using static HAS.Registration.Feature.Identity.AddUserIdentity;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;
using static HAS.Registration.Feature.UseCase.OnboardUser;
using static HAS.Registration.GetAppProfileByPublicName;

namespace HAS.Registration.Pages.Account
{
    public class Register : PageModel
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpProfileClient;

        public Register(IMediator mediator, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _mediator = mediator;
            _configuration = configuration;
            _httpProfileClient = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
        }

        [BindProperty]
        public RegisterNewUserCommand Data { get; set; }

        [TempData]
        public string SubscriptionDetails { get; set; }

        public InstructorProfile Instructor { get; set; }
        
        public async Task OnGet(string publicName, string code = null)
        {
            var newUserCommand = new RegisterNewUserCommand();

            if(!string.IsNullOrEmpty(publicName))
            {
                // check if publicName maps to instructor profile
                var instructor = await GetInstructorDetails(publicName);

                // if yes
                if(instructor != null)
                {
                    if(code != null)
                    {
                        newUserCommand.EntryCode = code;
                    }
                    Instructor = instructor;
                    newUserCommand.InstructorId = instructor.InstructorId;
                }
            }

            Data = newUserCommand;
        }

        public class RegisterNewUserCommand : IRequest<string>
        {
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Entry Code")]
            public string EntryCode { get; set; }

            public string InstructorId { get; set; }
            public string ReturnUrl { get; set; }
        }

        public class Validator : AbstractValidator<RegisterNewUserCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Email).NotEmpty().EmailAddress();
                RuleFor(m => m.Password).NotEmpty().MinimumLength(6).MaximumLength(100).WithMessage("The password must be at least 6 characters long.");
                RuleFor(m => m.ConfirmPassword).NotEmpty().Equal(m => m.Password).WithMessage("The password and confirmation password do not match");
                RuleFor(m => m.EntryCode).NotEmpty();
            }
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                // check if user is registed with GatedRegistration
                var registerCheck = await _mediator.Send(new ValidateRegistrationCommand(Data.Email, Data.EntryCode));

                try
                {
                    switch (registerCheck.Result.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            var userId = await _mediator.Send(new OnboardUserCommand(Data.Email, Data.Email, Data.Password));
                            if (!string.IsNullOrEmpty(userId))
                            {
                                if(Data.InstructorId != null)
                                {
                                    var subReg = new SubscriptionRegistration
                                    {
                                        UserId = userId,
                                        Email = Data.Email,
                                        InstructorId = Data.InstructorId
                                    };

                                    TempData.Set("SubscriptionDetails", subReg);

                                    return RedirectToPage("ChooseSubscription");
                                }
                                else
                                {
                                    return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.NoContent });
                                }
                            }
                            else
                            {
                                return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.BadRequest });
                            }

                        case HttpStatusCode.OK:
                            return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.OK });

                        case HttpStatusCode.AlreadyReported:
                            return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.AlreadyReported });

                        case HttpStatusCode.Found:
                            return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.Found });

                        case HttpStatusCode.Unauthorized:
                            return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.Unauthorized });

                        default:
                            return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.BadRequest });

                    }

                }
                catch (IdentityUserCreateException ex)
                {
                    AddErrors(ex.Errors);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"", ex.Errors.ToString()));
                }
            }

            return Page();
        }

        private void AddErrors(IEnumerable<IdentityError> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<InstructorProfile> GetInstructorDetails(string publicName)
        {
            var profile = await _mediator.Send(new GetAppProfileByPublicNameQuery(publicName));

            var instructor = new InstructorProfile
            {
                Email = profile.PersonalDetails.Email,
                FirstName = profile.PersonalDetails.FirstName,
                InstructorId = profile.Id,
                LastName = profile.PersonalDetails.LastName,
                PublicName = profile.AppDetails.InstructorDetails.PublicName,
                ScreenName = profile.PersonalDetails.ScreenName
            };

            return instructor;

        }
    }
}