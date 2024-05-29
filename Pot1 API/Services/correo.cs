using Microsoft.Data.SqlClient;

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
                                  "@body = @par_mensaje;";

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
        public void EnviarTicketCorreo(string destinatario, string asunto, int id, string servicio, string descripcion)
        {
            string asunto = ""
            // Crear el cuerpo del correo en formato HTML
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

            // Llamar a la función enviar
            enviar(destinatario, asunto, cuerpo);
        }
    }

}
