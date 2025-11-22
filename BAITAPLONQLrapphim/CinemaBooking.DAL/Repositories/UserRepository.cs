using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(CinemaDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetByNormalizedUsernameAsync(string normalizedUsername)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername && !u.IsDeleted);
    }

    public async Task<User?> GetByNormalizedEmailAsync(string normalizedEmail)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted);
    }

    public async Task<User?> GetUserWithRolesAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    public async Task<List<Role>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && !ur.Role.IsDeleted)
            .Select(ur => ur.Role)
            .ToListAsync();
    }
}

