using System.Data.SqlClient;
using Dapper;
using TaskManager.api.Models;
using Microsoft.Extensions.Configuration;

namespace TaskManager.api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config["SqlConnectionString"]!;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Users WHERE Email = @email";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { email });
        }
        public async Task CreateUserAsync(string email, string passwordHash)
{
    using var conn = new SqlConnection(_connectionString);
    var sql = "INSERT INTO Users (Email, PasswordHash) VALUES (@email, @passwordHash)";
    await conn.ExecuteAsync(sql, new { email, passwordHash });
}

    }
}
