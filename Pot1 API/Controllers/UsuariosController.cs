using Microsoft.AspNetCore.Mvc;
using Pot1_API.Models;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : Controller
    {
        private readonly Pot1Context _Contexto;

        public UsuariosController(Pot1Context Contexto)
        {
            _Contexto = Contexto;
        }
        [HttpGet]
        [Route("ObtenerDash")]
        public IActionResult GetDash()
        {

        }
    }
}
