using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces.Repositories;

namespace QOPIQ.Infrastructure.Data.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        private new readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            // Convert the token string to Guid for comparison
            if (Guid.TryParse(token, out Guid tokenId))
            {
                return await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == tokenId, cancellationToken);
            }
            
            return null;
        }

        public async Task InvalidateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Convert Guid to string for comparison with UserId
            var userIdString = userId.ToString();
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userIdString && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(token);
            }
        }

        public async Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Convert the token string to Guid for comparison
            if (Guid.TryParse(token, out Guid tokenId))
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == tokenId && !rt.IsExpired && !rt.IsRevoked, cancellationToken);

                return refreshToken != null;
            }
            
            return false;
        }
    }
}
