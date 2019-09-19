using System;
using System.Collections.Generic;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class InvitedUser : Entity<string>
    {
        private string EmailAddress { get; set; }
        private string EntryCode { get; set; }
        private bool Registered { get; set; }
        private bool Invited { get; set; }
        private DateTime DateRegistered { get; set; }
        private List<InvitedUserLogEntry> RegistrationLog { get; set; }

        private InvitedUser() { }

        public InvitedUser(string id, string emailAddress, string entryCode, bool isRegistered, bool isInvited, DateTime dateRegistered)
            : base(id)
        {
            EmailAddress = emailAddress;
            EntryCode = entryCode;
            Registered = isRegistered;
            Invited = isInvited;
            DateRegistered = dateRegistered;
        }

        public bool IsRegistered() => Registered;

        public bool Register() => Registered = true;

        public bool IsInvited() => Invited;

        public DateTime RegisteredDate() => DateRegistered;

        public bool Verify(string submittedEntryCode) => EntryCode.ToUpper() == submittedEntryCode.ToUpper();

        public IEnumerable<InvitedUserLogEntry> Logs => RegistrationLog;

        public void Log(bool registrationAttempt, string entryCode = null) => RegistrationLog.Add(new InvitedUserLogEntry(entryCode ?? EntryCode, EmailAddress, DateTime.UtcNow, registrationAttempt));

        public InvitedUserSnapshot AsSnapshot()
        {
            return new InvitedUserSnapshot
            {
                DateRegistered = DateRegistered,
                EmailAddress = EmailAddress,
                EntryCode = EntryCode,
                Id = Id,
                Invited = Invited,
                Registered = Registered,
                Logs = RegistrationLog.AsSnapshots()
            };
        }

        public static InvitedUser Create(string id, string emailAddress, string entryCode, bool isRegistered, bool isInvited, DateTime dateRegistered)
        {
            return new InvitedUser(id, emailAddress, entryCode, isRegistered, isInvited, dateRegistered);
        }
    }

    public class InvitedUserSnapshot
    {
        public string Id { get; set; }
        public string EmailAddress { get; set; }
        public string EntryCode { get; set; }
        public bool Registered { get; set; }
        public bool Invited { get; set; }
        public DateTime DateRegistered { get; set; }
        public IEnumerable<InvitedUserLogEntrySnapshot> Logs { get; set; }
    }

    public static class InvitedUserExtensions
    {
        public static InvitedUser ToEntity(this InvitedUserSnapshot snapshot) => InvitedUser.Create(snapshot.Id, snapshot.EmailAddress, snapshot.EntryCode, snapshot.Registered, snapshot.Invited, snapshot.DateRegistered);
    }
}
