using AutoMapper;
using HAS.Registration.Data;
using HAS.Registration.Models;
using MediatR;
using MongoDB.Driver;
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
    public class UpdateUser
    {
        public class UpdateUserCommand : IRequest<GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public InvitedUser User { get; private set; }
            public HttpStatusCode Code { get; private set; }
            public string Message { get; private set; }

            public UpdateUserCommand(InvitedUser user, HttpStatusCode code, string message)
            {
                User = user;
                Code = code;
                Message = message;
            }
        }

        public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, GatedRegistrationServiceResponse<RegistrationResult>>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public UpdateUserCommandHandler(GatedRegistrationContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserDAOProfile>();
                });
            }

            public async Task<GatedRegistrationServiceResponse<RegistrationResult>> Handle(UpdateUserCommand cmd, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                var dao = mapper.Map<InvitedUserDAO>(cmd.User);

                try
                {
                    var filter = Builders<InvitedUserDAO>.Filter.Eq(x => x.Id, dao.Id);
                    var update = await _db.Users.FindOneAndReplaceAsync(filter, dao);

                    return new GatedRegistrationServiceResponse<RegistrationResult>(RegistrationResult.Create(cmd.Code, cmd.Message), cmd.Message);
                }
                catch(Exception)
                {
                    return new GatedRegistrationServiceResponse<RegistrationResult>(true, "An error occurred updating a user", RegistrationResult.Create(HttpStatusCode.BadRequest, "An error occurred updating a user"));
                }
            }
        }
    }
}
