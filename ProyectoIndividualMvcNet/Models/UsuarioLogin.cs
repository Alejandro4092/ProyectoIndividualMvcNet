using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("V_USUARIOS_LOGIN")] // Nombre exacto de la vista en SQL
    public class UsuarioLogin
    {
        [Key] // Aunque sea una vista, EF necesita una clave para mapear
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Imagen { get; set; }
        public int RolId { get; set; }
        public string Salt { get; set; }
        public byte[] Pass { get; set; }
    }
}