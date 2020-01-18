using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class InstructorProfile
    {
        public string InstructorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ScreenName { get; set; }
        public string PublicName { get; set; }
    }
}
