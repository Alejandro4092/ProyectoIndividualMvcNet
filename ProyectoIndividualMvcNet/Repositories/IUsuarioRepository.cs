using ProyectoIndividualMvcNet.Models;

namespace ProyectoIndividualMvcNet.Repositories
{
    public interface IUsuarioRepository
    {
        Task RegisterUserAsync(string nombre, string email, string imagen, string password);
        Task<Usuario> LogInUserAsync(string email, string password);
        Task<Usuario> FindUsuarioAsync(int idUsuario);
        Task UpdatePerfilSinPasswordAsync(
            int idUsuario,
            string nombre,
            string email,
            string imagen
        );
        Task<List<Usuario>> GetUsuariosAsync();
        Task DeleteUsuarioAsync(int idUsuario);
        Task<List<Resena>> GetTodasResenasAsync();
        Task DeleteResenaAsync(int idResena);
        Task<List<UsuarioRankingDto>> GetRankingUsuariosPorComprasAsync(int top = 10);
    }
}
