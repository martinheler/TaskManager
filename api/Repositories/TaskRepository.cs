using System.Data.SqlClient;
using Dapper;
using TaskManager.api.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;


namespace TaskManager.api.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(IConfiguration config)
        {
            _connectionString = config["SqlConnectionString"]!;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Tasks";
            var result = await conn.QueryAsync<TaskItem>(sql);
            return result.ToList();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<TaskItem>("SELECT * FROM Tasks WHERE Id = @id", new { id });
        }

        public async Task CreateAsync(TaskItem task)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"INSERT INTO Tasks (Title, Description, DueDate, Status, CreatedBy, AssignedTo)
                        VALUES (@Title, @Description, @DueDate, @Status, @CreatedBy, @AssignedTo)";
            await conn.ExecuteAsync(sql, task);
        }

        public async Task UpdateAsync(TaskItem task)
        {
            using var conn = new SqlConnection(_connectionString);
            var sql = @"UPDATE Tasks SET Title = @Title, Description = @Description, DueDate = @DueDate,
                        Status = @Status, AssignedTo = @AssignedTo WHERE Id = @Id";
            await conn.ExecuteAsync(sql, task);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM Tasks WHERE Id = @id", new { id });
        }
    }
}
