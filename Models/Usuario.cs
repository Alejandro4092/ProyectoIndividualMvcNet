using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaJuegos.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Usuario")]
        public string Usuario1 { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("Password")]
        public string Password { get; set; }

        [Column("Rol")]
        public string Rol { get; set; }

        [Column("FechaAlta")]
        public DateTime FechaAlta { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }
    }
}