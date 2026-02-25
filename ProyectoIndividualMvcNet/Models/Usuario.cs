using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("IDUSUARIO")]
        public int IdUsuario { get; set; }

        [Column("NOMBRE")]
        public string Nombre { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("IMAGEN")]
        public string Imagen { get; set; }

        [Column("RolId")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public virtual Rol Rol { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }
    }
}