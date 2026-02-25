using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaJuegos.Models
{
    [Table("PedidoDetalle")]
    public class PedidoDetalle
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PedidoId")]
        public int PedidoId { get; set; }

        [Column("JuegoId")]
        public int JuegoId { get; set; }

        [Column("Cant")]
        public int Cant { get; set; }

        [Column("Precio")]
        public decimal Precio { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        [ForeignKey("JuegoId")]
        public virtual Juego Juego { get; set; }
    }
}