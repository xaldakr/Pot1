using Microsoft.Data.SqlClient;
using Pot1_API.Models;

namespace Pot1_API.Services
{
    public class correo
    {
        private IConfiguration _configuration;
        public correo(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void enviar(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                string connectionString = _configuration.GetSection("ConnectionStrings").GetSection("Pot1DbConnection").Value;

                string sqlQuery = "exec msdb.dbo.sp_send_dbmail " +
                                  "@profile_name = 'SQLMailPot1', " +
                                  "@recipients = @par_destinatarios, " +
                                  "@subject = @par_asunto, " +
                                  "@body = @par_mensaje, " +
                                  "@body_format = 'HTML';";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@par_destinatarios", destinatario);
                        command.Parameters.AddWithValue("@par_asunto", asunto);
                        command.Parameters.AddWithValue("@par_mensaje", cuerpo);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
        }
        public void EnviarTicketCorreo(string destinatario, int id, string servicio, string descripcion)
        {
            string asunto = "Ticket Creado con id "+ id;
            
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .description {{
                            color: #8D774A;
                            font-style: italic;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Creación de Ticket</div>
                    <div class='content'>
                        <p>Haz creado tu ticket correctamente</p>
                        <p>El id de tu ticket es <span class='highlight'>{id}</span></p>
                        <p>El servicio de revisión es <span class='service'>{servicio}</span> con la descripción de:</p>
                        <p class='description'>{descripcion}</p>
                        <p>Espera más detalles cuando el personal se encargue del ticket</p>
                    </div>
                </body>
                </html>";

            
            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarCambioEstadoTicketCorreo(string destinatario, int id, string servicio, string estado)
        {
            string asunto = "Cambio de estado del ticket " + id;
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .state {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Cambio de estado del Ticket</div>
                    <div class='content'>
                        <p>El ticket que creaste de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span> ha cambiado a estado <span class='state'>{estado}</span></p>
                        <p>Consulta más información dentro de los detalles del ticket del sistema interno</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarCierreTicketCorreo(string destinatario, int id, string servicio)
        {
            string asunto = "Cierre del ticket " + id;
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Cierre del Ticket</div>
                    <div class='content'>
                        <p>El ticket que creaste de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span> se ha cerrado.</p>
                        <p>Consulta más información dentro de los detalles del ticket del sistema interno</p>
                        <p>Esperamos que la solución a tu problema haya resultado ser la más adecuada.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarAsignacionTicketCorreo(string destinatario, int id, string servicio, string prioridad)
        {
            string asunto = "Se te ha asignado el ticket " + id;
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .priority {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Asignación de Ticket</div>
                    <div class='content'>
                        <p>Se te ha asignado la resolución del ticket de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span> de prioridad <span class='priority'>{prioridad}</span>.</p>
                        <p>Consulta más información dentro de los detalles del ticket del sistema interno.</p>
                        <p>Además se espera la pronta resolución de este.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarAsignacionTareaCorreo(string destinatario, string tarea, string prioridad, int id, string servicio)
        {
            string asunto = "Se te ha asignado la tarea " + tarea + " para el ticket " + id;
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .priority {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .task {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Asignación de Tarea</div>
                    <div class='content'>
                        <p>Se te ha asignado la resolución de la tarea <span class='task'>{tarea}</span>.</p>
                        <p>La prioridad de la tarea es <span class='priority'>{prioridad}</span>.</p>
                        <p>Para el ticket de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span>.</p>
                        <p>Consulta más información dentro de los detalles del ticket del sistema interno.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarComentarioTicketCorreo(string destinatario, int id, string NombreRemitente, string servicio, string comentario, string archivo)
        {
            string asunto = NombreRemitente + " ha comentado en el ticket " + id;
            string archivoHtml = string.IsNullOrEmpty(archivo) ? "" : $@"<p>Se ha enviado un archivo adjunto <a href='{archivo}'>aquí</a></p>";

            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .comment {{
                            color: #8D774A;
                            font-style: italic;
                        }}
                        .sender {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .file {{
                            color: #3A5894;
                            text-decoration: underline;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Comentario sobre el ticket {id}</div>
                    <div class='content'>
                        <p><span class='sender'>{NombreRemitente}</span> ha realizado un comentario sobre el ticket de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span>.</p>
                        <p class='comment'>“{comentario}”</p>
                        {archivoHtml}
                        <p>Para conocer más detalles revise el sistema de tickets.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarCambioEstadoTareaCorreo(string destinatario, int id, string remitente, string tarea, string servicio, string estado, bool finalizado)
        {
            string asunto = " Cambio de estado de una tarea del ticket " + id;
            string finalizadoTexto = finalizado ? "La tarea ha finalizado" : "La tarea aún no ha finalizado";

            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .task {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .comment {{
                            color: #8D774A;
                            font-style: italic;
                        }}
                        .sender {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Cambio de estado de una tarea del ticket {id}</div>
                    <div class='content'>
                        <p><span class='sender'>{remitente}</span> ha realizado un cambio en la tarea <span class='task'>{tarea}</span> del ticket de id <span class='highlight'>{id}</span> y servicio <span class='service'>{servicio}</span>.</p>
                        <p>El nuevo estado es:</p>
                        <p class='comment'>“{estado}”</p>
                        <p>{finalizadoTexto}</p>
                        <p>Para conocer más detalles revise el sistema de tickets.</p>
                    </div>
                </body>
                </html>";
            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarBienvenidaUsuarioCorreo(string destinatario, string clase, string nombre, string apellido, string telefono, string contacto, string correo, string contrasena)
        {
            string asunto = "Bienvenido al servicio de Pot1 Tickets";
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .field {{
                            color: #8D774A;
                        }}
                        .contact {{
                            color: #3A5894;
                            text-decoration: underline;
                        }}
                        .login {{
                            color: #3A5894;
                            font-style: italic;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Bienvenido al servicio de Pot1 Tickets</div>
                    <div class='content'>
                        <p>Se te ha creado un usuario de clase <span class='field'>{clase}</span></p>
                        <p>Tus datos son:</p>
                        <p>Nombre: <span class='field'>{nombre}</span></p>
                        <p>Apellido: <span class='field'>{apellido}</span></p>
                        <p>Teléfono: <span class='field'>{telefono}</span></p>
                        <p>Contacto: <span class='field'>{contacto}</span></p>
                        <p>Estos son sus datos de login, por favor guárdelos en un lugar seguro</p>
                        <p>Correo: <span class='contact'>{correo}</span></p>
                        <p>Contraseña: <span class='login'>{contrasena}</span></p>
                        <p>Si quiere hacer un cambio de datos más adelante, contacte a este correo por favor.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarCambioDatosIngresoCorreo(string destinatario, string clase, string nombre, string apellido, string telefono, string contacto, string correo, string contrasena)
        {
            string asunto = "Cambio de datos al ingreso de Pot1 Tickets";
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .field {{
                            color: #8D774A;
                        }}
                        .contact {{
                            color: #3A5894;
                            text-decoration: underline;
                        }}
                        .login {{
                            color: #3A5894;
                            font-style: italic;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>Cambio de datos al ingreso de Pot1 Tickets</div>
                    <div class='content'>
                        <p>Se le han cambiado los datos de ingreso a la plataforma de Pot1 Tickets</p>
                        <p>Tus nuevos datos son:</p>
                        <p>Clase: <span class='field'>{clase}</span></p>
                        <p>Nombre: <span class='field'>{nombre}</span></p>
                        <p>Apellido: <span class='field'>{apellido}</span></p>
                        <p>Teléfono: <span class='field'>{telefono}</span></p>
                        <p>Contacto: <span class='field'>{contacto}</span></p>
                        <p>Estos son sus nuevos datos de login, por favor guárdelos en un lugar seguro</p>
                        <p>Correo: <span class='contact'>{correo}</span></p>
                        <p>Contraseña: <span class='login'>{contrasena}</span></p>
                        <p>Si quiere hacer un cambio de datos más adelante, contacte a este correo por favor.</p>
                    </div>
                </body>
                </html>";

            enviar(destinatario, asunto, cuerpo);
        }
        public void EnviarRechazoTareaCorreo(string destinatario, string remitente, string tarea, int id, string servicio)
        {
            string asunto = $@"{remitente} ha rechazado una tarea";
            string cuerpo = $@"
                <html>
                <head>
                    <style>
                        .header {{
                            background-color: #2E3B4E;
                            color: white;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 24px;
                            font-weight: bold;
                        }}
                        .content {{
                            background-color: #D3DCE5;
                            padding: 20px;
                            font-family: Arial, sans-serif;
                            font-size: 18px;
                            text-align: center;
                            color: black;
                        }}
                        .highlight {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .service {{
                            color: #8D774A;
                        }}
                        .task {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                        .comment {{
                            color: #8D774A;
                            font-style: italic;
                        }}
                        .sender {{
                            color: #8D774A;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>{remitente} ha rechazado una tarea</div>
                    <div class='content'>
                        <p><span class='sender'>{remitente}</span> ha rechazado la realización de la tarea <span class='task'>{tarea}</span> del ticket de id <span class='highlight'>{id}</span> y del servicio de <span class='service'>{servicio}</span></p>
                        <p>Reasigna la tarea para que esta se pueda completar, y si crees que la conducta fue inapropiada habla con el administrador o manda un mensaje a este correo.</p>
                    </div>
                </body>
                </html>";
            enviar(destinatario, asunto, cuerpo);
        }
    }
}
