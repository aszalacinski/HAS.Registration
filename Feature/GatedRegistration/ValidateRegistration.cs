using AutoMapper;
using HAS.Registration.Data;
using HAS.Registration.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Feature.GatedRegistration.AddUserToGatedRegistration;
using static HAS.Registration.Feature.GatedRegistration.GetUserInGatedRegistrationByEmailAddress;
using static HAS.Registration.Feature.GatedRegistration.UpdateUserInGatedRegistration;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class ValidateRegistration
    {
        public class ValidateRegistrationCommand : IRequest<ValidateRegistrationResponse<ValidateRegistrationResult>>
        {
            public string EmailAddress { get; private set; }
            public string EntryCode { get; private set; }

            public ValidateRegistrationCommand(string emailAddress, string entryCode)
            {
                EmailAddress = emailAddress;
                EntryCode = entryCode;
            }
        }

        public class ValidateRegistrationCommandHandler : IRequestHandler<ValidateRegistrationCommand, ValidateRegistrationResponse<ValidateRegistrationResult>>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;
            private readonly IMediator _mediator;

            public ValidateRegistrationCommandHandler(GatedRegistrationContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserDAOProfile>();
                });
            }

            public async Task<ValidateRegistrationResponse<ValidateRegistrationResult>> Handle(ValidateRegistrationCommand cmd, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                InvitedUser invitedUser = await _mediator.Send(new GetUserInGatedRegistrationByEmailAddressQuery(cmd.EmailAddress));

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
                                return await _mediator.Send(new UpdateUserInGatedRegistrationCommand(invitedUser, HttpStatusCode.NoContent, "User successfully registered"));
                            }
                            else
                            {
                                // user is in database and was invited and has already registered
                                // log entry attempt, return false
                                invitedUser.Log(false, 208, cmd.EntryCode);
                                return await _mediator.Send(new UpdateUserInGatedRegistrationCommand(invitedUser, HttpStatusCode.AlreadyReported, "User has already registered"));
                            }
                        }
                        else
                        {
                            //user attempted to log in with invalid entry code
                            invitedUser.Log(false, 401, cmd.EntryCode);
                            return await _mediator.Send(new UpdateUserInGatedRegistrationCommand(invitedUser, HttpStatusCode.Unauthorized, "User attempted to register with invalid entry code"));
                        }
                    }
                    else
                    {
                        // user is in database but is uninvited, capture email, log entry attempt, return false
                        invitedUser.Log(false, 302, cmd.EntryCode);
                        return await _mediator.Send(new UpdateUserInGatedRegistrationCommand(invitedUser, HttpStatusCode.Found, "User is in database but is uninvited"));
                    }
                }
                else
                {
                    // add user to database as uninvited, capture email, log entry attempt return false
                    InvitedUser newUser = InvitedUser.Create(string.Empty, string.Empty, string.Empty, cmd.EmailAddress, string.Empty, string.Empty, "0F0F0F", false, false, DateTime.MinValue, new List<InvitedUserLogEntry>());
                    newUser.Log(false, 200, cmd.EntryCode);

                    return await _mediator.Send(new AddUserToGatedRegistrationCommand(newUser, HttpStatusCode.OK, "User was added to database as uninvited"));
                }

            }
        }

        public class ValidateRegistrationResult
        {
            public HttpStatusCode StatusCode { get; private set; }
            public string Message { get; private set; }

            private ValidateRegistrationResult(HttpStatusCode code, string message)
            {
                StatusCode = code;
                Message = message;
            }

            public static ValidateRegistrationResult Create(HttpStatusCode code, string message) => new ValidateRegistrationResult(code, message);
        }
    }
}
