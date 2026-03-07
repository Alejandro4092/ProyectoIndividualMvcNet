using Microsoft.EntityFrameworkCore;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // El carrito dura 30 min de inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<TiendaJuegosContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("SqlTiendaJuego")));
builder.Services.AddTransient<JuegoRepository>();
builder.Services.AddTransient<UsuarioRepository>(); 
builder.Services.AddScoped<ImageHelper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();