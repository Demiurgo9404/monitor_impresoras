using System;
using System.Security.Claims;

namespace QOPIQ.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, string role);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
