using Microsoft.AspNetCore.Mvc;
using Pot1_API.Models;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MiscController : Controller
    {
        private readonly Pot1Context _Contexto;

        public MiscController(Pot1Context Contexto)
        {
            _Contexto = Contexto;
        }
        [HttpGet]
        [Route("ObtenerEstados")]
        public IActionResult GetStates()
        {
            string[] estados =
            {
                "EN ESPERA DE INFORMACIÓN",
                "EN RESOLUCIÓN",
                "RESUELTO"
            };
            return Ok(estados);
        }
    }

}
