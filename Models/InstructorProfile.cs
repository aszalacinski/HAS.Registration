using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class InstructorProfile
    {
        public string UserId { get; private set; }
        public string ProfileId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string ScreenName { get; private set; }
        public string PublicName { get; private set; }

        private InstructorProfile(string userId, string profileId, string firstName, string lastName, string email, string screenName, string publicName)
        {
            UserId = userId;
            ProfileId = profileId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            ScreenName = screenName;
            PublicName = publicName;
        }

        public static InstructorProfile Create(string userId, string profileId, string firstName, string lastName, string email, string screenName, string publicName)
            => new InstructorProfile(userId, profileId, firstName, lastName, email, screenName, publicName);
    }
}
