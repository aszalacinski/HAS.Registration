using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Configuration
{
    public class CloudSettings
    {
        public string DBConnectionString_MongoDB { get; set; }
        public string DBConnectionString_MongoDB_DatabaseName { get; set; }

        public string Azure_Queue_ConnectionString { get; set; }
        public string Azure_Queue_Name_ReservationCompletedEvent { get; set; }
    }
}
