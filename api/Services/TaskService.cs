// Business logic layer
using TaskManager.api.Models;
using TaskManager.api.Repositories;

namespace TaskManager.api.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _repo;

        public TaskService(ITaskRepository repo)
        {
            _repo = repo;
        }

        public Task<List<TaskItem>> GetAllAsync() => _repo.GetAllAsync();

        public Task<TaskItem?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public Task CreateAsync(TaskItem task) => _repo.CreateAsync(task);

        public Task UpdateAsync(TaskItem task) => _repo.UpdateAsync(task);

        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
