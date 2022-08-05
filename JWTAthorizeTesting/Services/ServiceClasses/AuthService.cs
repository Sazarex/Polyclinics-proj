using JWTAthorizeTesting.Models;
using JWTAthorizeTesting.Services.Interfaces;
using System.Text;
using System.Security.Cryptography;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace JWTAthorizeTesting.Services.ServiceClasses
{
    public class AuthService : IAuthService
    {
        public bool Authorize(AuthModel authModel, out string scheme, out ClaimsPrincipal claimsPrincipal)
        {
            if (authModel == null)
            {
                scheme = null;
                claimsPrincipal = null;
                return false;
            }

            using (var db = new AppDbContext())
            {

                byte[] source = ASCIIEncoding.ASCII.GetBytes(authModel.Password);
                byte[] hashedPassword = new MD5CryptoServiceProvider().ComputeHash(source);
                string hashedPasswordString = Convert.ToBase64String(hashedPassword);


                User? user = db.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == authModel.Login && u.Password == hashedPasswordString);

                if (user == null)
                {
                    scheme = null;
                    claimsPrincipal = null;
                    return false;
                }


                var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Name,user.Login),
                new Claim(ClaimTypes.Role,user.Role.Title)
                };

                var claimIdentity = new ClaimsIdentity(claims, "Cookie");
                claimsPrincipal = new ClaimsPrincipal(claimIdentity);


                scheme = "Cookie";
                return true; 
            }
        }
    }
}
