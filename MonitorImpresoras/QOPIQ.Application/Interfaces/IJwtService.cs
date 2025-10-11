using System.Security.Claims;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Application.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateJwtToken(User user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        Dictionary<string, string> GetClaimsFromToken(string token);
    }
}
