using ApiGeo.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGeo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeolocalizarController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly SolicitudContext _context;
        private readonly Support.AmqpService amqpService;
        private const string QueueName = "Geocodificador";

        public GeolocalizarController(ILoggerFactory loggerFactory, SolicitudContext context, Support.AmqpService amqpService = null)
        {
            this._logger = loggerFactory.CreateLogger<GeolocalizarController>();
            this._context = context;
            this.amqpService = amqpService ?? throw new ArgumentNullException(nameof(amqpService));
        }

        public async Task<ActionResult<IEnumerable<Models.Solicitud>>> ListarTodo()
        {
            try
            {
                _logger.LogInformation("ListarTodo Requested");
                return await _context.SolicitudItems.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return BadRequest("HA OCURRIDO UN ERROR EN TIEMPOS DE EJECUCION");
            }
        }

        [HttpGet]
        public async Task<ActionResult<Models.Solicitud>> ListarPorId(int id)
        {
            try
            {
                _logger.LogInformation($"Consulta por id: {id}");

                var solicitudItem = await _context.SolicitudItems.FindAsync(id);

                if (solicitudItem == null)
                    return NotFound();

                return Ok(new
                {
                    id = solicitudItem.Id,
                    latitud = solicitudItem.Latitud,
                    longitud = solicitudItem.Longitud,
                    estado = solicitudItem.Estado
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return BadRequest("HA OCURRIDO UN ERROR EN TIEMPOS DE EJECUCION");
            }
        }

        [HttpPost]
        public async Task<ActionResult> geolocalizar(GeolocalizarRequest request)
        {

            using (var beginTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _logger.LogInformation(request.ToString());

                    //Verifico que el body llege con los campos solicitados.
                    Util.ParamVerify paramVerify = new Util.ParamVerify();
                    paramVerify.requiredParam(request.calle, "EL PARAMETRO CALLE NO PUEDE ESTAR VACIO");
                    paramVerify.requiredParam(request.numero, "EL PARAMETRO NUMERO NO PUEDE ESTAR VACIO");
                    paramVerify.requiredParam(request.ciudad, "EL PARAMETRO CIUDAD NO PUEDE ESTAR VACIO");
                    paramVerify.requiredParam(request.codigo_postal, "EL PARAMETRO CODIGO POSTAL NO PUEDE ESTAR VACIO");
                    paramVerify.requiredParam(request.provincia, "EL PARAMETRO PROVINCIA NO PUEDE ESTAR VACIO");
                    paramVerify.requiredParam(request.pais, "EL PARAMETRO PAIS NO PUEDE ESTAR VACIO");

                    Models.Solicitud solicitud = new Models.Solicitud();
                    solicitud.Calle = request.calle;
                    solicitud.Ciudad = request.ciudad;
                    solicitud.Numero = request.numero;
                    solicitud.Codigo_Postal = request.codigo_postal;
                    solicitud.Provincia = request.provincia;
                    solicitud.Pais = request.pais;
                    solicitud.Estado = Estados.PROCESANDO;

                    _context.SolicitudItems.Add(solicitud);
                    await _context.SaveChangesAsync();

                    //Confirmo las transacciones realizadas:
                    beginTransaction.CommitAsync();
                    
                    //Notifico mediante RabbitMQ al Geocodificador que se registro una nueva solicitud:
                    amqpService.PublishMessage(QueueName, solicitud);

                    //Devolvemos 202/Aceptado, como lo pide el ejercicio:
                    return Accepted(new { Id = solicitud.Id });

                }
                catch (Exception ex)
                {
                    //De ocurrir un error en tiempos de ejecucion me aseguro de restaurar los registros de la bd:
                    beginTransaction.RollbackAsync();
                    _logger.LogError(ex.Message.ToString());
                    return BadRequest("HA OCURRIDO UN ERROR EN TIEMPOS DE EJECUCION");
                }
            }            
        }
    }
}
