using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("ConfirmError");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                //return NotFound($"Unable to load user with ID '{userId}'.");
                return RedirectToPage("ConfirmError");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                //throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
                return RedirectToPage("ConfirmError");
            }

            return Page();
        }
    }
}