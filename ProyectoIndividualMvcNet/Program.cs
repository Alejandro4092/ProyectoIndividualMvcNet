using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Middlewares;
using ProyectoIndividualMvcNet.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "GameStore.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Usuarios/ErrorAcceso";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<TiendaJuegosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlTiendaJuego"))
);

builder.Services.AddTransient<IJuegoRepository, JuegoRepository>();
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
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

// Debe ir DESPUÉS de UseSession, UseAuthentication y UseAuthorization
// para que tanto la sesión como los claims estén disponibles
app.UseMiddleware<SessionRecoveryMiddleware>();

app.MapControllerRoute(name: "default", pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();