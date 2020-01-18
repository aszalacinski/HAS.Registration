using System;
using System.Collections.Generic;

namespace HAS.Registration.Models
{
    public class Profile
    {
        public string Id { get; set; }
        public DateTime LastUpdate { get; set; }
        public PersonalDetails PersonalDetails { get; set; }
        public AppDetails AppDetails { get; set; }

        public bool IsInstructor() => AppDetails.AccountType.ToUpper() == "INSTRUCTOR";
    }

    public class PersonalDetails
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string ScreenName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public LocationDetails Location { get; set; }
    }

    public class AppDetails
    {
        public string AccountType { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime JoinDate { get; set; }
        public IEnumerable<SubscriptionDetails> Subscriptions { get; set; }
        public InstructorDetails InstructorDetails { get; set; }
    }

    public class LocationDetails
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
    }

    public class SubscriptionDetails
    {
        public string InstructorId { get; set; }
        public IEnumerable<ClassDetails> Classes { get; set; }
    }

    public class InstructorDetails
    {
        public DateTime? StartDate { get; set; }
        public string PublicName { get; set; }
    }

    public class ClassDetails
    {
        public string ClassId { get; set; }
        public bool Liked { get; set; }
    }
}
