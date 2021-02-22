using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using ApiGeo.Support;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGeo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitTestController : ControllerBase
    {
        private readonly AmqpService amqpService;

        public RabbitTestController(AmqpService amqpService)
        {
            this.amqpService = amqpService ?? throw new ArgumentNullException(nameof(amqpService));
        }

        [HttpPost]
        public IActionResult PublishMessage([FromBody] object message)
        {
            amqpService.PublishMessage("NotifGeocodificador", message);
            return Ok();
        }
    }
}
