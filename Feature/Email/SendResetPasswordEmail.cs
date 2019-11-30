using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration.Feature.Email
{
    public class SendResetPasswordEmail
    {
        public class SendResetPasswordEmailCommand : IRequest<bool>
        {
            public IdentityUser User { get; private set; }

            public SendResetPasswordEmailCommand(IdentityUser user) => User = user;
        }

        public class SendResetPasswordEmailCommandHandler : IRequestHandler<SendResetPasswordEmailCommand, bool>
        {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly IEmailSender _emailSender;
            private readonly IUrlHelper _urlHelper;
            private readonly IHttpContextAccessor _httpContextAcessor;

            public SendResetPasswordEmailCommandHandler(UserManager<IdentityUser> userManager, IEmailSender emailSender, IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionAccessor)
            {
                _userManager = userManager;
                _emailSender = emailSender;
                _urlHelper = urlHelperFactory.GetUrlHelper(actionAccessor.ActionContext);
                _httpContextAcessor = httpContextAccessor;
            }

            public async Task<bool> Handle(SendResetPasswordEmailCommand cmd, CancellationToken cancellationToken)
            {
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(cmd.User);
                
                var callbackUrl = _urlHelper.Page("/Account/ResetPassword", null, new { code }, _httpContextAcessor.HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(cmd.User.Email.Value, "Reset Password for MyPractice.Yoga",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return true;
            }
        }
    }
}
