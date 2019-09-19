using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class InvitedUserLogEntryDAO
    {
        [BsonElement("code")]
        public string EntryCode { get; set; }

        [BsonElement("email")]
        public string EmailAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("adate")]
        public DateTime AttemptDate { get; set; }

        [BsonElement("success")]
        public bool Success { get; set; }
    }

    public static class InvitedUserLogEntryDAOExtensions
    {
        public static InvitedUserLogEntry ToValueObject(this InvitedUserLogEntryDAO dao)
        {
            return InvitedUserLogEntry.Create(dao.EntryCode, dao.EmailAddress, dao.AttemptDate, dao.Success);
        }

        public static InvitedUserLogEntryDAO ToDAO(this InvitedUserLogEntrySnapshot snapshot)
        {
            return new InvitedUserLogEntryDAO
            {
                EntryCode = snapshot.EntryCode,
                EmailAddress = snapshot.EmailAddress,
                AttemptDate = snapshot.AttemptDate,
                Success = snapshot.Success
            };
        }

        public static InvitedUserLogEntryDAO ToDAO(this InvitedUserLogEntry log) => log.AsSnapshot().ToDAO();

        public static IEnumerable<InvitedUserLogEntry> ToValueObject(this IEnumerable<InvitedUserLogEntryDAO> dao) => dao.Select(x => x.ToValueObject()).AsEnumerable();

        public static IEnumerable<InvitedUserLogEntryDAO> ToDAO(this IEnumerable<InvitedUserLogEntrySnapshot> snapshot) => snapshot.Select(x => x.ToDAO()).AsEnumerable();

        public static IEnumerable<InvitedUserLogEntryDAO> ToDAO(this IEnumerable<InvitedUserLogEntry> log) => log.Select(x => x.AsSnapshot().ToDAO()).AsEnumerable();
    }
}
