using Microsoft.AspNetCore.Mvc;
using Pot1_API.Models;

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
        [Route("ObtenerTodo")]
        public IActionResult GetAll()
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios
                             on e.id_encargado equals em.id_usuario
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
                             } ).OrderByDescending(res => res.FechaDate).ToList();
            if (listicket.Count ==0 )
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
                             join em in _Contexto.Usuarios
                             on e.id_encargado equals em.id_usuario
                             where c.id_usuario == id
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
        [Route("ObtenerPorEncargado/{id}")]
        public IActionResult GetEncar(int id)
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios
                             on e.id_encargado equals em.id_usuario
                             where em.id_usuario == id
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
    }
}
