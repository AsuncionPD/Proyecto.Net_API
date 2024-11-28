using Microsoft.AspNetCore.Mvc;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Service;
using System.Text.RegularExpressions;
using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;


namespace ApiWebBeachSA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservacionesController : Controller
    {
        private readonly DbContextHotel _context = null;

        public ReservacionesController(DbContextHotel pContext)
        {
            _context = pContext;

        }

        [HttpGet("Listado")]
        public List<Reservacion> Listado()
        {
            List<Reservacion> lista = null;
            lista = _context.Reservaciones.ToList();
            return lista;
        }

        /// <summary>
        /// Método encargado de agregar una reservacion y enviar un PDF con los datos de la reservacion al correo electrónico del usuario
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("Agregar")]
        public async Task<string> Agregar(Reservacion temp)
        {
            string mensaje = "Debe ingresar la informacion completa de la reservacion ";

            if (temp == null)
            {
                return mensaje;
            }
            else
            {
                try
                {
                    temp.ReservacionID = 0;
                    _context.Reservaciones.Add(temp);
                    await _context.SaveChangesAsync();

                    var cliente = _context.Clientes.FirstOrDefault(x => x.Cedula == temp.ClienteID);
                    if (cliente != null && !string.IsNullOrEmpty(cliente.Email))
                    {
                        byte[] pdfAdjunto = GenerarPDF(temp, cliente);

                        string cuerpoCorreo = GenerarCuerpoCorreo(temp, cliente);
                        EnviarCorreo(cliente.Email, "Confirmación de Reservación", cuerpoCorreo, pdfAdjunto, "Reservacion.pdf");

                        mensaje = $"Reservación almacenada con éxito y correo enviado a {cliente.Email} con archivo PDF adjunto.";
                    }
                    else
                    {
                        mensaje = "Reservación almacenada, pero no se encontró un correo válido para el cliente.";
                    }
                }
                catch (Exception ex)
                {
                    var aux= _context.Clientes.FirstOrDefault(x=>x.Cedula==temp.ClienteID);
                    mensaje = $"Error al agregar la reservacion del cliente {aux.Nombre} detalle {ex.InnerException}";
                }
                return mensaje;
            }
        }

        /// <summary>
        /// Método para eliminar una reservacion por medio del id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int id)
        {
            string mensaje = $"Reservacion no eliminada, el id {id} no existe";

            Reservacion temp = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == id);

            if (temp != null)
            {
                _context.Reservaciones.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"La reservacion con la id {temp.ReservacionID} fue eliminado correctamente..";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de editar la información de una reservacion
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("Editar")]
        public async Task<string> Editar(Reservacion temp)
        {
            var aux = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == temp.ReservacionID);

            string mensaje = $"La reservacion con la id {temp.ReservacionID} no existe";

            if (aux != null)
            {
                aux.PaqueteID = temp.PaqueteID;
                aux.FechaReservacion = temp.FechaReservacion;
                aux.Noches = temp.Noches;
                aux.Personas = temp.Personas;
                aux.TotalSinDescuento = temp.TotalSinDescuento;
                aux.DescuentoPorcentaje = temp.DescuentoPorcentaje;
                aux.TotalConDescuento = temp.TotalConDescuento;
                aux.FormaPago = temp.FormaPago;
                aux.NumeroCheque = temp.NumeroCheque;
                aux.Banco = temp.Banco;
                aux.Prima = temp.Prima;
                aux.RestoEnMensualidades = temp.RestoEnMensualidades;
                aux.TipoCambio = temp.TipoCambio;
                aux.TotalUSD = temp.TotalUSD;


                _context.Reservaciones.Update(aux);
                await _context.SaveChangesAsync();
                mensaje = $"La reservacion con la id {aux.ReservacionID} se actualizado correctamente";
            }
           
            return mensaje;
        }

        /// <summary>
        /// Método encargado de consultar una reservacion por medio del Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Buscar")]
        public Reservacion Buscar(int id)
        {
            Reservacion temp = null;
            temp = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == id);
            return temp == null ? new Reservacion() { Banco = "No existe" } : temp;
        }

        /// <summary>
        /// Método encargado de consultar las reservaciones de un usuario por medio de su id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("ReservacionesCliente")]
        public List<Reservacion> ClienteReservaciones(int id)
        {
            var reservacionesCliente = _context.Reservaciones.Where(x => x.ClienteID == id).ToList();
            return reservacionesCliente == null ? null : reservacionesCliente;
        }


        [HttpGet("Count")]
        public int Count()
        {
            return _context.Reservaciones.Count();
        }

        /// <summary>
        /// Método para crear el cuerpo del correo electrónico
        /// </summary>
        /// <param name="reservacion"></param>
        /// <param name="cliente"></param>
        /// <returns></returns>
        private string GenerarCuerpoCorreo(Reservacion reservacion, Cliente cliente)
        {
            return $@"<h3>Confirmación de Reservación</h3>";
        }

        /// <summary>
        /// Método que configura el envío del correo electrónico
        /// </summary>
        /// <param name="emailDestino"></param>
        /// <param name="asunto"></param>
        /// <param name="cuerpo"></param>
        /// <param name="pdfAdjunto"></param>
        /// <param name="nombreArchivo"></param>
        private void EnviarCorreo(string emailDestino, string asunto, string cuerpo, byte[] pdfAdjunto, string nombreArchivo)
        {
            string emailOrigen = "beachhotel1995@gmail.com";
            string smtpHost = "smtp.gmail.com";
            string smtpPassword = "carzjknskvqnuoec";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailOrigen);
                mail.To.Add(emailDestino);
                mail.Subject = asunto;
                mail.Body = cuerpo;
                mail.IsBodyHtml = true;

                if (pdfAdjunto != null && !string.IsNullOrEmpty(nombreArchivo))
                {
                    Attachment attachment = new Attachment(new MemoryStream(pdfAdjunto), nombreArchivo);
                    mail.Attachments.Add(attachment);
                }

                using (SmtpClient smtp = new SmtpClient(smtpHost, 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(emailOrigen, smtpPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        /// <summary>
        /// Método que crea el PDF con los datos de la reservación
        /// </summary>
        /// <param name="reservacion"></param>
        /// <param name="cliente"></param>
        /// <returns></returns>
        private byte[] GenerarPDF(Reservacion reservacion, Cliente cliente)
        {
            using (var stream = new MemoryStream())
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Confirmación de Reservación";

                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont titleFont = new XFont("Verdana", 20, XFontStyle.Bold);
                XFont bodyFont = new XFont("Verdana", 12, XFontStyle.Regular);

                //decimal tipoCambio = ObtenerTipoCambio();

                //decimal totalSinDescuentoDolares = reservacion.TotalSinDescuento / tipoCambio;
                //decimal totalConDescuentoDolares = reservacion.TotalConDescuento / tipoCambio;

                gfx.DrawString("Confirmación de Reservación", titleFont, XBrushes.Black, new XPoint(50, 50));
                gfx.DrawString($"Cliente: {cliente.Nombre}", bodyFont, XBrushes.Black, new XPoint(50, 100));
                gfx.DrawString($"Cédula: {cliente.Cedula}", bodyFont, XBrushes.Black, new XPoint(50, 130));
                gfx.DrawString($"Correo: {cliente.Email}", bodyFont, XBrushes.Black, new XPoint(50, 160));
                gfx.DrawString($"Fecha de Reservación: {reservacion.FechaReservacion:dd/MM/yyyy}", bodyFont, XBrushes.Black, new XPoint(50, 220));
                gfx.DrawString($"Número de noches: {reservacion.Noches}", bodyFont, XBrushes.Black, new XPoint(50, 250));
                gfx.DrawString($"Número de personas: {reservacion.Personas}", bodyFont, XBrushes.Black, new XPoint(50, 280));
                gfx.DrawString($"Descuento aplicado: {reservacion.DescuentoPorcentaje}%", bodyFont, XBrushes.Black, new XPoint(50, 310));
                gfx.DrawString($"Forma de pago: {reservacion.FormaPago}", bodyFont, XBrushes.Black, new XPoint(50, 340));
                gfx.DrawString($"Total sin descuento en colones: {"₡"+reservacion.TotalSinDescuento}", bodyFont, XBrushes.Black, new XPoint(50, 370));
                gfx.DrawString($"Total sin descuento en dólares: {"$"+reservacion.TotalUSD}", bodyFont, XBrushes.Black, new XPoint(50, 400));
                //gfx.DrawString($"Total con descuento: {reservacion.TotalConDescuento:C} CRC / {totalConDescuentoDolares:C} USD", bodyFont, XBrushes.Black, new XPoint(50, 370));
                gfx.DrawString($"Total con descuento en colones: {"₡"+reservacion.TotalConDescuento}", bodyFont, XBrushes.Black, new XPoint(50, 430));

                if (reservacion.FormaPago == "Cheque")
                {
                    gfx.DrawString($"Número de cheque: {reservacion.NumeroCheque}", bodyFont, XBrushes.Black, new XPoint(50, 460));
                    gfx.DrawString($"Banco: {reservacion.Banco}", bodyFont, XBrushes.Black, new XPoint(50, 490));
                }

                document.Save(stream, false);

                return stream.ToArray();
            }
        }


    }//Cierre del controlador
}//Cierre del namespace
