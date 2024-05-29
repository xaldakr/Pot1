using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
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
            var result = (from u in _Contexto.Usuarios
                          join r in _Contexto.Roles on u.id_rol equals r.id_rol
                          join tr in _Contexto.Tipos_Rol on r.tipo_rol equals tr.id_tipo_rol
                          join t in _Contexto.Tickets on u.id_usuario equals t.id_encargado into ticketGroup
                          where tr.nombre == "Soporte"
                          select new
                          {
                              id_usuario = u.id_usuario,
                              nombre = u.nombre + " "+ u.apellido,
                              email = u.email,
                              telefono = u.telefono,
                              no_tickets = ticketGroup.Count()
                          })
                         .OrderByDescending(res => res.no_tickets)
                         .ToList();

            if (result.Count == 0)
            {
                return NotFound("No se han encontrado técnicos");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("ObtenerUsus/{tipo}")]
        public IActionResult GetUsers(int tipo, [FromQuery] string busqueda) {
            if (busqueda == null)
            {
                busqueda = "";
            }
            
            //Tipo debe ser dependiendo al rol que se esté agarrando, es decir, se debe comprobar que exista en una lista
            var roles = (from r in _Contexto.Roles
                         select r.id_rol).ToList();
            if (roles.Contains(tipo))
            {
                var usuarios = (from u in _Contexto.Usuarios
                                where u.id_rol == tipo && u.email.Contains(busqueda)
                                select new
                                {
                                    id_usuario =u.id_usuario,
                                    nombre = u.nombre + " " + u.apellido,
                                    email = u.email,
                                    telefono = u.telefono
                                }).OrderByDescending(res => res.nombre).ToList();
                if(usuarios.Count == 0)
                {
                    return NotFound("No se han encontrado usuarios");
                }
                else
                {
                    return Ok(usuarios);
                }
            }else
            {
                return BadRequest("Envie un parametro válido");
            }

        }
        [HttpPost]
        [Route("Login")]
        public IActionResult LogIn ([FromBody] JObject data)
        {
            if (data == null)
            {
                return BadRequest("No se ha enviado nada en el JSON.");
            }
            String correo;
            String contrasena;
            try
            {
                correo = data.Value<String>("correo");
                contrasena = data.Value<String>("contrasena");
            }
            catch (Exception)
            {
                return BadRequest("Formato inválido.");
            }
            var datologin = (from u in _Contexto.Usuarios
                             join r in _Contexto.Roles on
                             u.id_rol equals r.id_rol
                             join tr in _Contexto.Tipos_Rol on
                             r.tipo_rol equals tr.id_tipo_rol
                             where u.email == correo && u.contrasena == contrasena
                             select new
                             {
                                 id_usuario = u.id_usuario,
                                 tipo = tr.id_tipo_rol,
                             });
            if (datologin == null)
            {
                return NotFound("No se han encontrado datos");
            }
            return Ok(datologin);
        }
    }
}
