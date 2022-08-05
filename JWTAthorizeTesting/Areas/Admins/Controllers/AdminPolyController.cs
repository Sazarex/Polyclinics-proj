using JWTAthorizeTesting.Domain;
using Microsoft.AspNetCore.Mvc;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{
    public class AdminPolyController: Controller
    {
        public AppDbContext db { get; set; }


        public AdminPolyController(AppDbContext _db)
        {
            db = _db;
        }
    }
}
