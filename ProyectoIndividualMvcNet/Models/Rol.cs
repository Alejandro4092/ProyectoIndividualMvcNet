using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndividualMvcNet.Models
{
    [Table("Roles")]
    public class Rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Sin IDENTITY
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
