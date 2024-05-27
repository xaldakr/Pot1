using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Pot1_API.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly Pot1Context _Contexto;

        public TicketController(Pot1Context Contexto)
        {
            _Contexto = Contexto;
        }

        [HttpGet]
        [Route("ObtenerDash")]
        public IActionResult GetDash()
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios on e.id_encargado equals em.id_usuario into emJoin
                             from em in emJoin.DefaultIfEmpty()
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Empleado = em == null ? null : em.nombre + " " + em.apellido,
                                 Correo = c.email,
                             })
                    .OrderByDescending(res => res.FechaDate)
                    .Take(30)
                    .ToList();
            if (listicket.Count == 0)
            {
                return NotFound("No se han encontrado tickets");
            }
            return Ok(listicket);
        }

        [HttpGet]
        [Route("ObtenerTodo")]
        public IActionResult GetAll()
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios on e.id_encargado equals em.id_usuario into emJoin
                             from em in emJoin.DefaultIfEmpty()
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Empleado = em == null ? null : em.nombre + " " + em.apellido,
                                 Correo = c.email,
                             })
                    .OrderByDescending(res => res.FechaDate)
                    .ToList();
            if (listicket.Count == 0 )
            {
                return NotFound("No se han encontrado tickets");
            }
            return Ok(listicket);
        }

        [HttpGet]
        [Route("ObtenerPorCliente/{id}")]
        public IActionResult GetClien(int id)
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios on e.id_encargado equals em.id_usuario into emJoin
                             from em in emJoin.DefaultIfEmpty()
                             where c.id_usuario == id
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Empleado = em == null ? null : em.nombre + " " + em.apellido,
                                 Correo = c.email,
                             }).OrderByDescending(res => res.FechaDate).ToList();
            if (listicket.Count == 0)
            {
                return NotFound("No se han encontrado tickets");
            }
            return Ok(listicket);
        }

        [HttpGet]
        [Route("ObtenerPorEncargado/{id}")]
        public IActionResult GetEncar(int id)
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios
                             on e.id_encargado equals em.id_usuario
                             where em.id_usuario == id && !e.estado.Contains("RESUELTO")
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Empleado = em.nombre + " " + em.apellido,
                                 Correo = c.email,
                             }).OrderByDescending(res => res.FechaDate).ToList();
            if (listicket.Count == 0)
            {
                return NotFound("No se han encontrado tickets");
            }
            return Ok(listicket);
        }
        [HttpGet]
        [Route("ObtenerPorEstado")]
        public IActionResult GetPorEstado([FromBody] JObject data)
        {
            if (data == null)
            {
                return BadRequest("No se ha enviado nada en el JSON.");
            }

            // Descomponer y validar los datos
            DateTime inicio;
            DateTime fin;
            int tipo;
            //  1 es Resueltos
            //  2 es En Progreso
            //  3 es Abiertos

            try
            {
                inicio = data.Value<DateTime>("inicio");
                fin = data.Value<DateTime>("fin");
                tipo = data.Value<int>("tipo");
            }
            catch (Exception)
            {
                return BadRequest("Formato inválido.");
            }
            if(tipo > 3 || tipo < 1){
                return BadRequest("Tipo no especificado");
            }
            // Validar las fechas
            if (inicio > fin || inicio == default(DateTime) || fin == default(DateTime))
            {
                inicio = DateTime.Today;
                fin = DateTime.Today.AddDays(1).AddTicks(-1); // Fin del día de hoy
            }
            var listicket1 = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios on e.id_encargado equals em.id_usuario into emJoin
                             from em in emJoin.DefaultIfEmpty()
                             where e.fecha >= inicio && e.fecha <= fin
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Empleado = em == null ? null : em.nombre + " " + em.apellido,
                                 Correo = c.email,
                             }).OrderByDescending(res => res.FechaDate).ToList();
            var listicket = listicket1;
            if(tipo == 1)
            {
                listicket = (from e in listicket1
                             where e.Estado.Contains("RESUELTO")
                             select e).OrderByDescending(res => res.FechaDate).ToList();
            } else if (tipo == 2)
            {
                listicket = (from e in listicket1
                             where !e.Estado.Contains("RESUELTO") && !e.Estado.Contains("CREADO")
                             select e).OrderByDescending(res => res.FechaDate).ToList();
            } else
            {
                listicket = (from e in listicket1
                             where e.Estado.Contains("CREADO")
                             select e).OrderByDescending(res => res.FechaDate).ToList();
            }
            if (listicket.Count == 0)
            {
                return NotFound("No se han encontrado tickets");
            }
            return Ok(listicket);
        }
    }
}
