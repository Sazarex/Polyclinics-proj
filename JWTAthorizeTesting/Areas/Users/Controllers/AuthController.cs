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
using JWTAthorizeTesting.Models.Interfaces;
using JWTAthorizeTesting.Services.Interfaces;

namespace JWTAthorizeTesting.Areas.Users.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        readonly IAuthService _authService;

        private AppDbContext db;
        public AuthController(AppDbContext _db, IAuthService authService)
        {
            db = _db;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
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
        public async Task<IActionResult> Login(AuthModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string scheme = "";
            ClaimsPrincipal claimsPrincipal;
            if (!_authService.Authorize(model, out scheme, out claimsPrincipal))
            {
                return NotFound("Ошибка авторизации.");
            }
            await HttpContext.SignInAsync(scheme, claimsPrincipal);
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync("Cookie");
            return RedirectToAction("Index", "Home");
        }

    }
}
