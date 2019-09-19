using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class InvitedUserLogEntry : ValueObject<InvitedUserLogEntry>
    {
        public string EntryCode { get; }
        public string EmailAddress { get; }
        public DateTime AttemptDate { get; }
        public bool Success { get; }

        public InvitedUserLogEntry(string entryCode, string emailAddress, DateTime attemptDate, bool success)
        {
            EntryCode = entryCode;
            EmailAddress = emailAddress;
            AttemptDate = attemptDate;
            Success = success;
        }

        public InvitedUserLogEntrySnapshot AsSnapshot()
        {
            return new InvitedUserLogEntrySnapshot
            {
                AttemptDate = AttemptDate,
                EmailAddress = EmailAddress,
                EntryCode = EntryCode,
                Success = Success
            };
        }

        public static InvitedUserLogEntry Create(string entryCode, string emailAddress, DateTime attemptDate, bool success) => new InvitedUserLogEntry(entryCode, emailAddress, attemptDate, success);

        protected override IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
        {
            return new List<object> { EntryCode, EmailAddress, AttemptDate, Success };
        }
    }

    public class InvitedUserLogEntrySnapshot
    {
        public string EntryCode { get; set; }
        public string EmailAddress { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool Success { get; set; }
    }

    public static class InvitedUserLogEntryExtensions
    {
        public static InvitedUserLogEntry ToValueObject(this InvitedUserLogEntrySnapshot snapshot) => new InvitedUserLogEntry(snapshot.EntryCode, snapshot.EmailAddress, snapshot.AttemptDate, snapshot.Success);

        public static IEnumerable<InvitedUserLogEntry> ToValueObjects(this IEnumerable<InvitedUserLogEntrySnapshot> snapshots) => snapshots.Select(x => x.ToValueObject()).AsEnumerable();

        public static IEnumerable<InvitedUserLogEntrySnapshot> AsSnapshots(this IEnumerable<InvitedUserLogEntry> logs) => logs.Select(x => x.AsSnapshot()).AsEnumerable();
    }
}
