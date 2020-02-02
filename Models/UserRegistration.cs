using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class UserRegistration
    {
        // Identity User Id - On the Profile object this is found under PersonalDetails.UserId
        public string UserId { get; set; }

        // Profile Id - On the Profile Object as Id
        public string ProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string InstructorId { get; set; }

        public UserRegistration()
        {

        }

        private UserRegistration(string userId, string profileId, string firstName, string lastName, string email, string instructorId)
        {
            UserId = userId;
            ProfileId = profileId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            InstructorId = instructorId;
        }

        public static UserRegistration Create(string userId, string profileId, string firstName, string lastName, string email, string instructorId)
            => new UserRegistration(userId, profileId, firstName, lastName, email, instructorId);
    }
}
