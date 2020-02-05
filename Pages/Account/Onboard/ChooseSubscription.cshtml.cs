using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAS.Registration.Models;
using IdentityModel.Client;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;
using static HAS.Registration.GetAppProfileById;
using static HAS.Registration.SetRateModel;

namespace HAS.Registration.Pages.Account.Onboard
{
    public class ChooseSubscriptionModel : PageModel
    {
        private readonly IMediator _mediator;

        public ChooseSubscriptionModel(IMediator mediator)
        {
            _mediator = mediator;

        }
        
        public IEnumerable<Subscription> SubscriptionOptions { get; set; }
        

        [BindProperty]
        public SubscribeCommand Data { get; set; }

        public Profile Instructor { get; set; }

        public class SubscribeCommand : IRequest<Profile>
        {
            public string ProfileId { get; set; }
            public string InstructorId { get; set; }
            public string SubscriptionId { get; set; }
        }

        public async Task<IActionResult> OnGet()
        {

            if(TempData.Peek<UserRegistration>("UserRegistration") == null)
            {
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error Has Occurred", "An error has occurred during user onboarding. Please contact support."));
                return RedirectToPage("~/Error");
            }

            var subscriptionReg = TempData.Get<UserRegistration>("UserRegistration");

            if(subscriptionReg == null)
            {
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, "Registration issue", "There was an issue with setting up your subscription."));
                return RedirectToPage("/Account/Register");
            }

            Instructor = await _mediator.Send(new GetAppProfileByIdQuery(subscriptionReg.InstructorId));

            // check if provided id is an instructor
            if (Instructor.IsInstructor())
            {
                // get subscriptions
                SubscriptionOptions = await _mediator.Send(new GetInstructorSubscriptionsQuery(subscriptionReg.InstructorId));
                
                Data = new SubscribeCommand { 
                    InstructorId = Instructor.Id, 
                    ProfileId = subscriptionReg.ProfileId
                };
                return Page();
            }
            else
            {
                // redirect to default registration page
                await _mediator.Send(new ThrowAlertCommand(AlertType.WARNING, "Instructor Not Found", "There is no instructor that matches the instructor Id provided."));
                return RedirectToPage("/Account/Register");
            }
        }

        public async Task<IActionResult> OnPost()
        {   
            try
            {
                var profile = await _mediator.Send(Data);

                return RedirectToPage("../RegistrationResult", new { code = HttpStatusCode.NoContent });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await _mediator.Send(new ThrowAlertCommand(AlertType.DANGER, $"An Error has occurred", ex.Message));
            }

            return Page();
        }

        public class GetInstructorSubscriptionsQuery : IRequest<List<Subscription>>
        {
            public string InstructorId { get; private set; }

            public GetInstructorSubscriptionsQuery(string instructorId) => InstructorId = instructorId;
        }

        public class GetInstructorSubscriptionsQueryHandler : IRequestHandler<GetInstructorSubscriptionsQuery, List<Subscription>>
        {
            private IMediator _mediator;
            private readonly HttpClient _httpClientTribe;
            private readonly HttpClient _httpClientProfile;
            private readonly IConfiguration _configuration;

            public GetInstructorSubscriptionsQueryHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClientTribe = httpClientFactory.CreateClient(HASClientFactories.TRIBE);
                _httpClientProfile = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }


            public async Task<List<Subscription>> Handle(GetInstructorSubscriptionsQuery query, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                _httpClientTribe.SetBearerToken(token);

                string getTribesByInstructorUrl = $"{query.InstructorId}/a";

                var getTribeByInstructorIdResponse = await _httpClientTribe.GetAsync(getTribesByInstructorUrl);

                if(!getTribeByInstructorIdResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Something failed in retrieving the instructors tribes.");
                }

                var getTribesByInstructorIdContent = await getTribeByInstructorIdResponse.Content.ReadAsStringAsync();

                var tribes = JsonSerializer.Deserialize<List<Tribe>>(getTribesByInstructorIdContent, DefaultJsonSettings.Settings);

                if(tribes.Count() <= 0)
                {
                    throw new Exception($"No tribes for instructor");
                }
                
                // find the default tribe
                var tribe = tribes.Where(x => x.Name.Contains($"Default-{query.InstructorId}")).FirstOrDefault();

                if(tribe == null)
                {
                    throw new Exception($"Tribe not found");
                }

                var subscriptions = InitSubs(query.InstructorId);

                subscriptions[0].Id = tribe.Id;
                
                return subscriptions;
            }

            private List<Subscription> InitSubs(string instructorId)
            {
                return new List<Subscription>
            {
                new Subscription
                {
                    Amount = "FREE",
                    Cost = 0.00M,
                    Description = "Grants temporary access to the Closed Beta",
                    Enabled = true,
                    Id = "A0AA69D0-1F06-48CD-B73F-628BD6E3269D",
                    InstructorId = instructorId,
                    Term = "Monthly",
                    Title = "Closed Beta Access",
                    Features = new string[]
                    {
                        "Access to all functionality",
                        "Be the first to access paid tier"
                    }
                },
                new Subscription
                {
                    Amount = "Basic",
                    Cost = 50.00M,
                    Description = "Grants access to the Basic Tier",
                    Enabled = false,
                    Id = "F8EF05DE-8D8F-4885-A9ED-E2EAAD5BD0E1",
                    InstructorId = instructorId,
                    Term = "Monthly",
                    Title = "Basic",
                    Features = new string[]
                    {
                        "Access audio files of classes"
                    }
                },
                new Subscription
                {
                    Amount = "Premium",
                    Cost = 100.00M,
                    Description = "Grants access to the Premium Tier",
                    Enabled = false,
                    Id = "AC766351-7F52-40CD-B5A6-4E1FD4655395",
                    InstructorId = instructorId,
                    Term = "Monthly",
                    Title = "Premium",
                    Features = new string[]
                    {
                        "Access video files of classes"
                    }
                }
            };
            }
        }

        public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, Profile>
        {
            private readonly IMediator _mediator;
            private readonly HttpClient _httpClientTribe;
            private readonly HttpClient _httpClientProfile;
            private readonly IConfiguration _configuration;

            public SubscribeCommandHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClientTribe = httpClientFactory.CreateClient(HASClientFactories.TRIBE);
                _httpClientProfile = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }

            public async Task<Profile> Handle(SubscribeCommand cmd, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                _httpClientTribe.SetBearerToken(token);
                _httpClientProfile.SetBearerToken(token);

                // add student to instructor tribe
                string addStudentToTribeUrl = $"{cmd.InstructorId}/{cmd.SubscriptionId}/a/{cmd.ProfileId}";

                var addStudentToTribeResponse = await _httpClientTribe.PutAsync(addStudentToTribeUrl, null);

                if (!addStudentToTribeResponse.IsSuccessStatusCode)
                {
                    throw new AddStudentToTribeException($"Could not add {cmd.ProfileId} to tribe {cmd.SubscriptionId}. Please contact support");
                }

                // add subscription to instructor to student profile
                string addSubscriptionToStudentUrl = $"{cmd.ProfileId}/sub/add/{cmd.InstructorId}";

                var addSubscriptionToStudentResponse = await _httpClientProfile.PutAsync(addSubscriptionToStudentUrl, null);

                var addSubContent = await addSubscriptionToStudentResponse.Content.ReadAsStringAsync();

                if (!addSubscriptionToStudentResponse.IsSuccessStatusCode)
                {
                    throw new AddSubscriptionToAccountException($"Could not add {cmd.InstructorId} class to {cmd.ProfileId} as a subscription. Please contact support.");
                }

                return JsonSerializer.Deserialize<Profile>(addSubContent, DefaultJsonSettings.Settings);
            }
        }

        public class Subscription
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Term { get; set; }
            public string Amount { get; set; }
            public decimal Cost { get; set; }
            public string Id { get; set; }
            public bool Enabled { get; set; }
            public string InstructorId { get; set; }
            public string[] Features { get; set; }
        }

    }
}