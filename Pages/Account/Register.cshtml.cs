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
using static HAS.Registration.Feature.GatedRegistration.GetUserInGatedRegistrationByEmailAddress;
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

            // if publicName is NOT empty, then this is a student registration with a specific instructor
            if (!string.IsNullOrEmpty(publicName))
            {
                if(publicName.Trim().ToUpper() == "INSTRUCTOR")
                {
                    // code should be passed from the invitation email link
                    if (code != null)
                    {
                        newUserCommand.EntryCode = code;
                    }
                }
                else
                {
                    // check if publicName maps to instructor profile
                    Instructor = await GetInstructorDetails(publicName);

                    // if yes
                    if (Instructor != null)
                    {
                        // code should be passed from the invitation email link
                        if (code != null)
                        {
                            newUserCommand.EntryCode = code;
                        }
                        newUserCommand.InstructorId = Instructor.ProfileId;
                    }
                    else
                    {
                        // instructor doesn't exist so user is going to be registered as an invited instructor or uninvited user
                        // do nothing
                    }
                }
            } 
            else
            {
                // this is could be an invited instructor registering
                // or it's an uninvited user who found the registration page and is attempting to register
                // do nothing
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
                //TODO: for a student registration, pass instructors name to validate registration
                var registerCheck = await _mediator.Send(new ValidateRegistrationCommand(Data.Email, Data.EntryCode));
                
                // get invited user details from database
                var regDetails = await _mediator.Send(new GetUserInGatedRegistrationByEmailAddressQuery(Data.Email));

                try
                {
                    switch (registerCheck.Result.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            var userId = await _mediator.Send(new OnboardUserCommand(Data.Email, Data.Email, Data.Password));
                            if (!string.IsNullOrEmpty(userId))
                            {
                                
                                // if instructor id is NOT null, then it's a student... take them through the student onboarding flow
                                if (Data.InstructorId != null)
                                {
                                    var userReg = UserRegistration.Create(userId, string.Empty, regDetails.FirstName, regDetails.LastName, Data.Email, Data.InstructorId, string.Empty);

                                    TempData.Set("UserRegistration", userReg);

                                    return RedirectToPage("./Onboard/ChooseSubscription");
                                }
                                else
                                {
                                    // this is an instructor

                                    // take them through the instructor onboarding flow
                                    var userReg = UserRegistration.Create(userId, string.Empty, regDetails.FirstName, regDetails.LastName,  Data.Email, string.Empty, string.Empty);

                                    TempData.Set("UserRegistration", userReg);

                                    return RedirectToPage("./Onboard/SetProfileDetails");
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

            var instructor = InstructorProfile.Create(profile.PersonalDetails.UserId, profile.Id, profile.PersonalDetails.FirstName, profile.PersonalDetails.LastName, profile.PersonalDetails.Email, profile.PersonalDetails.ScreenName, profile.AppDetails.InstructorDetails.PublicName);
            
            return instructor;

        }
    }
}