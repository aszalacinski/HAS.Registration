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
        public int ResultCode { get; }

        public InvitedUserLogEntry(string entryCode, string emailAddress, DateTime attemptDate, bool success, int resultCode)
        {
            EntryCode = entryCode;
            EmailAddress = emailAddress;
            AttemptDate = attemptDate;
            Success = success;
            ResultCode = resultCode;
        }

        public InvitedUserLogEntrySnapshot AsSnapshot()
        {
            return new InvitedUserLogEntrySnapshot
            {
                AttemptDate = AttemptDate,
                EmailAddress = EmailAddress,
                EntryCode = EntryCode,
                Success = Success,
                ResultCode = ResultCode
            };
        }

        public static InvitedUserLogEntry Create(string entryCode, string emailAddress, DateTime attemptDate, bool success, int resultCode) => new InvitedUserLogEntry(entryCode, emailAddress, attemptDate, success, resultCode);

        protected override IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
        {
            return new List<object> { EntryCode, EmailAddress, AttemptDate, Success, ResultCode };
        }
    }

    public class InvitedUserLogEntrySnapshot
    {
        public string EntryCode { get; set; }
        public string EmailAddress { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool Success { get; set; }
        public int ResultCode { get; set; }
    }

    public static class InvitedUserLogEntryExtensions
    {
        public static InvitedUserLogEntry ToValueObject(this InvitedUserLogEntrySnapshot snapshot) => new InvitedUserLogEntry(snapshot.EntryCode, snapshot.EmailAddress, snapshot.AttemptDate, snapshot.Success, snapshot.ResultCode);

        public static IEnumerable<InvitedUserLogEntry> ToValueObjects(this IEnumerable<InvitedUserLogEntrySnapshot> snapshots) => snapshots.Select(x => x.ToValueObject()).AsEnumerable();

        public static IEnumerable<InvitedUserLogEntrySnapshot> AsSnapshots(this IEnumerable<InvitedUserLogEntry> logs) => logs.Select(x => x.AsSnapshot()).AsEnumerable();
    }
}
