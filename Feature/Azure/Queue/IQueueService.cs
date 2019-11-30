using System.Threading.Tasks;

namespace HAS.Registration.Feature.Azure.Queue
{
    public interface IQueueService
    {
        Task CreateQueue(string queueName);
        Task AddMessage<T>(T messageObj);
    }
}
