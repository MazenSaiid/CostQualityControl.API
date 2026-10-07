using System.Security.Claims;
using CostQualityControl.API.Models;

namespace CostQualityControl.API.Services;

public interface IJwtService
{
    string GenerateToken(AppUser user, IList<string> roles);
}
