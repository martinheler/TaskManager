using TaskManager.api.Models;

namespace TaskManager.api.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task CreateUserAsync(string email, string passwordHash);

    }
}
