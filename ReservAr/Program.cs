using ReservAr.Models.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<RepositorioCategoria>();
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioNegocio>();
builder.Services.AddScoped<RepositorioRecurso>();
builder.Services.AddScoped<RepositorioHorarioAtencion>();
builder.Services.AddScoped<RepositorioRol>();
builder.Services.AddScoped<RepositorioTarifa>();
builder.Services.AddScoped<RepositorioReserva>();


builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
      options.LoginPath = "/Account/Login";
      options.LogoutPath = "/Account/Logout";
      options.AccessDeniedPath = "/Account/AccesoDenegado";
      options.ExpireTimeSpan = TimeSpan.FromHours(8);
      options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["TokenAuthentication:Issuer"],
        ValidAudience = builder.Configuration["TokenAuthentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(builder.Configuration["TokenAuthentication:SecretKey"]!)),
        RoleClaimType = ClaimTypes.Role
      };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
