using System.ComponentModel.DataAnnotations;

namespace ApiWebBeachSA.Models
{
    public class Paquete
    {
        [Key]
        [Required]
        public int Id { get; set; }


        [Required]
        public string Nombre { get; set; }


        [Required]
        public decimal Precio { get; set; }


        [Required]
        public int PorcentajeReserva { get; set; }


        [Required]
        public int Mensualidades { get; set; }

    }
}
