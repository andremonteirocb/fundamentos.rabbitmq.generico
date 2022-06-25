using RabbitMQ.Client;

namespace Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue
{
    /// <summary>
    /// Esse publisher foi desenhado para simplificar o envio de uma mensagem
    /// </summary>
    public class Publisher
    {
        private readonly IModel model;
        private readonly ILogger<Publisher> logger;
        public string Id { get; }

        public Publisher(IModel model, ILogger<Publisher> logger)
        {
            this.model = model;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");
        }

        public void HandlePublish<TModel>(TModel message, string exchange, string routingKey = "")
        {
            this.model.ConfirmSelect(); //Ack na publicação.

            try
            {
                this.model.BasicPublish(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: this.model.CreatePersistentBasicProperties().SetMessageId(Guid.NewGuid().ToString("D")), //Extension Method para criar um basic properties com persistência
                    body: message.Serialize().ToByteArray().ToReadOnlyMemory()); //Extension Method para simplificar a publicação

                this.model.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5)); //Ack na publicação.
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Erro ao publicar mensagem. Transação com banco foi abortada.");
            }

            this.model.Close();
            this.model.Dispose();
        }
    }
}
