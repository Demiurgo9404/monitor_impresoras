using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces.Repositories;

namespace QOPIQ.Infrastructure.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private new readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(userName));

            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(email));

            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(email));

            return await _context.Users
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("El token de actualización no puede estar vacío", nameof(refreshToken));

            // Convert the refresh token string to Guid for comparison
            if (!Guid.TryParse(refreshToken, out Guid refreshTokenId))
                return null;

            return await _context.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshTokenId), cancellationToken);
        }

        public async Task<IList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            return user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(roleName));

            var roles = await GetRolesAsync(user.Id, cancellationToken);
            return roles.Contains(roleName);
        }

        // Métodos de compatibilidad con código existente
        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(email));

            return !await _context.Users.AnyAsync(
                u => u.Email == email && 
                    (excludeUserId == null || u.Id != excludeUserId.Value), 
                cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(roleName));

            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            var normalizedSearchTerm = searchTerm.Trim().ToLower();
            
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => 
                    u.UserName.ToLower().Contains(normalizedSearchTerm) ||
                    u.Email.ToLower().Contains(normalizedSearchTerm) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(normalizedSearchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(normalizedSearchTerm)))
                .ToListAsync(cancellationToken);
        }
    }
}
