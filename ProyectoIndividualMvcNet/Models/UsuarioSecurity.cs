using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("Usuarios_Security")]
    public class UsuarioSecurity
    {
        [Key]
        [Column("IDUSUARIO")]
        public int IdUsuario { get; set; }

        [Column("SALT")]
        public string Salt { get; set; }

        [Column("PASS")]
        public byte[] Pass { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
    }
}