using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNormalizedUsernameAsync(string normalizedUsername);
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail);
    Task<User?> GetUserWithRolesAsync(int userId);
    Task<List<Role>> GetUserRolesAsync(int userId);
}

