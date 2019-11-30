using FluentValidation;
using HAS.Registration.Feature.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.GatedRegistration.ValidateRegistration;
using static HAS.Registration.Feature.Identity.AddUserIdentity;
using static HAS.Registration.Feature.UseCase.OnboardUser;

namespace HAS.Registration.Pages.Account
{
    public class Register : PageModel
    {
        private readonly IMediator _mediator;

        public Register(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public RegisterNewUserCommand Data { get; set; }

        public void OnGet()
        {
            Data = new RegisterNewUserCommand();
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
                    switch(registerCheck.Result.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            var url = await _mediator.Send(new OnboardUserCommand(Data.Email, Data.Email, Data.Password));
                            if(url.Equals("RegistrationResult"))
                            {
                                return RedirectToPage("RegistrationResult", new { code = HttpStatusCode.NoContent });
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
                catch(IdentityUserCreateException ex)
                {
                    AddErrors(ex.Errors);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"", ex.Errors.ToString()));
                }
            }

            return Page();
        }

        private async Task<string> Onboard(string username, string email, string password)
        {
            return await _mediator.Send(new OnboardUserCommand(username, email, password));
        }

        private void AddErrors(IEnumerable<IdentityError> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}