using AutoMapper;
using HAS.Registration.Data;
using HAS.Registration.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Data.GatedRegistrationContext;
using static HAS.Registration.Feature.GatedRegistration.AddUser;
using static HAS.Registration.Feature.GatedRegistration.GetUserByEmailAddress;
using static HAS.Registration.Feature.GatedRegistration.UpdateUser;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class RegisterUser
    {
        public class RegisterUserCommand : IRequest<GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public string EmailAddress { get; private set; }
            public string EntryCode { get; private set; }

            public RegisterUserCommand(string emailAddress, string entryCode)
            {
                EmailAddress = emailAddress;
                EntryCode = entryCode;
            }
        }

        public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;
            private readonly IMediator _mediator;

            public RegisterUserCommandHandler(GatedRegistrationContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserDAOProfile>();
                });
            }

            public async Task<GatedRegistrationServiceResponse<RegistrationResult>> Handle(RegisterUserCommand cmd, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                InvitedUser invitedUser = await _mediator.Send(new GetUserByEmailAddressQuery(cmd.EmailAddress));

                if(invitedUser != null)
                {
                    if(invitedUser.Invited)
                    {
                        if(invitedUser.Verify(cmd.EntryCode))
                        {
                            if(!invitedUser.Registered)
                            {
                                invitedUser.Register();
                                invitedUser.Log(true, 204, cmd.EntryCode);
                                return await _mediator.Send(new UpdateUserCommand(invitedUser, HttpStatusCode.NoContent, "User successfully registered"));
                            }
                            else
                            {
                                // user is in database and was invited and has already registered
                                // log entry attempt, return false
                                invitedUser.Log(false, 208, cmd.EntryCode);
                                return await _mediator.Send(new UpdateUserCommand(invitedUser, HttpStatusCode.AlreadyReported, "User has already registered"));
                            }
                        }
                        else
                        {
                            //user attempted to log in with invalid entry code
                            invitedUser.Log(false, 401, cmd.EntryCode);
                            return await _mediator.Send(new UpdateUserCommand(invitedUser, HttpStatusCode.Unauthorized, "User attempted to register with invalid entry code"));
                        }
                    }
                    else
                    {
                        // user is in database but is uninvited, capture email, log entry attempt, return false
                        invitedUser.Log(false, 302, cmd.EntryCode);
                        return await _mediator.Send(new UpdateUserCommand(invitedUser, HttpStatusCode.Found, "User is in database but is uninvited"));
                    }
                }
                else
                {
                    // add user to database as uninvited, capture email, log entry attempt return false
                    InvitedUser newUser = InvitedUser.Create(string.Empty, cmd.EmailAddress, "0F0F0F", false, false, DateTime.MinValue, new List<InvitedUserLogEntry>());
                    newUser.Log(false, 200, cmd.EntryCode);

                    return await _mediator.Send(new AddUserCommand(newUser, HttpStatusCode.OK, "User was added to database as uninvited"));
                }

            }
        }

        public class RegistrationResult
        {
            public HttpStatusCode StatusCode { get; private set; }
            public string Message { get; private set; }

            private RegistrationResult(HttpStatusCode code, string message)
            {
                StatusCode = code;
                Message = message;
            }

            public static RegistrationResult Create(HttpStatusCode code, string message) => new RegistrationResult(code, message);
        }
    }
}
