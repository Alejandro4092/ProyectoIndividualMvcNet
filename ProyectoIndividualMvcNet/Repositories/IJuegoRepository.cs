using ProyectoIndividualMvcNet.Models;

namespace ProyectoIndividualMvcNet.Repositories
{
    public interface IJuegoRepository
    {
        Task<List<Juego>> GetJuegosAsync();
        Task<Juego> FindJuegoAsync(int idJuego);
        Task<List<Juego>> GetJuegosPlataformaGeneroAsync(string? texto, string? genero, string? plataforma);
        Task InsertJuegoAsync(string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo);
        Task EliminarJuegoAsync(int id);
        Task EditJuegoAsync(int id, string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo);
        Task ComprarJuegoAsync(int idUsuario, int idJuego, decimal precio);
        Task<List<CompraRealizada>> GetJuegosCompradosAsync(int idUsuario);
        Task ProcesarPedidoCarritoAsync(int idUsuario, List<Juego> carrito);
        Task<List<Resena>> GetResenasJuegoAsync(int idJuego);
        Task InsertarResenaAsync(int idUsuario, int idJuego, int nota, string comentario);
        Task<List<Juego>> GetJuegosMasVendidosAsync();
        Task<List<Resena>> GetAllResenasAsync();
        Task<List<GeneroDistribucionDto>> GetDistribucionJuegosPorGeneroAsync();
        Task<List<VentasMensualesDto>> GetTendenciaVentasMensualesAsync(int meses = 6);
        Task<int> GetNumeroJuegosFiltradosSpAsync(string? texto, string? genero, string? plataforma);
        Task<List<Juego>> GetGrupoJuegosFiltradosSpAsync(int page, int pageSize, string? texto, string? genero, string? plataforma);
    }
}