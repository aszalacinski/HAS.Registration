using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class InvitedUserDAO
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("email")]
        public string EmailAddress { get; set; }

        [BsonElement("code")]
        public string EntryCode { get; set; }

        [BsonElement("isReg")]
        public bool Registered { get; set; }

        [BsonElement("invit")]
        public bool Invited { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("dcreated")]
        public DateTime DateRegistered { get; set; }

        [BsonElement("logs")]
        public List<InvitedUserLogEntryDAO> Logs { get; set; }
    }

    public static class InvitedUserDAOExensions
    {
        public static InvitedUser ToEntity(this InvitedUserDAO dao) => InvitedUser.Create(dao.Id.ToString(), dao.EmailAddress, dao.EntryCode, dao.Registered, dao.Invited, dao.DateRegistered);

        public static InvitedUserDAO ToDAO(this InvitedUserSnapshot snapshot) => new InvitedUserDAO { Id = snapshot.Id.Equals(string.Empty) ? ObjectId.Empty : ObjectId.Parse(snapshot.Id), EmailAddress = snapshot.EmailAddress, EntryCode = snapshot.EntryCode, Registered = snapshot.Registered, Invited = snapshot.Invited, DateRegistered = snapshot.DateRegistered };

        public static InvitedUserDAO ToDAO(this InvitedUser invitedUser) => invitedUser.AsSnapshot().ToDAO();

        public static IEnumerable<InvitedUser> ToEntity(this IEnumerable<InvitedUserDAO> dao) => dao.Select(x => x.ToEntity()).AsEnumerable();

        public static IEnumerable<InvitedUserDAO> ToDAO(this IEnumerable<InvitedUserSnapshot> snapshot) => snapshot.Select(x => x.ToDAO()).AsEnumerable();

        public static IEnumerable<InvitedUserDAO> ToDAO(this IEnumerable<InvitedUser> invitedUser) => invitedUser.Select(x => x.AsSnapshot().ToDAO()).AsEnumerable();
    }
}
