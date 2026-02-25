using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProyectoIndividualMvcNet.Repositories
{
    #region procedures
    //    CREATE OR ALTER PROCEDURE sp_BuscarJuegos
    //    @Texto VARCHAR(100) = '', -- Valor por defecto vacío
    //    @Genero VARCHAR(50) = '',
    //    @Plataforma VARCHAR(50) = ''
    //AS
    //BEGIN
    //    SELECT* FROM Juegos
    //    WHERE
    //        (Titulo LIKE '%' + @Texto + '%')
    //        AND(@Genero = '' OR Genero = @Genero)
    //        AND(@Plataforma = '' OR Plataforma LIKE '%' + @Plataforma + '%')
    //        AND Activo = 1
    //    ORDER BY Titulo;
    //END
    //    USE TiendaJuegos;
    //    GO

    //-- PROCEDURE PARA CREAR(INSERTAR)
    //CREATE OR ALTER PROCEDURE sp_InsertarJuego
    //    @Titulo VARCHAR(100),
    //    @Descripcion VARCHAR(MAX),
    //    @Precio DECIMAL(18,2),
    //    @Stock INT,
    //    @Genero VARCHAR(50),
    //    @Plataforma VARCHAR(50),
    //    @Img VARCHAR(MAX),
    //    @Activo BIT
    //AS
    //BEGIN
    //    DECLARE @NuevoId INT;
    //    SELECT @NuevoId = ISNULL(MAX(Id), 0) + 1 FROM Juegos;

    //    INSERT INTO Juegos(Id, Titulo, Descripcion, Precio, Stock, Genero, Plataforma, Img, Activo)
    //    VALUES(@NuevoId, @Titulo, @Descripcion, @Precio, @Stock, @Genero, @Plataforma, @Img, @Activo);
    //    END
    //    GO

    //-- PROCEDURE PARA BORRAR(DELETE)
    //CREATE OR ALTER PROCEDURE sp_EliminarJuego
    //    @Id INT
    //AS
    //BEGIN
    //    DELETE FROM Juegos WHERE Id = @Id;
    //    END
    //    GO
//    CREATE OR ALTER PROCEDURE sp_EditarJuego
//        @Id INT,
//    @Titulo VARCHAR(100),
//    @Descripcion VARCHAR(MAX),
//    @Precio DECIMAL(18,2),
//    @Stock INT,
//    @Genero VARCHAR(50),
//    @Plataforma VARCHAR(50),
//    @Img VARCHAR(MAX),
//    @Activo BIT
//AS
//BEGIN
//    UPDATE Juegos
//    SET Titulo = @Titulo,
//        Descripcion = @Descripcion,
//        Precio = @Precio,
//        Stock = @Stock,
//        Genero = @Genero,
//        Plataforma = @Plataforma,
//        Img = @Img,
//        Activo = @Activo
//    WHERE Id = @Id;
//    END
//    GO
    #endregion
    public class JuegoRepository
    {
        private TiendaJuegosContext context;
        public JuegoRepository(TiendaJuegosContext context)
        {
            this.context = context;
        }
        public async Task<List<Juego>> GetJuegosAsync()
        {
           
            var consulta = from datos in this.context.Juegos
                           where datos.Activo == true
                           orderby datos.Titulo ascending
                           select datos;

            return await consulta.ToListAsync();
        }
        public async Task<Juego> FindJuegoAsync(int idJuego)
        {
            var consulta = from datos in this.context.Juegos where datos.Id == idJuego select datos;
            return await consulta.FirstOrDefaultAsync();
        }
        public async Task<List<Juego>> GetJuegosPlataformaGeneroAsync(string? texto, string? genero, string? plataforma)
        {
            texto ??= "";
            genero ??= "";
            plataforma ??= "";

            string sql = "sp_BuscarJuegos @Texto, @Genero, @Plataforma";

            SqlParameter paramTexto = new SqlParameter("@Texto", texto);
            SqlParameter paramGenero = new SqlParameter("@Genero", genero);
            SqlParameter paramPlataforma = new SqlParameter("@Plataforma", plataforma);

            var consulta = await this.context.Juegos
                 .FromSqlRaw(sql, paramTexto, paramGenero, paramPlataforma)
                 .ToListAsync();

            return consulta;
        }
        public async Task InsertarJuegoAsync(string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo)
        {
            // El SQL con los nombres de los parámetros del Procedure
            string sql = "sp_InsertarJuego @titulo, @descripcion, @precio, @stock, @genero, @plataforma, @img, @activo";

            SqlParameter pamTit = new SqlParameter("@titulo", titulo);
            SqlParameter pamDesc = new SqlParameter("@descripcion",descripcion);
            SqlParameter pamPre = new SqlParameter("@precio", precio);
            SqlParameter pamStock = new SqlParameter("@stock", stock);
            SqlParameter pamGen = new SqlParameter("@genero", genero);
            SqlParameter pamPlat = new SqlParameter("@plataforma",plataforma);
            SqlParameter pamImg = new SqlParameter("@img",img);
            SqlParameter pamAct = new SqlParameter("@activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamTit, pamDesc, pamPre, pamStock, pamGen, pamPlat, pamImg, pamAct);
        }
        public async Task EliminarJuegoAsync(int id)
        {
            string sql = "sp_EliminarJuego @id";

            SqlParameter pamId = new SqlParameter("@id", id);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamId);
        }
        public async Task EditarJuegoAsync(int id, string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo)
        {
            string sql = "sp_EditarJuego @id, @titulo, @descripcion, @precio, @stock, @genero, @plataforma, @img, @activo";

            SqlParameter pamId = new SqlParameter("@id", id);
            SqlParameter pamTit = new SqlParameter("@titulo", titulo);
            SqlParameter pamDesc = new SqlParameter("@descripcion", descripcion);
            SqlParameter pamPre = new SqlParameter("@precio", precio);
            SqlParameter pamStock = new SqlParameter("@stock", stock);
            SqlParameter pamGen = new SqlParameter("@genero", genero);
            SqlParameter pamPlat = new SqlParameter("@plataforma", plataforma);
            SqlParameter pamImg = new SqlParameter("@img", img);
            SqlParameter pamAct = new SqlParameter("@activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamId, pamTit, pamDesc, pamPre, pamStock, pamGen, pamPlat, pamImg, pamAct);
        }
    }
}
