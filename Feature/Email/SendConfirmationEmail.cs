using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDb;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration.Feature.Email
{
    public class SendConfirmationEmail
    {
        public class SendConfirmationEmailCommand : IRequest<bool>
        {
            public IdentityUser User { get; private set; }

            public SendConfirmationEmailCommand(IdentityUser user) => User = user;
        }

        public class SendConfirmationEmailCommandHelper : IRequestHandler<SendConfirmationEmailCommand, bool>
        {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly IEmailSender _emailSender;
            private readonly IUrlHelper _urlHelper;
            private readonly IHttpContextAccessor _httpContextAcessor;

            public SendConfirmationEmailCommandHelper(UserManager<IdentityUser> userManager, IEmailSender emailSender, IUrlHelper urlHelper, IHttpContextAccessor httpContextAccessor)
            {
                _userManager = userManager;
                _emailSender = emailSender;
                _urlHelper = urlHelper;
                _httpContextAcessor = httpContextAccessor;
            }

            public async Task<bool> Handle(SendConfirmationEmailCommand cmd, CancellationToken cancellationToken)
            {
                // Send an email with this link
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(cmd.User);

                var callbackUrl = _urlHelper.Action("ConfirmEmail", "Account", new { userId = cmd.User.Id, code = code }, _httpContextAcessor.HttpContext.Request.Scheme);
                
                await _emailSender.SendEmailAsync(cmd.User.Email.Value, "MyPractice.Yoga Email Confirmation",
                    "Please confirm your MyPractice.Yoga account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                return true;
            }
        }
    }
}
