using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static HAS.Registration.Feature.Email.SendResetPasswordEmail;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMediator _mediator;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IMediator mediator)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        [BindProperty]
        public ForgotPasswordCommand Model { get; set; }

        public void OnGet()
        {
            Model = new ForgotPasswordCommand();
        }

        public class ForgotPasswordCommand : IRequest<string>
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("ForgotPasswordConfirmation");
                }

                // Send an email with this link
                var email = await _mediator.Send(new SendResetPasswordEmailCommand(user));

                return RedirectToPage("ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}