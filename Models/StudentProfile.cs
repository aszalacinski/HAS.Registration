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
    }
}
