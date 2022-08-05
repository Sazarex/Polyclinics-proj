using JWTAthorizeTesting.Domain;
using Microsoft.AspNetCore.Mvc;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{
    public class AdminSpecController: Controller
    {
        public AppDbContext db { get; set; }


        public AdminSpecController(AppDbContext _db)
        {
            db = _db;
        }
    }
}
