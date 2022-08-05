using JWTAthorizeTesting.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class AdminCittiesController : Controller
    {
        public AppDbContext db { get; set; }

        public AdminCittiesController(AppDbContext _db)
        {
            db = _db;
        }

    }
}
