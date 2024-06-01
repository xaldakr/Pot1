using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Pot1_API.Models;
using Pot1_API.Services;
using System.Net.Sockets;

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
        [HttpGet]
        [Route("ObtenerTiposTicket")]
        public IActionResult GetTtick()
        {
            int numabier = (from t in _Contexto.Tickets
                            where t.estado.Contains("CREADO") select t).Count();
            int numenprog = (from t in _Contexto.Tickets
                             where !t.estado.Contains("RESUELTO") && !t.estado.Contains("CREADO")
                             select t).Count();
            int numcerrado = (from t in _Contexto.Tickets
                              where t.estado.Contains("RESUELTO")
                              select t).Count();
            return Ok(new
            {
                noabiertos = numabier,
                noprogreso = numenprog,
                nocerrados = numcerrado
            });
        }


        ///APARTADO DE TAREAS
        [HttpGet]
        [Route("ObtenerTareas/{id}/{tipo}")]
        public IActionResult ObtenerTarea(int id, [FromQuery] int? idticket = -1, int? tipo = 0, [FromQuery] string? nombre = "")
        {
            //tipo 1 no resueltos
            //tipo 2 resueltos
            idticket = idticket == null ? -1 : idticket;
            nombre = nombre == null ? "" : nombre;
            var listareas = (from t in _Contexto.Tareas
                             join ti in _Contexto.Tickets on t.id_ticket equals ti.id_ticket
                             join c in _Contexto.Usuarios on ti.id_cliente equals c.id_usuario
                             where t.id_encargado == id && t.nombre.Contains(nombre)
                             select new
                             {
                                 id_tarea= t.id_tarea,
                                 id_ticket = t.id_ticket,
                                 id_encargado =t.id_encargado,
                                 nombre= t.nombre,
                                 info = t.info,
                                 prioridad = t.prioridad,
                                 estado = t.estado,
                                 completada = t.completada,
                                 servicio = ti.servicio,
                                 cliente = c.nombre + " " + c.apellido
                             }).ToList();
            var listareas1 = listareas;
            if (idticket != -1)
            {
                listareas1 = (from l in listareas
                              where l.id_ticket == idticket
                              select l).ToList();
            }
            if(tipo == 1)
            {
                listareas1 = ( from l in listareas1
                               where l.completada == false select l).ToList();
            } else if (tipo == 2)
            {
                listareas1 = (from l in listareas1
                              where l.completada == true
                              select l).ToList();
            }
            if (listareas1.Count == 0)
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
            var tipo_encargado = _Contexto.Roles.FirstOrDefault(tr => tr.tipo_rol != 1 && tr.id_rol == encargado.id_rol);
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
                dato = "Se ha creado y asignado la tarea " + nuevaTarea.nombre + " al soporte " + encargado.nombre + " " + encargado.apellido,
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
            enviocorreo.EnviarAsignacionTareaCorreo(encargado.email, nuevaTarea.nombre, nuevaTarea.prioridad, id_ticket, ticket.servicio);
            enviocorreo.EnviarComentarioTicketCorreo(usuario.email, id_ticket, "Sistema de Notificaciones Autogeneradas", ticket.servicio, notificacion.dato, "");
            return Ok("Tarea creada exitosamente.");
        }
        [HttpPatch]
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
            if (tarea.completada == true)
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
                dato = "El soporte " + encargado.nombre + " " + encargado.apellido + " ha rechazado la tarea " + tarea.nombre + ".",
                url_archivo = "No existente",
                notificar_cliente = false,
                remitente = null,
                fecha = DateTime.Now,
                id_ticket = tarea.id_ticket,
            };
            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarRechazoTareaCorreo(duenotarea.email, encargado.nombre + " " + encargado.apellido, tarea.nombre, tarea.id_ticket, ticket.servicio);
            return Ok("Encargado de la tarea eliminado exitosamente.");
        }
        [HttpPatch]
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
            if (tarea.completada == true)
            {
                return BadRequest("Ejecución innecesaria.");
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
                fecha = DateTime.Now,
                id_ticket = tarea.id_ticket,
            };
            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarAsignacionTareaCorreo(usuario.email, tarea.nombre, tarea.prioridad, tarea.id_ticket, ticket.servicio);
            return Ok("Encargado asignado a la tarea exitosamente.");
        }
        [HttpPatch]
        [Route("EditarTarea/{id_tarea}")]
        public IActionResult EditarTarea(int id_tarea, [FromBody] JObject datedit)
        {
            bool completada = datedit.Value<bool>("completada");
            string estado = datedit.Value<string>("estado");

            var tarea = (from t in _Contexto.Tareas
                         where t.id_tarea == id_tarea
                         select t).FirstOrDefault();
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == tarea.id_ticket);
            var encargado = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == ticket.id_encargado);
            var cliente = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == ticket.id_cliente);
            var ejecutor = _Contexto.Usuarios.FirstOrDefault(u => u.id_usuario == tarea.id_encargado);
            if (tarea == null)
            {
                return NotFound("No ha encontrado la tarea");
            }
            if (tarea.completada == true)
            {
                return BadRequest("Imposible editar una tarea ya finalizada.");
            }
            tarea.estado = estado;
            tarea.completada = completada;
            _Contexto.SaveChanges();

            correo enviocorreo = new correo(_configuration);
            if(encargado.id_usuario != ejecutor.id_usuario)
            {
                enviocorreo.EnviarCambioEstadoTareaCorreo(encargado.email, ticket.id_ticket, ejecutor.nombre + " " + ejecutor.apellido, tarea.info, ticket.servicio, estado, completada);
            }
            if (completada)
            {
                enviocorreo.EnviarCambioEstadoTareaCorreo(cliente.email, ticket.id_ticket, ejecutor.nombre + " " + ejecutor.apellido, tarea.info, ticket.servicio, estado, completada);
            }
            return Ok(tarea);
        }
        [HttpPost]
        [Route("Comentar/{id_ticket}")]
        public async Task<IActionResult> Comentar(int id_ticket)
        {
            var form = await Request.ReadFormAsync();
            string dato = form["dato"];
            int? id_remitente = form["id_remitente"].IsNullOrEmpty()? null:int.Parse(form["id_remitente"]);
            bool notificar_cliente = Boolean.Parse(form["notificar_cliente"]);

            // Obtener el archivo (si hay alguno)
            IFormFile archivo = form.Files.FirstOrDefault();

            // Verificar si el archivo fue enviado
            bool archivoEnviado = archivo != null;
            string urlArchivoCargado = "";
            //Si fue enviado, hacer lo de firebase
            if (archivoEnviado)
            {
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
                        AuthTokenAsyncFactory = () => Task.FromResult(tokenUser),
                        ThrowOnCancel = true
                    }).Child("Archivos")
                          .Child(archivo.FileName)
                          .PutAsync(archivoASubir, cancellation.Token);
                    var archivoCargado = await tareaCargarArchivo;

                    urlArchivoCargado = archivoCargado.ToString();

                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            }
            if(urlArchivoCargado == "")
            {
                urlArchivoCargado = "No existente";
            }
            var remitente = (from u in _Contexto.Usuarios
                             where u.id_usuario == id_remitente
                             select u).FirstOrDefault();
            var usuario = (from u in _Contexto.Usuarios
                           join t in _Contexto.Tickets on u.id_usuario equals t.id_cliente
                           where t.id_ticket == id_ticket
                           select u).FirstOrDefault();
            var ticket = _Contexto.Tickets.FirstOrDefault(t => t.id_ticket == id_ticket);
            Notificacion notificacion = new Notificacion
            {
                dato = dato,
                url_archivo = urlArchivoCargado,
                notificar_cliente = notificar_cliente,
                fecha = DateTime.Now,
                remitente = id_remitente,
                id_ticket = id_ticket,
            };

            _Contexto.Notificaciones.Add(notificacion);
            _Contexto.SaveChanges();
            string arc = urlArchivoCargado == "No existente" ? "" : urlArchivoCargado;
            correo enviocorreo = new correo(_configuration);
            enviocorreo.EnviarComentarioTicketCorreo(usuario.email, id_ticket, remitente.nombre + " " + remitente.apellido, ticket.servicio, notificacion.dato, arc);
            return Ok(notificacion);
        }
    } 

}
