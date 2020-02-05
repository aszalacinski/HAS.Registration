using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class Tribe
    {
        public string Id { get; set; }
        public string InstructorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public bool IsSubscription { get; set; }
        public TribeSubscriptionDetails SubscriptionDetails { get; set; }
        public IEnumerable<Member> Members { get; set; }
    }

    public class Member
    {
        public string Id { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class TribeSubscriptionDetails
    {
        public IEnumerable<SubscriptionRate> Rates { get; set; }
    }

    public class SubscriptionRate
    {
        public int Rate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
