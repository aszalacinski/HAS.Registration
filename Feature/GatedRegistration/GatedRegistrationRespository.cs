using HAS.Registration.ApplicationServices.MongoDb;
using HAS.Registration.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class GatedRegistrationRespository : IGatedRegistrationRepository
    {
        private readonly CloudSettings _cloudSettings;

        private DbContext _dbContext;
        private IMongoCollection<InvitedUserDAO> _invitedUser;

        public GatedRegistrationRespository(CloudSettings cloudSettings)
        {
            _cloudSettings = cloudSettings;
            _dbContext = DbContext.Create(cloudSettings.DBConnectionString_MongoDB_DatabaseName, cloudSettings.DBConnectionString_MongoDB);
            _invitedUser = _dbContext.Database.GetCollection<InvitedUserDAO>("gated-registration");
        }

        public async Task<InvitedUser> Add(InvitedUser invitedUser)
        {
            var dao = invitedUser.ToDAO();
            await _invitedUser.InsertOneAsync(dao);
            return dao.ToEntity();
        }

        public async Task<InvitedUser> Find(Expression<Func<InvitedUserDAO, bool>> expression)
        {
            var dao = await Task.Run(() => _invitedUser.AsQueryable().Where(expression).FirstOrDefault());
            if(dao != null)
            {
                return dao.ToEntity();
            }

            return null;
        }

        public async Task<IEnumerable<InvitedUser>> FindAll(Expression<Func<InvitedUserDAO, bool>> expression)
        {
            var dao = await Task.Run(() => _invitedUser.AsQueryable().Where(expression).AsEnumerable());
            if (dao != null)
            {
                return dao.ToEntity();
            }

            return null;
        }

        public async Task<InvitedUser> Update(InvitedUser invitedUser)
        {
            var dao = invitedUser.ToDAO();
            var filter = Builders<InvitedUserDAO>.Filter.Eq(x => x.Id, dao.Id);
            var options = new FindOneAndReplaceOptions<InvitedUserDAO> { ReturnDocument = ReturnDocument.After };
            var result = await _invitedUser.FindOneAndReplaceAsync(filter, dao, options);

            return result.ToEntity();

        }
    }
}
