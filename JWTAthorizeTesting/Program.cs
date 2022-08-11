using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Models.Interfaces;
using JWTAthorizeTesting.Services.Interfaces;
using JWTAthorizeTesting.Services.ServiceClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder();

builder.Services.AddMvc();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddTransient<ICityService, CityService>();
builder.Services.AddTransient<IPolyService, PolyService>();
builder.Services.AddTransient<IDocService, DocService>();
builder.Services.AddTransient<ISpecService, SpecService>();
builder.Services.AddTransient<IAuthService, AuthService>();

builder.Services.AddAuthentication("Cookie")
    .AddCookie("Cookie", options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";

    });

builder.Services.AddAuthorization(options =>
{
    //создаем политику, назначаем администратором текущего пользователя, если он содержит
    // клэйм с типом роли со значением администратор
    options.AddPolicy("Administrator", builder =>
    {
        builder.RequireClaim(ClaimTypes.Role, "Administrator");
    });

    options.AddPolicy("User", builder =>
    {
        builder.RequireClaim(ClaimTypes.Role, "User");
    });

});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".City.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(3600);
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseSession();   // добавляем middleware для работы с сессиями

app.MapAreaControllerRoute(
    name: "main_area",
    areaName: "Users",
    pattern: "Main/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admins",
    pattern: "AdminPanel/{controller}/{action}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Users}/{controller=Home}/{action=Index}/{id?}");

app.Run();
