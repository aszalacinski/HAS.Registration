using HAS.Registration.Feature.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Feature.Email.SendConfirmationEmail;
using static HAS.Registration.Feature.Identity.AddUserIdentity;
using static HAS.Registration.Feature.Message.AddRegistrationCompletedEventToQueue;

namespace HAS.Registration.Feature.UseCase
{
    public class OnboardUser
    {
        public class OnboardUserCommand : IRequest<string>
        {
            public string Username { get; private set; }
            public string Email { get; private set; }
            public string Password { get; private set; }

            public OnboardUserCommand(string username, string email, string password)
            {
                Username = username;
                Email = email;
                Password = password;
            }
        }

        public class OnboardUserCommandHander : IRequestHandler<OnboardUserCommand, string>
        {
            private IMediator _mediator;

            public OnboardUserCommandHander(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<string> Handle(OnboardUserCommand cmd, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await _mediator.Send(new AddUserIdentityCommand(cmd.Username, cmd.Email, cmd.Password));

                    if (user != null)
                    {
                        var emailComplete = await _mediator.Send(new SendConfirmationEmailCommand(user));
                        var queueComplete = await _mediator.Send(new AddRegistrationCompletedEventToQueueCommand(user.Email.Value, user.Id));

                        return user.Id;
                    }

                    return string.Empty;
                }
                catch (IdentityUserCreateException ex)
                {
                    throw ex;
                }

            }
        }
    }
}
