using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class InvitedUserDTO
    {
        public string Id { get; set; }
        public string EmailAddress { get; set; }
        public string EntryCode { get; set; }
        public bool Registered { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
