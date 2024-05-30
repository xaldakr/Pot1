using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Pot1_API.Models;
using Pot1_API.Services;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MiscController : Controller
    {
        private readonly Pot1Context _Contexto;
        private IConfiguration _configuration;


        public MiscController(Pot1Context Contexto, IConfiguration configuration)
        {
            _Contexto = Contexto;
            _configuration = configuration;
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
        [HttpGet]
        [Route("ObtenerRoles")]
        public IActionResult GetRoles()
        {
            var role = (from r in _Contexto.Roles select r).ToList();
            return Ok(role);
        }
        [HttpGet]
        [Route("ObtenerPrioridades")]
        public IActionResult GetPrios()
        {
            string[] prios =
            {
                "BAJA", "NORMAL", "IMPORTANTE", "CRÍTICA"
            };
            return Ok(prios);
        }



        ///APARTADO DE TAREAS
        [HttpGet]
        [Route("ObtenerTareas/{id}")]
        public IActionResult ObtenerTarea(int id)
        {
            var listareas =(from t in _Contexto.Tareas
                            where t.id_encargado == id && t.completada == false
                            select t).ToList();
            if(listareas.Count == 0)
            {
                return NotFound("No se han encontrado tareas");
            }
            else
            {
                return Ok(listareas);
            }
        }

        [HttpPost]
        [Route("CrearTarea")]
        public IActionResult CrearTarea([FromBody] JObject tareaJson)
        {
            // Extraer los datos del JObject
            string nombre = tareaJson.Value<string>("nombre");
            string info = tareaJson.Value<string>("info");
            string prioridad = tareaJson.Value<string>("prioridad");
            int id_ticket = tareaJson.Value<int>("id_ticket");
            int id_encargado = tareaJson.Value<int>("id_encargado"); 

            // Validar que la prioridad y el estado sean válidos
            var prioridadesValidas = new List<string> { "BAJA", "NORMAL", "IMPORTANTE", "CRÍTICA" };
            if (!prioridadesValidas.Contains(prioridad))
            {
                return BadRequest("Prioridad no válida.");
            }

            // Verificar que el ticket exista
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            if (ticket == null)
            {
                return NotFound("Ticket no encontrado.");
            }
            var encargado = _Contexto.Usuarios.FirstOrDefault(e => e.id_usuario == id_encargado);
            if (encargado == null)
            {
                return NotFound("No existe el encargado.");
            }
            var tipo_encargado = _Contexto.Roles.FirstOrDefault(tr => tr.tipo_rol != 1  && tr.id_rol == encargado.id_rol);
            if (tipo_encargado == null)
            {
                return BadRequest("No se puede asignar una tarea a un cliente.");
            }
            // Crear la nueva tarea
            var nuevaTarea = new Tarea
            {
                nombre = nombre,
                info = info,
                prioridad = prioridad,
                estado = "",
                completada = false,
                id_ticket = id_ticket,
                id_encargado = id_encargado
            };
            
            _Contexto.Tareas.Add(nuevaTarea);
            _Contexto.SaveChanges();
            Notificacion notificacion = new Notificacion
            {
                dato = "Se ha creado y asignado la tarea " + nuevaTarea.nombre+" al soporte " + encargado.nombre + " "+ encargado.apellido,
                url_archivo = "No existente",
                notificar_cliente = true,
                remitente = null,
                id_ticket = id_ticket,
            };
            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            var usuario = (from t in _Contexto.Tickets
                           join u in _Contexto.Usuarios on t.id_cliente equals u.id_usuario
                           where t.id_ticket == id_ticket
                           select u).FirstOrDefault();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarAsignacionTareaCorreo(encargado.email, nuevaTarea.nombre, nuevaTarea.prioridad, id_ticket, ticket.servicio);
            enviocorreo.EnviarComentarioTicketCorreo(usuario.email, id_ticket, "Sistema de Notificaciones Autogeneradas", ticket.servicio, notificacion.dato, "");
            return Ok("Tarea creada exitosamente.");
        }
        [HttpPut]
        [Route("RechazarTarea/{id_tarea}")]
        public IActionResult RechazarTarea(int id_tarea)
        {
            // Obtener la tarea de la base de datos
            var tarea = _Contexto.Tareas.FirstOrDefault(t => t.id_tarea == id_tarea);
            if (tarea == null)
            {
                return NotFound("Tarea no encontrada.");
            }
            var encargado = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tarea.id_encargado);
            if (encargado == null)
            {
                return BadRequest("Ejecución innecesaria.");
            }
            // Eliminar el encargado de la tarea
            tarea.id_encargado = null;
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == tarea.id_ticket);
            var duenotarea = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == ticket.id_encargado);

            _Contexto.SaveChanges();
            Notificacion notificacion = new Notificacion
            {
                dato = "El soporte "+ encargado.nombre + " " + encargado.apellido + " ha rechazado la tarea " + tarea.nombre + "."  ,
                url_archivo = "No existente",
                notificar_cliente = false,
                remitente = null,
                id_ticket = tarea.id_ticket,
            };
            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarRechazoTareaCorreo(duenotarea.email, encargado.nombre + " " + encargado.apellido, tarea.nombre, tarea.id_ticket, ticket.servicio);
            return Ok("Encargado de la tarea eliminado exitosamente.");
        }
        [HttpPut]
        [Route("AsignarTarea/{id_tarea}")]
        public IActionResult AsignarTarea(int id_tarea, [FromBody] JObject asignacionJson)
        {
            int idEncargado = asignacionJson.Value<int>("id_encargado");

            // Verificar que el usuario existe
            var usuario = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == idEncargado);
            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado.");
            }
            var tipo_encargado = _Contexto.Roles.FirstOrDefault(tr => tr.tipo_rol != 1 && tr.id_rol == usuario.id_rol);
            if (tipo_encargado == null)
            {
                return BadRequest("No se puede asignar una tarea a un cliente.");
            }
            // Obtener la tarea de la base de datos
            var tarea = _Contexto.Tareas.FirstOrDefault(t => t.id_tarea == id_tarea);
            if (tarea == null)
            {
                return NotFound("Tarea no encontrada.");
            }

            // Asignar el encargado a la tarea
            tarea.id_encargado = idEncargado;
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == tarea.id_ticket);
            _Contexto.SaveChanges();

            Notificacion notificacion = new Notificacion
            {
                dato = "Se ha asignado la tarea " + tarea.nombre + " al soporte " + usuario.nombre + " " + usuario.apellido,
                url_archivo = "No existente",
                notificar_cliente = false,
                remitente = null,
                id_ticket = tarea.id_ticket,
            };
            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarAsignacionTareaCorreo(usuario.email, tarea.nombre, tarea.prioridad, tarea.id_ticket, ticket.servicio);
            return Ok("Encargado asignado a la tarea exitosamente.");
        }

    }

}
