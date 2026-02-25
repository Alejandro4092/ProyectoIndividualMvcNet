using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaJuegos.Models
{
    [Table("Juegos")]
    public class Juego
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Titulo")]
        public string Titulo { get; set; }

        [Column("Descripcion")]
        public string Descripcion { get; set; }

        [Column("Precio")]
        public decimal Precio { get; set; }

        [Column("Stock")]
        public int Stock { get; set; }

        [Column("Genero")]
        public string Genero { get; set; }

        [Column("Plataforma")]
        public string Plataforma { get; set; }

        [Column("Img")]
        public string Img { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; }

        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }
    }
}