﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class RegistrationCompletedEvent
    {
        public string Email { get; set; }
        public string UserId { get; set; }
    }
}
