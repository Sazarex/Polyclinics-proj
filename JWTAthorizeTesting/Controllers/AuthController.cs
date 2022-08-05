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

namespace JWTAthorizeTesting.Controllers
{
    [Authorize]
    public class AuthController: Controller
    {
        readonly ICityService _cityService;
        readonly IAuthService _authService;

        private AppDbContext db;
        public AuthController(AppDbContext _db, ICityService cityService, IAuthService authService)
        {
            db = _db;
            _cityService = cityService;
            _authService = authService;
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
        public IActionResult AdminPanel()
        {
            //Объявляю модель города и выбираю в неё все города из бд
            CityViewModel cityModel = new CityViewModel()
            {
                Cities = _cityService.ChooseAll()
            };


            return View(cityModel);

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

            return Redirect("/Home/Index");
        }


        public async Task<IActionResult> LogOff()
        {
            
            await HttpContext.SignOutAsync("Cookie");
            return Redirect("/Home/Index");
        }

    }
}
