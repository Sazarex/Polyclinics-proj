using JWTAthorizeTesting.Domain;
using Microsoft.AspNetCore.Mvc;

namespace JWTAthorizeTesting.Areas.Admins.Controllers
{
    public class AdminDocsController: Controller
    {
        public AppDbContext db { get; set; }


        public AdminDocsController(AppDbContext _db)
        {
            db = _db;
        }
    }
}
