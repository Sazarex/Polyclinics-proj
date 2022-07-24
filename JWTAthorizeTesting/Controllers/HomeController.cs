using JWTAthorizeTesting.Domain;
using JWTAthorizeTesting.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAthorizeTesting.Controllers
{
    public class HomeController:Controller
    {
        private AppDbContext db;

        public HomeController(AppDbContext _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Name = User.Identity.Name;
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;


            City city = db.Cities.FirstOrDefault(city => city.Title == "Москва");

            if (city != null)
            {
                HttpContext.Response.Cookies.Append("City", city.Title);
                ViewBag.City = city.Title;
            }
            

            return View(await db.Specializations.ToListAsync());

        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
