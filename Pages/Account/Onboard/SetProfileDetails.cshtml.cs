using HAS.Registration.Models;
using IdentityModel.Client;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;
using static HAS.Registration.GetAppProfileByUserId;

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
        
        public async Task<IActionResult> OnGet(bool isDev = false)
        {
            var updateUserDetails = new UpdateUserProfileDetailsCommand();

            if (TempData.Peek<UserRegistration>("UserRegistration") == null)
            {
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error Has Occurred", "An error has occurred during user onboarding. Please contact support."));
                return RedirectToPage("~/Error");
            }

            var userRegDetails = TempData.Get<UserRegistration>("UserRegistration");
                       
            // if instructoriId is not empty
            if (!string.IsNullOrEmpty(userRegDetails.InstructorId) || !userRegDetails.InstructorId.Equals(userRegDetails.ProfileId))
            {
                Student = new StudentProfile(); // TODO: Populate with Call to GetStudentProfile

                updateUserDetails.IsInstructor = false;
                updateUserDetails.InstructorId = userRegDetails.InstructorId;
                updateUserDetails.ProfileId = string.Empty; // TODO: Populate with Call to GetStudentProfile
                updateUserDetails.UserId = userRegDetails.UserId;
                updateUserDetails.Email = userRegDetails.Email;
                updateUserDetails.FirstName = userRegDetails.FirstName;
                updateUserDetails.LastName = userRegDetails.LastName;
                updateUserDetails.ScreenName = $"{userRegDetails.FirstName[0]}{userRegDetails.LastName}";
            }
            else
            {
                Instructor = await GetInstructorDetails(userRegDetails.UserId);
                
                updateUserDetails.IsInstructor = true;

                if (Instructor != null)
                {
                    updateUserDetails.InstructorId = Instructor.ProfileId;
                    updateUserDetails.ProfileId = Instructor.ProfileId;
                    updateUserDetails.UserId = Instructor.UserId;
                    updateUserDetails.Email = Instructor.Email;
                    updateUserDetails.FirstName = userRegDetails.FirstName;
                    updateUserDetails.LastName = userRegDetails.LastName;
                    updateUserDetails.ScreenName = $"{userRegDetails.FirstName[0]}{userRegDetails.LastName}";
                    updateUserDetails.PublicName = $"{userRegDetails.FirstName[0]}{userRegDetails.LastName}";
                }
            }

            Data = updateUserDetails;
            return Page();
        }

        public class UpdateUserProfileDetailsCommand : IRequest<Profile>
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

            public string ProfileId { get; set; }

            public string Email { get; set; }

            public string InstructorId { get; set; }
            public bool IsInstructor { get; set; }
        }

        public async Task<IActionResult> OnPost()
        {
            if(ModelState.IsValid)
            {
                // update user profile

                try
                {
                    Profile updatedProfile = await _mediator.Send(Data);

                    var userReg = UserRegistration.Create(updatedProfile.PersonalDetails.UserId, updatedProfile.Id, updatedProfile.PersonalDetails.FirstName, updatedProfile.PersonalDetails.LastName, updatedProfile.PersonalDetails.Email, Data.InstructorId);

                    TempData.Set("UserRegistration", userReg);

                    if (!updatedProfile.Id.Equals(Data.InstructorId))
                    {
                        // if student

                        return RedirectToPage("ChooseSubscription");
                    }
                    else
                    {
                        // if instructor

                        return RedirectToPage("SetBaseRate");
                    }
                }
                catch(UserProfileUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Profile Update Error", ex.Message));
                }
                catch(PublicNameCollisionException ex)
                {
                    ModelState.AddModelError("PublicName", ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Public Name Conflict", ex.Message));
                }
                catch(UpdateToInstructorException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Upgrade to Instructor Error", ex.Message));
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"General Error", ex.Message));
                }
            }
            
            return Page();
        }

        public class UpdateUserProfileDetailsCommandHandler : IRequestHandler<UpdateUserProfileDetailsCommand, Profile>
        {
            private readonly IMediator _mediator;
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _configuration;

            public UpdateUserProfileDetailsCommandHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClient = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }

            public async Task<Profile> Handle(UpdateUserProfileDetailsCommand cmd, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                _httpClient.SetBearerToken(token);

                // update personal details
                string updatePersonalUri = $"{cmd.ProfileId}/upd";

                var personalPayload = new { cmd.FirstName, cmd.LastName, cmd.ScreenName };

                var personalJson = JsonSerializer.Serialize(personalPayload, DefaultJsonSettings.Settings);

                var pContent = new StringContent(personalJson, Encoding.UTF8, "application/json");

                var pResponse = await _httpClient.PutAsync(updatePersonalUri, pContent);

                var prContent = await pResponse.Content.ReadAsStringAsync();

                if (!pResponse.IsSuccessStatusCode)
                {
                    throw new UserProfileUpdateException($"There was an error in updating your user profile. Please contact support.");
                }

                // if instructor, update account to instructor
                if (cmd.InstructorId.Equals(cmd.ProfileId))
                {
                    // make sure publicname is original

                    string nameCheckUri = $"by/publicName/{cmd.PublicName}";

                    var iProfile = await _httpClient.GetAsync(nameCheckUri);

                    if(iProfile.StatusCode == HttpStatusCode.NotFound)
                    {
                        // update account to instructor
                        string updateToInstructorUri = $"{cmd.ProfileId}/as/in";

                        var payload = new { cmd.PublicName };

                        var json = JsonSerializer.Serialize(payload, DefaultJsonSettings.Settings);

                        var putContent = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PutAsync(updateToInstructorUri, putContent);

                        var content = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new UpdateToInstructorException($"Could not update {cmd.ScreenName} to an Instructor; please contact support.");
                        }

                        return JsonSerializer.Deserialize<Profile>(content, DefaultJsonSettings.Settings);
                    }
                    else
                    {
                        throw new PublicNameCollisionException($"The Public Name {cmd.PublicName} is already in use by another instructor. Please choose a different Public Name.");
                    }
                }
                else
                {
                    // update some student stuff related to instructor
                }


                return JsonSerializer.Deserialize<Profile>(prContent, DefaultJsonSettings.Settings); ;
            }
        }


        private async Task<InstructorProfile> GetInstructorDetails(string userId)
        {
            var profile = await _mediator.Send(new GetAppProfileByUserIdQuery(userId));

            var instructor = InstructorProfile.Create(profile.PersonalDetails.UserId, profile.Id, profile.PersonalDetails.FirstName, profile.PersonalDetails.LastName, profile.PersonalDetails.Email, profile.PersonalDetails.ScreenName, profile.AppDetails.InstructorDetails.PublicName);

            return instructor;

        }

        public class UserProfileUpdateException : Exception
        {
            public UserProfileUpdateException() { }

            public UserProfileUpdateException(string message)
                : base(message) { }

            public UserProfileUpdateException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class PublicNameCollisionException : Exception
        {
            public PublicNameCollisionException() { }

            public PublicNameCollisionException(string message)
                : base(message) { }

            public PublicNameCollisionException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class UpdateToInstructorException : Exception
        {
            public UpdateToInstructorException() { }

            public UpdateToInstructorException(string message)
                : base(message) { }

            public UpdateToInstructorException(string message, Exception inner)
                : base(message, inner) { }
        }
    }
}