using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HAS.Registration.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.GetAppProfileById;

namespace HAS.Registration
{
    public class ProfileDetailsModel : PageModel
    {
        private readonly IMediator _mediator;

        public ProfileDetailsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty]
        public UpdateUserProfileDetailsCommand Data { get; set; }

        [TempData]
        public string SubscriptionDetails { get; set; }

        public InstructorProfile Instructor { get; set; }

        public StudentProfile Student { get; set; }
        
        public async Task<IActionResult> OnGet()
        {
            //var updateUserDetails = new UpdateUserProfileDetailsCommand();

            //var subDetails = TempData.Get<SubscriptionRegistration>("SubscriptionDetails");

            //if(subDetails == null)
            //{
            //    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error Has Occurred", "An error has occurred during user onboarding. Please contact support."));
            //    return RedirectToPage("~/Error");
            //}


            //// if user id != instructor id then this is a student registration
            //if(!subDetails.UserId.Equals(subDetails.InstructorId))
            //{
            //    Student = new StudentProfile(); // TODO: Populate with Call to GetStudentProfile

            //    updateUserDetails.IsInstructor = false;
            //    updateUserDetails.InstructorId = subDetails.InstructorId;
            //    updateUserDetails.UserId = subDetails.UserId;
            //    updateUserDetails.Email = subDetails.Email;
            //    updateUserDetails.FirstName = string.Empty; // TODO: Populate with Call to GetStudentProfile
            //    updateUserDetails.LastName = string.Empty; // TODO: Populate with Call to GetStudentProfile
            //    updateUserDetails.ScreenName = string.Empty; // TODO: Populate with Call to GetStudentProfile
            //}
            //else
            //{
            //    var instructorDetails = await GetInstructorDetails(subDetails.InstructorId);

            //    Instructor = instructorDetails;

            //    updateUserDetails.IsInstructor = true;

            //    if(instructorDetails != null)
            //    {
            //        updateUserDetails.InstructorId = instructorDetails.InstructorId;
            //        updateUserDetails.UserId = instructorDetails.InstructorId;
            //        updateUserDetails.Email = instructorDetails.Email;
            //        updateUserDetails.FirstName = instructorDetails.FirstName;
            //        updateUserDetails.LastName = instructorDetails.LastName;
            //        updateUserDetails.ScreenName = $"{instructorDetails.FirstName[0]}{instructorDetails.LastName}";
            //        updateUserDetails.PublicName = $"{instructorDetails.FirstName[0]}{instructorDetails.LastName}";
            //    }
            //}

            //Data = updateUserDetails;
            return Page();
        }

        public class UpdateUserProfileDetailsCommand : IRequest<string>
        {
            [Display(Name="First Name")]
            public string FirstName { get; set; }

            [Display(Name="Last Name")]
            public string LastName { get; set; }

            [Display(Name="Screen Name")]
            public string ScreenName { get; set; }

            [Display(Name="Public Name")]
            public string PublicName { get; set; }

            public string UserId { get; set; }

            public string Email { get; set; }

            public string InstructorId { get; set; }
            public bool IsInstructor { get; set; }
        }

        public async Task<IActionResult> OnPost()
        {

            return RedirectToPage("SetBaseRate");

            if (!Data.UserId.Equals(Data.InstructorId))
            {
                return RedirectToPage("ChooseSubscription");
            }
            else
            {
                return RedirectToPage("SetBaseRate");
            }
        }

        private async Task<InstructorProfile> GetInstructorDetails(string instructorId)
        {
            var profile = await _mediator.Send(new GetAppProfileByIdQuery(instructorId));

            var instructor = new InstructorProfile
            {
                Email = profile.PersonalDetails.Email,
                FirstName = profile.PersonalDetails.FirstName,
                InstructorId = profile.Id,
                LastName = profile.PersonalDetails.LastName,
                PublicName = profile.AppDetails.InstructorDetails.PublicName,
                ScreenName = profile.PersonalDetails.ScreenName
            };

            return instructor;

        }
    }
}