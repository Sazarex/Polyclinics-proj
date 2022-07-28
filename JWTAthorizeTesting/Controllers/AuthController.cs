using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Security.Cryptography;
using JWTAthorizeTesting.Domain;
using Microsoft.EntityFrameworkCore;
using JWTAthorizeTesting.Entities;
using JWTAthorizeTesting.Models;

namespace JWTAthorizeTesting.Controllers
{
    [Authorize]
    public class AuthController: Controller
    {
        private AppDbContext db;
        public AuthController(AppDbContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Открывается админ панель на странице с городами
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminPanel()
        {
            AdminPanelViewModel adminModel = new AdminPanelViewModel()
            {
                Doctors = db.Doctors.ToList(),
                Specializations = db.Specializations.ToList(),
                Cities = db.Cities.ToList(),
                Experiences = db.Experiences.ToList(),
                Polyclinics = db.Polyclinics.ToList()
            };
            return View(adminModel);

        }


        [Authorize(Roles = "User, Administrator")]
        public string ForUser()
        {
            return "For user";
        }


        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            byte[] source = ASCIIEncoding.ASCII.GetBytes(model.Password);
            byte[] hashedPassword = new MD5CryptoServiceProvider().ComputeHash(source);
            string hashedPasswordString = Convert.ToBase64String(hashedPassword);


            User? user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == hashedPasswordString);

            if (user == null)
            {
                return View(model);
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Login),
                new Claim(ClaimTypes.Role,user.Role.Title)
            };

            var claimIdentity = new ClaimsIdentity(claims, "Cookie");
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);


            await HttpContext.SignInAsync("Cookie", claimPrincipal);

            return Redirect("/Home/Index");
        }


        public async Task<IActionResult> LogOff()
        {
            
            await HttpContext.SignOutAsync("Cookie");
            return Redirect("/Home/Index");
        }

    }
}
