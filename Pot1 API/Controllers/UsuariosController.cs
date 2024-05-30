using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Pot1_API.Models;
using Pot1_API.Services;
using System.Text.RegularExpressions;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : Controller
    {
        private readonly Pot1Context _Contexto;
        private IConfiguration _configuration;
        public UsuariosController(Pot1Context Contexto, IConfiguration configuration)
        {
            _Contexto = Contexto;
            _configuration = configuration;
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
        public IActionResult GetUsers(int tipo, [FromQuery] string busqueda = "") {
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
        [HttpGet]
        [Route("ObtenerTecnicos")]
        public IActionResult GetTecnicos([FromQuery] string busqueda = "")
        {
            if (busqueda == null)
            {
                busqueda = "";
            }

            var roles = (from r in _Contexto.Roles
                         select r.tipo_rol).ToList();
            if (roles.Contains(2))
            {
                var usuarios = (from u in _Contexto.Usuarios
                                join r in _Contexto.Roles
                                on u.id_rol equals r.id_rol
                                where r.tipo_rol == 2 && u.email.Contains(busqueda)
                                select new
                                {
                                    id_usuario = u.id_usuario,
                                    nombre = u.nombre + " " + u.apellido,
                                    email = u.email,
                                    telefono = u.telefono
                                }).OrderByDescending(res => res.nombre).ToList();
                if (usuarios.Count == 0)
                {
                    return NotFound("No se han encontrado usuarios");
                }
                else
                {
                    return Ok(usuarios);
                }
            }
            else
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
                             }).FirstOrDefault();
            if (datologin == null)
            {
                return NotFound("No se han encontrado datos");
            }
            return Ok(datologin);
        }
        /*Modelo de API pal post y put
        POST /api/Usuarios/CrearUsuario
            {
                "Nombre": "Carlos",
                "Apellido": "Fuentes",
                "Telefono": "2475-3465",
                "Email": "carlosfue@gmail.com",
                "Contrasena": "2fl23q23l5",
                "TelContacto": "2345-2351",
                "IdRol": 1
            }
        */
        [HttpPost]
        [Route("CrearUsuario")]
        public IActionResult CrearUsuario([FromBody] JObject usuarioJson)
        {
            // Extraer los datos del JObject
            string nombre = usuarioJson.Value<string>("Nombre");
            string apellido = usuarioJson.Value<string>("Apellido");
            string telefono = usuarioJson.Value<string>("Telefono");
            string email = usuarioJson.Value<string>("Email");
            string contrasena = usuarioJson.Value<string>("Contrasena");
            string telContacto = usuarioJson.Value<string>("TelContacto");
            int idRol = usuarioJson.Value<int>("IdRol");

            // Validar formato del correo
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("El correo electrónico no tiene un formato válido.");
            }

            // Validar formato del teléfono y del teléfono de contacto
            if (!Regex.IsMatch(telefono, @"^\d{4}-\d{4}$") || !Regex.IsMatch(telContacto, @"^\d{4}-\d{4}$"))
            {
                return BadRequest("Los números de teléfono deben tener el formato ####-####.");
            }

            // Validar que el teléfono y el teléfono de contacto no sean iguales
            if (telefono == telContacto)
            {
                return BadRequest("El teléfono de contacto no puede ser igual al teléfono principal.");
            }

            // Verificar la capacidad del rol
            var rol = _Contexto.Roles.Include(r => r.tipo_rol).FirstOrDefault(r => r.id_rol == idRol);
            if (rol == null)
            {
                return NotFound("El rol especificado no existe.");
            }

            int cantidadUsuariosConRol = _Contexto.Usuarios.Count(u => u.id_rol== idRol);
            if (cantidadUsuariosConRol >= rol.capacidad)
            {
                return BadRequest("La capacidad del rol ha sido alcanzada.");
            }

            // Crear el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                nombre = nombre,
                apellido = apellido,
                telefono = telefono,
                email = email,
                contrasena = contrasena,
                tel_contacto = telContacto,
                id_rol = idRol
            };

            _Contexto.Usuarios.Add(nuevoUsuario);
            _Contexto.SaveChanges();

            correo enviarbienvenida = new correo(_configuration);
            enviarbienvenida.EnviarBienvenidaUsuarioCorreo(email, rol.nombre, nombre, apellido, telefono, telContacto, email, contrasena);

            return Ok(nuevoUsuario);
        }
        [HttpPut]
        [Route("EditarUsuario/{id_usuario}")]
        public IActionResult EditarUsuario(int id_usuario, [FromBody] JObject usuarioJson)
        {
            // Extraer los datos del JObject
            string nombre = usuarioJson.Value<string>("Nombre");
            string apellido = usuarioJson.Value<string>("Apellido");
            string telefono = usuarioJson.Value<string>("Telefono");
            string email = usuarioJson.Value<string>("Email");
            string contrasena = usuarioJson.Value<string>("Contrasena");
            string telContacto = usuarioJson.Value<string>("TelContacto");
            int idRol = usuarioJson.Value<int>("IdRol");

            // Validar formato del correo
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest("El correo electrónico no tiene un formato válido.");
            }

            // Validar formato del teléfono y del teléfono de contacto
            if (!Regex.IsMatch(telefono, @"^\d{4}-\d{4}$") || !Regex.IsMatch(telContacto, @"^\d{4}-\d{4}$"))
            {
                return BadRequest("Los números de teléfono deben tener el formato ####-####.");
            }

            // Validar que el teléfono y el teléfono de contacto no sean iguales
            if (telefono == telContacto)
            {
                return BadRequest("El teléfono de contacto no puede ser igual al teléfono principal.");
            }

            // Verificar la capacidad del rol
            var rol = _Contexto.Roles.Include(r => r.tipo_rol).FirstOrDefault(r => r.id_rol == idRol);
            if (rol == null)
            {
                return NotFound("El rol especificado no existe.");
            }

            int cantidadUsuariosConRol = _Contexto.Usuarios.Count(u => u.id_rol == idRol);
            var usuarioExistente = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == id_usuario);
            if (usuarioExistente == null)
            {
                return NotFound("El usuario especificado no existe.");
            }

            // Si el usuario ya tiene el rol, no incrementar el conteo
            if (usuarioExistente.id_rol != idRol && cantidadUsuariosConRol >= rol.capacidad)
            {
                return BadRequest("La capacidad del rol ha sido alcanzada.");
            }

            // Actualizar el usuario existente
            usuarioExistente.nombre = nombre;
            usuarioExistente.apellido = apellido;
            usuarioExistente.telefono = telefono;
            usuarioExistente.email = email;
            usuarioExistente.contrasena = contrasena;
            usuarioExistente.tel_contacto = telContacto;
            usuarioExistente.id_rol = idRol;

            _Contexto.SaveChanges();

            correo enviarbcambio = new correo(_configuration);
            enviarbcambio.EnviarCambioDatosIngresoCorreo(email, rol.nombre, nombre, apellido, telefono, telContacto, email, contrasena);

            return Ok("Usuario actualizado exitosamente.");
        }
    }
}
