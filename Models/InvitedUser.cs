using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class InvitedUser
    {
        public string Id { get; private set; }
        public string EmailAddress { get; private set; }
        public string EntryCode { get; private set; }
        public bool Registered { get; private set; }
        public bool Invited { get; private set; }
        public DateTime DateRegistered { get; private set; }
        public List<InvitedUserLogEntry> Logs { get; private set; }

        private InvitedUser() { }

        private InvitedUser(string id, string emailAddress, string entryCode, bool isRegistered, bool isInvited, DateTime dateRegistered, IEnumerable<InvitedUserLogEntry> logs)
        {
            Id = id;
            EmailAddress = emailAddress;
            EntryCode = entryCode;
            Registered = isRegistered;
            Invited = isInvited;
            DateRegistered = dateRegistered;
            Logs = logs.ToList() ?? new List<InvitedUserLogEntry>();
        }

        public bool Register()
        {
            Registered = true;
            DateRegistered = DateTime.UtcNow;

            return Registered == true;
        }

        public bool Verify(string submittedEntryCode) => EntryCode.ToUpper() == submittedEntryCode.ToUpper();

        public void Log(bool registrationAttempt, int resultCode, string entryCode = null) => Logs.Add(InvitedUserLogEntry.Create(entryCode ?? EntryCode, EmailAddress, DateTime.UtcNow, registrationAttempt, resultCode));

        public static InvitedUser Create(string id, string emailAddress, string entryCode, bool isRegistered, bool isInvited, DateTime dateRegistered, IEnumerable<InvitedUserLogEntry> logs)
            => new InvitedUser(id, emailAddress, entryCode, isRegistered, isInvited, dateRegistered, logs);
    }

    public class InvitedUserLogEntry
    {
        public string EntryCode { get; private set; }
        public string EmailAddress { get; private set; }
        public DateTime AttemptDate { get; private set; }
        public bool Success { get; private set; }
        public int ResultCode { get; private set; }

        private InvitedUserLogEntry() { }

        private InvitedUserLogEntry(string entryCode, string emailAddress, DateTime attemptDate, bool success, int resultCode)
        {
            EntryCode = entryCode;
            EmailAddress = emailAddress;
            AttemptDate = attemptDate;
            Success = success;
            ResultCode = resultCode;
        }

        public static InvitedUserLogEntry Create(string entryCode, string emailAddress, DateTime attemptDate, bool success, int resultCode)
            => new InvitedUserLogEntry(entryCode, emailAddress, attemptDate, success, resultCode);
    }
}
