using Microsoft.AspNetCore.Mvc;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Models;

namespace Fundamentos.RabbitMQ.Generico.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitMQController : ControllerBase
    {
        private ConsumerManager _consumer;
        private Publisher _publisher;
        public RabbitMQController(Publisher publisher, ConsumerManager consumer)
        {
            _publisher = publisher;

            _consumer = consumer;
            _consumer.Add("processar-pagamentos", 2);
        }

        [HttpPost]
        public IActionResult Publicar([FromBody] Message payment)
        {
            _publisher.HandlePublish(payment, exchange: "novos-pedidos");
            return Ok();
        }
    }
}