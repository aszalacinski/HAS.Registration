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
using static HAS.Registration.Feature.GatedRegistration.ValidateRegistration;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class AddUserToGatedRegistration
    {
        public class AddUserToGatedRegistrationCommand : IRequest<ValidateRegistrationResponse<ValidateRegistrationResult>>
        {
            public InvitedUser User { get; private set; }
            public HttpStatusCode Code { get; private set; }
            public string Message { get; private set; }

            public AddUserToGatedRegistrationCommand(InvitedUser user, HttpStatusCode code, string message)
            {
                User = user;
                Code = code;
                Message = message;
            }
        }

        public class AddUserToGatedRegistrationCommandHandler : IRequestHandler<AddUserToGatedRegistrationCommand, ValidateRegistrationResponse<ValidateRegistrationResult>>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public AddUserToGatedRegistrationCommandHandler(GatedRegistrationContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserDAOProfile>();
                });
            }

            public async Task<ValidateRegistrationResponse<ValidateRegistrationResult>> Handle(AddUserToGatedRegistrationCommand cmd, CancellationToken cancellationToken)
            {

                var mapper = new Mapper(_mapperConfiguration);

                var dao = mapper.Map<InvitedUserDAO>(cmd.User);

                try
                {
                    await _db.Users.InsertOneAsync(dao);
                    return new ValidateRegistrationResponse<ValidateRegistrationResult>(ValidateRegistrationResult.Create(cmd.Code, cmd.Message), cmd.Message);
                }
                catch (Exception)
                {
                    return new ValidateRegistrationResponse<ValidateRegistrationResult>(true, "An error occurred adding a user", ValidateRegistrationResult.Create(HttpStatusCode.BadRequest, "An error occurred adding a user"));
                }

            }
        }
    }
}
