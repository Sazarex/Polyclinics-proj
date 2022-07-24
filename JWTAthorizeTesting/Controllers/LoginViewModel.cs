using System.ComponentModel.DataAnnotations;

namespace JWTAthorizeTesting.Controllers
{
    public class LoginViewModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
