using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Usuarios/ErrorAcceso";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
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
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();