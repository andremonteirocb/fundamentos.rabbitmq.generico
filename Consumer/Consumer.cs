using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Fundamentos.RabbitMQ.Generico.Extensions;
using Fundamentos.RabbitMQ.Generico.Models;

namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue
{
    public class Consumer
    {
        private readonly IModel model;
        private readonly ILogger<Consumer> logger;
        private EventingBasicConsumer eventingBasicConsumer;
        private bool isRunning;

        public int MessagesPerSecond { get; private set; }
        public string Queue;
        public string Id { get; }

        public Consumer(IModel model, ILogger<Consumer> logger)
        {
            this.model = model;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");

            this.eventingBasicConsumer = new EventingBasicConsumer(model);
            this.eventingBasicConsumer.Received += this.OnMessage;
        }

        private void OnMessage(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (this.isRunning == false)
            {
                if (this.model.IsOpen)
                {
                    this.model.BasicReject(eventArgs.DeliveryTag, true);
                    this.logger.LogInformation("Mensagem sofreu rejeição leve em função do desligamento do consumidor");
                }
                return;
            }

            if (this.MessagesPerSecond != 0)
                this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

            Message message;

            try
            {
                message = eventArgs.Body.ToArray().ToUTF8String().Deserialize<Message>();
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


            if (this.isRunning)
            {
                try
                {
                    if (this.isRunning)
                    {
                        if (this.isRunning)
                        {
                            this.model.BasicAck(eventArgs.DeliveryTag, false);
                        }
                        else
                        {
                            this.logger.LogInformation("Abordando procesamento sem ack e sem commit");
                        }
                    }
                    else
                    {
                        if (this.model.IsOpen)
                        {
                            this.model.BasicReject(eventArgs.DeliveryTag, true);
                            this.logger.LogInformation("Mensagem sofreu rejeição leve em função do desligamento do consumidor");
                        }
                    }
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
            else
            {
                if (this.model.IsOpen) 
                this.model.BasicReject(eventArgs.DeliveryTag, true);
            }
        }

        public void Initialize(string queue, int messagesPerSecond)
        {
            this.Queue = queue;
            this.MessagesPerSecond = messagesPerSecond;

            if (this.MessagesPerSecond > 0)
                this.model.SetPrefetchCount((ushort)(this.MessagesPerSecond * 5));
            else
                this.model.SetPrefetchCount((ushort)(1000));

            this.isRunning = true;
            this.model.BasicConsume(this.Queue, false, this.eventingBasicConsumer);
        }
    }
}
