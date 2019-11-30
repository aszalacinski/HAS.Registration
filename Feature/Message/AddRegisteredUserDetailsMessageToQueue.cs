using System.Threading;
using System.Threading.Tasks;
using HAS.Registration.Feature.Azure.Queue;
using HAS.Registration.Feature.Identity;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace HAS.Registration.Feature.Message
{
    public class AddRegistrationCompletedEventToQueue
    {
        public class AddRegistrationCompletedEventToQueueCommand : IRequest<bool>
        {
            public string Email { get; private set; }
            public string UserId { get; private set; }

            public AddRegistrationCompletedEventToQueueCommand(string email, string userId)
            {
                Email = email;
                UserId = userId;
            }
        }

        public class AddRegistrationCompletedEventToQueueCommandHandler : IRequestHandler<AddRegistrationCompletedEventToQueueCommand, bool>
        {
            public IQueueService _queueService;

            public AddRegistrationCompletedEventToQueueCommandHandler(IConfiguration configuration)
            {
                _queueService = AzureStorageQueueService.Create(configuration["Azure:Storage:Events:ConnectionString"]);
                _queueService.CreateQueue(configuration["Azure:Storage:Queue:RegistrationEvent:Name"]);
            }

            public async Task<bool> Handle(AddRegistrationCompletedEventToQueueCommand cmd, CancellationToken cancellationToken)
            {
                await _queueService.AddMessage<RegisteredUserDetailsMessage>(new RegisteredUserDetailsMessage { Email = cmd.Email, UserId = cmd.UserId });

                return true;
            }
        }
    }
}
