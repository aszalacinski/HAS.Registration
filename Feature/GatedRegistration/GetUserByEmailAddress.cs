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
    public class GetUserByEmailAddress
    {
        public class GetUserByEmailAddressQuery : IRequest<InvitedUser>
        {
            public string EmailAddress { get; private set; }

            public GetUserByEmailAddressQuery(string emailAddress) => EmailAddress = emailAddress;
        }

        public class GetUserByEmailAddressQueryHandler : IRequestHandler<GetUserByEmailAddressQuery, InvitedUser>
        {
            public readonly GatedRegistrationContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public GetUserByEmailAddressQueryHandler(GatedRegistrationContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<InvitedUserProfile>();
                });
            }

            public async Task<InvitedUser> Handle(GetUserByEmailAddressQuery query, CancellationToken cancellationToken)
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
