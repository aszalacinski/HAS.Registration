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

namespace HAS.Registration
{
    public class SetRateModel : PageModel
    {
        private readonly IMediator _mediator;

        public SetRateModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [BindProperty]
        public UpdateDefaultClassRateCommand Data { get; set; }

        [TempData]
        public string SubscriptionDetails { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var updateClassDetails = new UpdateDefaultClassRateCommand();

            if (TempData.Peek<UserRegistration>("UserRegistration") == null)
            {
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error Has Occurred", "An error has occurred during user onboarding. Please contact support."));
                return RedirectToPage("~/Error");
            }

            var userRegDetails = TempData.Get<UserRegistration>("UserRegistration");

            // somehow, a non-instructor got to this page
            if(userRegDetails.InstructorId != userRegDetails.ProfileId)
            {
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error Has Occurred", "An error has occurred during user onboarding. Please contact support."));
                return RedirectToPage("~/Error");
            }

            updateClassDetails.Rate = 0;
            updateClassDetails.InstructorId = userRegDetails.ProfileId;
            updateClassDetails.ProfileId = userRegDetails.ProfileId;
            updateClassDetails.UserId = userRegDetails.UserId;
            updateClassDetails.Email = userRegDetails.Email;

            Data = updateClassDetails;
            return Page();
        }

        public class UpdateDefaultClassRateCommand : IRequest<Tribe>
        {
            [Display(Name="Class Rate")]
            public decimal Rate { get; set; }

            public string UserId { get; set; }

            public string ProfileId { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public string InstructorId { get; set; }
        }

        public async Task<IActionResult> OnPost()
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var tribe = await _mediator.Send(Data);

                    var userReg = UserRegistration.Create(Data.UserId, Data.ProfileId, Data.FirstName, Data.LastName, Data.Email, Data.InstructorId, tribe.Id);

                    TempData.Set("UserRegistration", userReg);

                    return RedirectToPage("../RegistrationResult", new { code = HttpStatusCode.NoContent });
                }
                // TODO: Add rollback logic
                catch(AddStudentTribeException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Add Tribe Error", ex.Message));
                }
                catch(UpdateTribeToSubscriptionException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Update Tribe to Subscription Error", ex.Message));
                }
                catch (AddStudentToTribeException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Add Student to Tribe Error", ex.Message));
                }
                catch (UpdateTribeClassRateException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Add Rate Error", ex.Message));
                }
                catch(AddSubscriptionToAccountException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"Add Subcription to Account Error", ex.Message));
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error has occurred", ex.Message));
                }
            }

            return Page();

        }

        public class UpdateDefaultClassRateCommandHandler : IRequestHandler<UpdateDefaultClassRateCommand, Tribe>
        {
            private readonly IMediator _mediator;
            private readonly HttpClient _httpClientTribe;
            private readonly HttpClient _httpClientProfile;
            private readonly IConfiguration _configuration;

            public UpdateDefaultClassRateCommandHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClientTribe = httpClientFactory.CreateClient(HASClientFactories.TRIBE);
                _httpClientProfile = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }

            public async Task<Tribe> Handle(UpdateDefaultClassRateCommand cmd, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                _httpClientTribe.SetBearerToken(token);
                _httpClientProfile.SetBearerToken(token);

                // create a student tribe
                string addStudentTribeUrl = $"{cmd.InstructorId}/a/stu";

                var addTribePayload = new { name = $"Default-{cmd.InstructorId}", description = "Default tribe for an instructor account. This tribe cannot be deleted. This is the location where subscribed students go." };

                var addTribePayloadJSON = JsonSerializer.Serialize(addTribePayload, DefaultJsonSettings.Settings);

                var addTribeContent = new StringContent(addTribePayloadJSON, Encoding.UTF8, "application/json");

                var addTribeResponse = await _httpClientTribe.PostAsync(addStudentTribeUrl, addTribeContent);

                var addTribeResponseContent = await addTribeResponse.Content.ReadAsStringAsync();

                if(!addTribeResponse.IsSuccessStatusCode)
                {
                    throw new AddStudentTribeException($"Could not create a student tribe for instructor {cmd.InstructorId}. Please contact support.");
                }

                var tribe = JsonSerializer.Deserialize<Tribe>(addTribeResponseContent, DefaultJsonSettings.Settings);

                var tribeId = tribe.Id;

                // set to subscription
                string setTribeToSubUrl = $"{cmd.InstructorId}/{tribeId}/sub";

                var setTribeToSubResponse = await _httpClientTribe.PutAsync(setTribeToSubUrl, null);

                if(!setTribeToSubResponse.IsSuccessStatusCode)
                {
                    throw new UpdateTribeToSubscriptionException($"There was an issue setting the class to a subscription. Please contact support.");
                }

                // add instructor as student to tribe
                string addStudentToTribeUrl = $"{cmd.InstructorId}/{tribeId}/a/{cmd.InstructorId}";

                var addStudentToTribeResponse = await _httpClientTribe.PutAsync(addStudentToTribeUrl, null);

                if(!addStudentToTribeResponse.IsSuccessStatusCode)
                {
                    throw new AddStudentToTribeException($"Could not add {cmd.InstructorId} to tribe {tribeId}. Please contact support");
                }

                // add instructor as subscriber to themselves
                string addInstructorAsSubscriberUrl = $"{cmd.ProfileId}/sub/add/{cmd.InstructorId}";

                var addInstructorAsSubscriberResponse = await _httpClientProfile.PutAsync(addInstructorAsSubscriberUrl, null);

                if(!addInstructorAsSubscriberResponse.IsSuccessStatusCode)
                {
                    throw new AddSubscriptionToAccountException($"Could not add {cmd.InstructorId} class to {cmd.ProfileId} as a subscription. Please contact support.");
                }

                // update the rate

                int rate = Convert.ToInt32(cmd.Rate * 100);

                string updateSubRateUrl = $"{cmd.InstructorId}/u/{tribeId}/subrate/{rate}";

                var updateTribeRateResponse = await _httpClientTribe.PutAsync(updateSubRateUrl, null);

                var rateResponse = await updateTribeRateResponse.Content.ReadAsStringAsync();

                if(!updateTribeRateResponse.IsSuccessStatusCode)
                {
                    throw new UpdateTribeClassRateException($"Could not update the class rate for tribe {tribeId} hosted by instructor {cmd.InstructorId}. Please contact support.");
                }

                return JsonSerializer.Deserialize<Tribe>(rateResponse, DefaultJsonSettings.Settings);
            }
        }

        public class AddStudentTribeException : Exception
        {
            public AddStudentTribeException() { }

            public AddStudentTribeException(string message)
                : base(message) { }

            public AddStudentTribeException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class UpdateTribeClassRateException : Exception
        {
            public UpdateTribeClassRateException() { }

            public UpdateTribeClassRateException(string message)
                : base(message) { }

            public UpdateTribeClassRateException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class UpdateTribeToSubscriptionException : Exception
        {
            public UpdateTribeToSubscriptionException() { }

            public UpdateTribeToSubscriptionException(string message)
                : base(message) { }

            public UpdateTribeToSubscriptionException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class AddStudentToTribeException : Exception
        {
            public AddStudentToTribeException() { }

            public AddStudentToTribeException(string message)
                : base(message) { }

            public AddStudentToTribeException(string message, Exception inner)
                : base(message, inner) { }
        }

        public class AddSubscriptionToAccountException : Exception
        {
            public AddSubscriptionToAccountException() { }

            public AddSubscriptionToAccountException(string message)
                : base(message) { }

            public AddSubscriptionToAccountException(string message, Exception inner)
                : base(message, inner) { }
        }
    }
}