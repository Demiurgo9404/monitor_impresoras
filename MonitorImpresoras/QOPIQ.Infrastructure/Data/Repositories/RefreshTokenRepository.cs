using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Models;
using System.Linq;

namespace QOPIQ.Infrastructure.Data.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public void Remove(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
