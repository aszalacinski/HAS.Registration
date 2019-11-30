using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HAS.Registration.Pages.Account
{
    public class RegistrationResultModel : PageModel
    {
        [BindProperty]
        public string Message { get; set; }

        public void OnGet(HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.NoContent:
                    Message = "<h2>MyPractice.Yoga Registration Successful</h2>" +
                        "<p> You now need to confirm your registration. Please check your email and click the link in the confirmation email.</ p > ";
                    break;

                case HttpStatusCode.AlreadyReported:
                    Message = "You have already registered. Please login at <a href=\"https://www.mypractice.yoga\">MyPractice.Yoga</a>.";
                    break;

                case HttpStatusCode.OK:
                case HttpStatusCode.Found:
                    Message = "Thanks for attempting to register with MyPractice.Yoga. " +
                        "<br /><br />We are currently in what is called a Closed Beta of our application and registration is currently open to invited students only. " +
                        "<br /><br />We have captured your email address and will keep you informed of future updates of when MyPractice.Yoga goes Open Beta. " +
                        "<br /><br />You can also speak with your yoga instructor who is providing content through MyPractice.Yoga for more details. " +
                        "<br /><br />Thanks again for your interest in MyPractice.Yoga! More to come!";
                    break;

                case HttpStatusCode.Unauthorized:
                    Message = "Thanks for attempting to validate your registgration with MyPractice.Yoga " +
                        "<br /><br />The entry code you entered was not valid. Please retry entering the entry code again exactly as it was presented to you. " +
                        "<br /><br />If you continue to have issues with validating your registration, please contact the yoga instructor who is providing content through MyPractice.Yoga for more details. " +
                        "<br /><br />Thanks again for your interest in MyPractice.Yoga!";
                    break;

                case HttpStatusCode.BadRequest:
                default:
                    Message = "An error occurred during update of Invited User record. Please try again later";
                    break;
            }
        }
    }
}