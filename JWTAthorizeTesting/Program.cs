using JWTAthorizeTesting.Domain;
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


builder.Services.AddAuthentication("Cookie")
    .AddCookie("Cookie", options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";

    });


builder.Services.AddAuthorization(options =>
{
    //������� ��������, ��������� ��������������� �������� ������������, ���� �� ��������
    // ����� � ����� ���� �� ��������� �������������
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
app.UseSession();   // ��������� middleware ��� ������ � ��������
app.MapDefaultControllerRoute();


app.Run();
