using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAS.Registration.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using static HAS.Registration.Feature.Alert.ThrowAlert;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;
using static HAS.Registration.GetAppProfileById;

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

        public class SubscribeCommand : IRequest<string>
        {
            public string StudentUserId { get; set; }
            public string InstructorProfileId { get; set; }
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
                
                Data = new SubscribeCommand { InstructorProfileId = Instructor.Id, StudentUserId = subscriptionReg.UserId };
                return Page();
            }
            else
            {
                // redirect to default registration page
                await _mediator.Send(new ThrowAlertCommand(AlertType.WARNING, "Instructor Not Found", "There is no instructor that matches the instructor Id provided."));
                return RedirectToPage("/Account/Register");
            }
        }

        public IActionResult OnPost()
        { 
            // update use profile with selection
            // use the student user id to update the student profile

            // update instructor default tribe with selection
            // use the instructor id to update the instructor profile

            return RedirectToPage("CompleteRegistration");
        }

        public class GetInstructorSubscriptionsQuery : IRequest<IEnumerable<Subscription>>
        {
            public string InstructorId { get; private set; }

            public GetInstructorSubscriptionsQuery(string instructorId) => InstructorId = instructorId;
        }

        public class GetInstructorSubscriptionsQueryHandler : IRequestHandler<GetInstructorSubscriptionsQuery, IEnumerable<Subscription>>
        {
            public Task<IEnumerable<Subscription>> Handle(GetInstructorSubscriptionsQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult(InitSubs(query.InstructorId));
            }

            private IEnumerable<Subscription> InitSubs(string instructorId)
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