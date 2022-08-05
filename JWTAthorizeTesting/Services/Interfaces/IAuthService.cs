using JWTAthorizeTesting.Models;
using System.Security.Claims;

namespace JWTAthorizeTesting.Services.Interfaces
{
    public interface IAuthService
    {
        public bool Authorize(AuthModel authModel, out string scheme, out ClaimsPrincipal claimsPrincipal);
    }
}
