﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Models
{
    public class GatedRegistrationResultViewModel
    {
        private readonly int _resultCode;

        public string Message { get; set; }

        public GatedRegistrationResultViewModel(int code)
        {
            switch (code)
            {
                case 204:
                    Message = "Thanks for registering. Please login at <a href=\"https://www.mypractice.yoga\">MyPractice.Yoga</a>.";
                    break;

                case 200:
                case 302:
                    Message = "Thanks for attempting to register with MyPractice.Yoga. " +
                        "<br /><br />We are currently in what is called a Closed Beta of our application and registration is currently open to invited students only. " +
                        "<br /><br />We have captured your email address and will keep you informed of future updates of when MyPractice.Yoga goes Open Beta. " +
                        "<br /><br />You can also speak with your yoga instructor who is providing content through MyPractice.Yoga for more details. " +
                        "<br /><br />Thanks again for your interest in MyPractice.Yoga! More to come!";
                    break;

                case 401:
                    Message = "Thanks for attempting to validate your registgration with MyPractice.Yoga " +
                        "<br /><br />The entry code you entered was not valid. Please retry entering the entry code again exactly as it was presented to you. " +
                        "<br /><br />If you continue to have issues with validating your registration, please contact the yoga instructor who is providing content through MyPractice.Yoga for more details. " +
                        "<br /><br />Thanks again for your interest in MyPractice.Yoga!";
                    break;

                case 400:
                default:
                    Message = "An error occurred during update of Invited User record. Please try again later";
                    break;
            }

            _resultCode = code;
        }
    }
}