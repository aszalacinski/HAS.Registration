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
using static HAS.Registration.Feature.GatedRegistration.RegisterUser;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class AddUser
    {
        public class AddUserCommand : IRequest<GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public InvitedUser User { get; private set; }
            public HttpStatusCode Code { get; private set; }
            public string Message { get; private set; }

            public AddUserCommand(InvitedUser user, HttpStatusCode code, string message)
            {
                User = user;
                Code = code;
                Message = message;
            }
        }

        public class AddUserCommandHandler : IRequestHandler<AddUserCommand, GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public AddUserCommandHandler(GatedRegistrationContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserDAOProfile>();
                });
            }

            public async Task<GatedRegistrationServiceResponse<RegistrationResult>> Handle(AddUserCommand cmd, CancellationToken cancellationToken)
            {

                var mapper = new Mapper(_mapperConfiguration);

                var dao = mapper.Map<InvitedUserDAO>(cmd.User);

                try
                {
                    await _db.Users.InsertOneAsync(dao);
                    return new GatedRegistrationServiceResponse<RegistrationResult>(RegistrationResult.Create(cmd.Code, cmd.Message), cmd.Message);
                }
                catch (Exception)
                {
                    return new GatedRegistrationServiceResponse<RegistrationResult>(true, "An error occurred adding a user", RegistrationResult.Create(HttpStatusCode.BadRequest, "An error occurred adding a user"));
                }

            }
        }
    }
}
