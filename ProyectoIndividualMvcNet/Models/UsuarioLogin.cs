using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("V_USUARIOS_LOGIN")]
    public class UsuarioLogin
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

        [Column("SALT")]
        public string Salt { get; set; }

        [Column("PASS")]
        public byte[] Pass { get; set; }

        // Nueva columna para que el profesor vea la clave en texto plano
        [Column("PASSWORD")]
        public string Password { get; set; }
    }
}