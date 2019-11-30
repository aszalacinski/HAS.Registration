using AutoMapper;
using HAS.Registration.Data;
using HAS.Registration.Models;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Data.GatedRegistrationContext;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class GetUserInGatedRegistrationByEmailAddress
    {
        public class GetUserInGatedRegistrationByEmailAddressQuery : IRequest<InvitedUser>
        {
            public string EmailAddress { get; private set; }

            public GetUserInGatedRegistrationByEmailAddressQuery(string emailAddress) => EmailAddress = emailAddress;
        }

        public class GetUserInGatedRegistrationByEmailAddressQueryHandler : IRequestHandler<GetUserInGatedRegistrationByEmailAddressQuery, InvitedUser>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public GetUserInGatedRegistrationByEmailAddressQueryHandler(GatedRegistrationContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserProfile>();
                });
            }

            public async Task<InvitedUser> Handle(GetUserInGatedRegistrationByEmailAddressQuery query, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                var projection = Builders<InvitedUserDAO>.Projection.Expression(x => mapper.Map<InvitedUser>(x));

                var invitedUser = await _db.Users
                                            .Find(x => x.EmailAddress.ToUpper() == query.EmailAddress.ToUpper())
                                            .Project(projection)
                                            .FirstOrDefaultAsync();

                return invitedUser;
            }
        }
    }
}
