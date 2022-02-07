namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue
{
    public class ConsumerManager
    {
        private readonly IServiceProvider _serviceProvider;
        public ConsumerManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Add(string queue, int messagesPerSecond)
        {
            var consumer = _serviceProvider.GetRequiredService<Consumer>();
            consumer.Initialize(queue, messagesPerSecond);
        }
    }
}
