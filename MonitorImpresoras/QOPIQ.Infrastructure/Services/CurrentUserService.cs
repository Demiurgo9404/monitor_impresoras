using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QOPIQ.Application.Interfaces;

namespace QOPIQ.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Guid? UserId => 
            Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id
                : null;

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    }
}
