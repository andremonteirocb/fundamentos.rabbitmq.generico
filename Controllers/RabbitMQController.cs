using Microsoft.AspNetCore.Mvc;
using Fundamentos.RabbitMQ.Generico.Core.Infrastructure.Queue;
using Fundamentos.RabbitMQ.Generico.Models;

namespace Fundamentos.RabbitMQ.Generico.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitMQController : ControllerBase
    {
        private IConfiguration _configuration;
        private Publisher _publisher;
        public RabbitMQController(Publisher publisher, IConfiguration configuration)
        {
            _configuration = configuration;
            _publisher = publisher;
        }

        [HttpPost]
        public IActionResult Publicar([FromBody] Message payment)
        {
            _publisher.HandlePublish(payment, exchange: _configuration["RabbitMqConfig:DirectExchange"]);
            return Ok();
        }
    }
}