using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class StudentProfile
    {
        public string UserId { get; private set; }
        public string ProfileId { get; private set; }
        public string InstructorId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string ScreenName { get; private set; }

        private StudentProfile(string userId, string profileId, string instructorId, string firstName, string lastName, string email, string screenName)
        {
            UserId = userId;
            ProfileId = profileId;
            InstructorId = instructorId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            ScreenName = screenName;
        }

        public static StudentProfile Create(string userId, string profileId, string instructorId, string firstName, string lastName, string email, string screenName)
            => new StudentProfile(userId, profileId, instructorId, firstName, lastName, email, screenName);
    }
}
