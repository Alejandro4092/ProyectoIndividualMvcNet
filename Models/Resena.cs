using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaJuegos.Models
{
    [Table("Resenas")]
    public class Resena
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("JuegoId")]
        public int JuegoId { get; set; }

        [Column("Nota")]
        public int Nota { get; set; }

        [Column("Comentario")]
        public string Comentario { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("JuegoId")]
        public virtual Juego Juego { get; set; }
    }
}