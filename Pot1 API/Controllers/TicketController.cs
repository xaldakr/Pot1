using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Pot1_API.Models;
using Pot1_API.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pot1_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly Pot1Context _Contexto;
        private IConfiguration _configuration;
        public TicketController(Pot1Context Contexto, IConfiguration configuration)
        {
            _Contexto = Contexto;
            _configuration = configuration;
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
        [Route("ObtenerSinAsignar")]
        public IActionResult GetUnasigned()
        {
            var listicket = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios on e.id_cliente equals c.id_usuario
                             where e.id_encargado == null
                             select new
                             {
                                 Id = e.id_ticket,
                                 Servicio = e.servicio,
                                 FechaDate = e.fecha,
                                 Fecha = e.fecha.ToString("dd/MMMM/yyyy"),
                                 Estado = e.estado,
                                 Cliente = c.nombre + " " + c.apellido,
                                 Correo = c.email,
                             })
                    .OrderByDescending(res => res.FechaDate)
                    .ToList();
            if (listicket.Count == 0)
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
        [Route("ObtenerPorFiltro/{id}")]
        public IActionResult GetFiltro(int id)
        {

            //el id es como lo siguiente
            //1 es categoria o servicio
            //2 es por fecha
            //3 es por nombre del encargado
            //4 es por nombre del cliente

            var listicket1 = (from e in _Contexto.Tickets
                             join c in _Contexto.Usuarios
                             on e.id_cliente equals c.id_usuario
                             join em in _Contexto.Usuarios on e.id_encargado equals em.id_usuario into emJoin
                             from em in emJoin.DefaultIfEmpty() where !e.estado.Contains("RESUELTO")
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
            switch (id)
            {
                case 1:
                    listicket = listicket1.OrderByDescending(res => res.Servicio).ToList(); 
                    break;
                case 2:
                    listicket = listicket1.OrderByDescending(res => res.FechaDate).ToList();
                    break;
                case 3:
                    listicket = listicket1.OrderByDescending(res => res.Empleado).ToList();
                    break;
                case 4:
                    listicket = listicket1.OrderByDescending(res => res.Cliente).ToList();
                    break;
                default:
                    break;
            }
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
        [HttpPost]
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

        //El main mastodonte
        [HttpGet]
        [Route("ObtenerDetalle/{id_ticket}")]
        public IActionResult GetTicketDetail(int id_ticket)
        {
            var ticketDetail = (from t in _Contexto.Tickets
                                join c in _Contexto.Usuarios on t.id_cliente equals c.id_usuario
                                join e in _Contexto.Usuarios on t.id_encargado equals e.id_usuario into encargadoGroup
                                from e in encargadoGroup.DefaultIfEmpty()
                                where t.id_ticket == id_ticket
                                select new
                                {
                                    ClienteNombre = c.nombre,
                                    ClienteApellido = c.apellido,
                                    ClienteTelefono = c.telefono,
                                    Servicio = t.servicio,
                                    Estado = t.estado,
                                    Prioridad = t.prioridad,
                                    Descripcion = t.descripcion,
                                    Fecha = t.fecha,
                                    Encargado = e == null ? null : e.nombre + " " + e.apellido,
                                    Archivos = _Contexto.Archivos.Where(a => a.id_ticket == id_ticket).Select(a => a.url).ToList(),
                                    Tareas = _Contexto.Tareas.Where(tr => tr.id_ticket == id_ticket).Select(tr => new
                                    {
                                        tr.id_tarea,
                                        tr.nombre,
                                        tr.prioridad,
                                        tr.info,
                                        tr.estado,
                                        Encargado = tr.id_encargado == null ? null : _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tr.id_encargado).nombre + " " + _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tr.id_encargado).apellido
                                    }).ToList(),
                                    Notificaciones = _Contexto.Notificaciones.Where(n => n.id_ticket == id_ticket).Select(n => new
                                    {
                                        Remitente = n.remitente == null ? (n.autogenerada ? "Autogenerada" : "Anonimo") : _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == n.remitente).nombre + " " + _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == n.remitente).apellido,
                                        n.dato,
                                        UrlArchivo = n.url_archivo == "No existente" ? null : n.url_archivo
                                    }).ToList()
                                }).FirstOrDefault();

            if (ticketDetail == null)
            {
                return NotFound("No se ha encontrado el ticket.");
            }

            return Ok(ticketDetail);
        }
        [HttpGet]
        [Route("ObtenerDetalleCliente/{id_ticket}")]
        public IActionResult GetTicketDetailClient(int id_ticket)
        {
            var ticketDetail = (from t in _Contexto.Tickets
                                join c in _Contexto.Usuarios on t.id_cliente equals c.id_usuario
                                join e in _Contexto.Usuarios on t.id_encargado equals e.id_usuario into encargadoGroup
                                from e in encargadoGroup.DefaultIfEmpty()
                                where t.id_ticket == id_ticket
                                select new
                                {
                                    ClienteNombre = c.nombre,
                                    ClienteApellido = c.apellido,
                                    ClienteTelefono = c.telefono,
                                    Servicio = t.servicio,
                                    Estado = t.estado,
                                    Prioridad = t.prioridad,
                                    Descripcion = t.descripcion,
                                    Fecha = t.fecha,
                                    Encargado = e == null ? null : e.nombre + " " + e.apellido,
                                    Archivos = _Contexto.Archivos.Where(a => a.id_ticket == id_ticket).Select(a => a.url).ToList(),
                                    Tareas = _Contexto.Tareas.Where(tr => tr.id_ticket == id_ticket).Select(tr => new
                                    {
                                        tr.id_tarea,
                                        tr.nombre,
                                        tr.prioridad,
                                        tr.info,
                                        tr.estado,
                                        Encargado = tr.id_encargado == null ? null : _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tr.id_encargado).nombre + " " + _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tr.id_encargado).apellido
                                    }).ToList(),
                                    Notificaciones = _Contexto.Notificaciones.Where(n => n.id_ticket == id_ticket && n.notificar_cliente).Select(n => new
                                    {
                                        Remitente = n.remitente == null ? (n.autogenerada ? "Autogenerada" : "Anonimo") : _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == n.remitente).nombre + " " + _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == n.remitente).apellido,
                                        n.dato,
                                        UrlArchivo = n.url_archivo == "No existente" ? null : n.url_archivo
                                    }).ToList()
                                }).FirstOrDefault();

            if (ticketDetail == null)
            {
                return NotFound("No se ha encontrado el ticket.");
            }

            return Ok(ticketDetail);
        }
        [HttpPost]
        [Route("CrearTicket")]
        public async Task<IActionResult> CrearTicket()
        {
            var form = await Request.ReadFormAsync();

            // Obtener los datos del form
            string descripcion = form["descripcion"];
            string servicio = form["servicio"];
            string estado = "CREADO";
            string prioridad = form["prioridad"];
            int id_cliente = int.Parse(form["id_cliente"]);
            DateTime fecha = DateTime.Now;
            int? id_encargado = null;

            // Obtener el archivo (si hay alguno)
            IFormFile archivo = form.Files.FirstOrDefault();

            if (descripcion.IsNullOrEmpty() || servicio.IsNullOrEmpty())
            {
                return BadRequest("Debe haber datos en la descripción y servicio");
            }
            // Verificar si el archivo fue enviado
            bool archivoEnviado = archivo != null;
            string urlArchivoCargado = "";
            //Si fue enviado, hacer lo de firebase
            if(archivoEnviado) {
                //Datos de FB
                try
                {
                    Stream archivoASubir = archivo.OpenReadStream();
                    string emailFB = "pot1tickets@gmail.com";
                    string claveFB = "merequetengue";
                    string rutaFB = "pot1-tickets.appspot.com";
                    string ApiKeyFB = "AIzaSyApIl4UPhpZWOa8xchSMAP5ZjbCTwppF6I";

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKeyFB));
                    var autentificarFB = await auth.SignInWithEmailAndPasswordAsync(emailFB, claveFB);

                    var cancellation = new CancellationTokenSource();
                    var tokenUser = autentificarFB.FirebaseToken;

                    var tareaCargarArchivo = new FirebaseStorage(rutaFB, new FirebaseStorageOptions
                          {
                           AuthTokenAsyncFactory = () => Task.FromResult(tokenUser), ThrowOnCancel = true
                          }).Child("Archivos")
                          .Child(archivo.FileName)
                          .PutAsync(archivoASubir, cancellation.Token);
                    var archivoCargado = await tareaCargarArchivo;

                    urlArchivoCargado = archivoCargado.ToString();

                } catch (Exception ex)
                {
                    archivoEnviado = false;
                    urlArchivoCargado = "";
                }
                
            }
            var nuevoTicket = new Ticket
            {
                estado = estado,
                descripcion = descripcion,
                prioridad = prioridad,
                servicio = servicio,
                fecha = fecha,
                id_cliente = id_cliente,
                id_encargado = id_encargado,
            };
            _Contexto.Tickets.Add(nuevoTicket);
            _Contexto.SaveChanges();

            if (!urlArchivoCargado.IsNullOrEmpty())
            {
                var nuevoArchivo = new Archivo
                {
                    url = urlArchivoCargado,
                    id_ticket = nuevoTicket.id_ticket,
                };
                _Contexto.Archivos.Add(nuevoArchivo);
                _Contexto.SaveChanges();
            }
            string correouser = (from u in _Contexto.Usuarios where u.id_usuario == id_cliente select u.email).FirstOrDefault();

            correo enviarconfirmacion = new correo(_configuration);
            enviarconfirmacion.EnviarTicketCorreo(correouser, nuevoTicket.id_ticket, nuevoTicket.servicio, nuevoTicket.descripcion);

            return Ok(new {id_ticket = nuevoTicket.id_ticket}); 
        }
        [HttpPut]
        [Route("EditarTicket/{id_ticket}")]
        public IActionResult EditarTicket(int id_ticket, [FromBody] JObject ticketJson)
        {
            string nuevoEstado = ticketJson.Value<string>("estado");
            int idEjecutor = ticketJson.Value<int>("id");

            var estadosValidos = new List<string> { "CREADO", "ASIGNADO", "EN ESPERA DE INFORMACIÓN", "EN RESOLUCIÓN", "RESUELTO" };
            if (!estadosValidos.Contains(nuevoEstado))
            {
                return BadRequest("Estado no válido.");
            }

            // Obtener el ticket de la base de datos
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            if (ticket == null)
            {
                return NotFound("Ticket no encontrado.");
            }

            // Verificar si el id_ejecutor es el encargado del ticket o un administrador
            var usuarioEjecutor = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == idEjecutor);
            if (usuarioEjecutor == null)
            {
                return BadRequest("Aún no se ha asignado un técnico para editar su estado");
            }

            bool esEncargado = ticket.id_encargado == idEjecutor;
            bool esAdministrador = usuarioEjecutor.id_rol == 3;

            if (!esEncargado && !esAdministrador)
            {
                return Forbid("No tienes permisos para editar este ticket.");
            }

            // Verificar si el ticket ya está cerrado
            if (ticket.estado == "RESUELTO")
            {
                return BadRequest("El ticket ya está cerrado.");
            }

            // Si el nuevo estado es RESUELTO, verificar que todas las tareas estén completadas
            if (nuevoEstado == "RESUELTO")
            {
                var tareasIncompletas = _Contexto.Tareas.Any(t => t.id_ticket == id_ticket && !t.completada);
                if (tareasIncompletas)
                {
                    return BadRequest("No se puede resolver el ticket porque hay tareas incompletas.");
                }
            }

            // Actualizar el estado del ticket
            ticket.estado = nuevoEstado;
            ticket.resuelta = nuevoEstado == "RESUELTO";
            _Contexto.SaveChanges();

            Notificacion notificacion = new Notificacion {
                dato = nuevoEstado == "RESUELTO"? "El ticket se ha cerrado":"Estado del ticket cambiado a " + nuevoEstado,
                url_archivo = "No existente",
                notificar_cliente = true,
                fecha = DateTime.Now,
                remitente = idEjecutor,
                id_ticket = id_ticket,
            };

            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();

            var usuario = (from t in _Contexto.Tickets
                           join u in _Contexto.Usuarios on t.id_cliente equals u.id_usuario 
                           where t.id_ticket == id_ticket select u).FirstOrDefault();
            correo enviocorreo = new correo(_configuration);
            if (nuevoEstado == "RESUELTO")
            {
                enviocorreo.EnviarCierreTicketCorreo(usuario.email, id_ticket, ticket.servicio);
            }
            else
            {
                enviocorreo.EnviarCambioEstadoTicketCorreo(usuario.email, id_ticket, ticket.servicio, nuevoEstado);
            }

            return Ok("Estado del ticket actualizado exitosamente.");
        }
        [HttpPut]
        [Route("AsignarSoporte/{id_ticket}")]
        public IActionResult AsignarSoporte(int id_ticket, [FromBody] JObject asignacionJson)
        {
            int idSoporte = asignacionJson.Value<int>("id_soporte");

            // Verificar que el usuario sea un soporte
            var usuarioSoporte = (from u in _Contexto.Usuarios
                                  join r in _Contexto.Roles on u.id_rol equals r.id_rol
                                  where r.id_rol == 2 && u.id_usuario == idSoporte
                                  select new
                                  {
                                      id_rol = r.id_rol,
                                      correo = u.email,
                                      nombre = u.nombre + " " +u.apellido,
                                  }).FirstOrDefault();
            if (usuarioSoporte == null)
            {
                return BadRequest("Usuario de soporte no encontrado.");
            }

            // Obtener el ticket de la base de datos
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            if (ticket == null)
            {
                return NotFound("Ticket no encontrado.");
            }

            // Verificar que el ticket no tenga ya un encargado asignado
            if (ticket.id_encargado != null)
            {
                return BadRequest("El ticket ya tiene un encargado asignado.");
            }

            // Asignar el ID de soporte y cambiar el estado del ticket
            ticket.id_encargado = idSoporte;
            ticket.estado = "ASIGNADO";

            
            _Contexto.SaveChanges();

            Notificacion notificacion = new Notificacion
            {
                dato = "Se ha asignado el ticket al soporte " + usuarioSoporte.nombre,
                url_archivo = "No existente",
                notificar_cliente = true,
                remitente = null,
                fecha = DateTime.Now,
                id_ticket = id_ticket,
            };

            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            var usuario = (from t in _Contexto.Tickets
                           join u in _Contexto.Usuarios on t.id_cliente equals u.id_usuario
                           where t.id_ticket == id_ticket
                           select u).FirstOrDefault();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarAsignacionTicketCorreo(usuarioSoporte.correo, ticket.id_ticket, ticket.servicio, ticket.prioridad);
            enviocorreo.EnviarCambioEstadoTicketCorreo(usuario.email, ticket.id_ticket, ticket.servicio, "ASIGNADO");
            return Ok("Soporte asignado al ticket exitosamente.");
        }

    }
}
