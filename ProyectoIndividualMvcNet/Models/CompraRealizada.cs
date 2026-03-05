using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("V_COMPRAS_REALIZADAS")]
    public class CompraRealizada
    {
        [Key]
        [Column("IdDetalle")]
        public int IdDetalle { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("IdJuego")]
        public int IdJuego { get; set; }

        [Column("TITULO")]
        public string Titulo { get; set; }

        [Column("IMG")]
        public string Img { get; set; }

        [Column("GENERO")]
        public string Genero { get; set; }

        [Column("PLATAFORMA")]
        public string Plataforma { get; set; }

        [Column("CANTIDAD")]
        public int Cantidad { get; set; }

        [Column("Precio")]  
        public decimal PrecioUnitario { get; set; }

        [Column("Total")]  
        public decimal PrecioTotal { get; set; }

        [Column("Fecha")]  
        public DateTime FechaCompra { get; set; }
    }
}