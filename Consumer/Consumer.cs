using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Fundamentos.RabbitMQ.Generico.Extensions;

namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue
{
    public class Consumer<T>
    {
        private readonly IModel model;
        private readonly ILogger<T> logger;
        private EventingBasicConsumer eventingBasicConsumer;

        public int MessagesPerSecond { get; private set; }
        public Action<T> Action { get; private set; }

        public string Queue;
        public string Id { get; }

        public Consumer(IModel model, ILogger<T> logger)
        {
            this.model = model;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");

            this.eventingBasicConsumer = new EventingBasicConsumer(model);
            this.eventingBasicConsumer.Received += this.OnMessage;
        }

        private void OnMessage(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (this.MessagesPerSecond != 0)
                this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

            T result = default(T);

            try
            {
                result = eventArgs.Body.ToArray().ToUTF8String().Deserialize<T>();
            }
            catch (Exception ex)
            {
                if (this.model.IsOpen)
                {
                    this.model.BasicReject(eventArgs.DeliveryTag, false);
                    this.logger.LogError(ex, "Mensagem sofreu uma rejeição grave em função de um erro na desserialização. A mensagem será descartada");
                }
                return;
            }

            try
            {
                Action(result);
                this.model.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                if (this.model.IsOpen)
                {
                    this.model.BasicNack(eventArgs.DeliveryTag, false, true);
                    this.logger.LogError(ex, "Mensagem foi reenfileirada para processamento futuro, o consumidor atual não conseguiu processá-la.");
                }
            }
        }

        public void QueueBind(string queue, int messagesPerSecond, Action<T> action)
        {
            this.Queue = queue;
            this.MessagesPerSecond = messagesPerSecond;
            this.Action = action;

            if (this.MessagesPerSecond > 0)
                this.model.SetPrefetchCount((ushort)(this.MessagesPerSecond * 5));
            else
                this.model.SetPrefetchCount((ushort)(1000));

            this.model.BasicConsume(this.Queue, false, this.eventingBasicConsumer);
        }
    }
}