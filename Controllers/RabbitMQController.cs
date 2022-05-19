using Microsoft.AspNetCore.Mvc;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Models;

namespace Fundamentos.RabbitMQ.Generico.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitMQController : ControllerBase
    {
        private Publisher _publisher;
        public RabbitMQController(Publisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        public IActionResult Publicar([FromBody] Message payment)
        {
            _publisher.HandlePublish(payment, exchange: "novos-pedidos");
            return Ok();
        }
    }
}