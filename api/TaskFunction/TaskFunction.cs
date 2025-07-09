
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using TaskManager.api.Models;
using TaskManager.api.Services;
using TaskManager.api.Repositories;
using System.Text.Json;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Functions
{
    public class TaskFunction
    {
        private readonly ITaskRepository _repo;
        private readonly JwtService _jwt;
        private readonly TaskService _service;

        public TaskFunction(ITaskRepository repo, JwtService jwt, TaskService service)
        {
            _repo = repo;
            _jwt = jwt;
            _service = service;
        }

        private bool IsAuthorized(HttpRequestData req, out ClaimsPrincipal? user, out HttpResponseData? errorResponse)
        {
            errorResponse = null;
            user = null;

            if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                errorResponse.WriteStringAsync("Missing Authorization header").Wait();
                return false;
            }

            var token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "");
            user = _jwt.ValidateToken(token);

            if (user is null)
            {
                errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                errorResponse.WriteStringAsync("Invalid or missing token").Wait();
                return false;
            }

            return true;
        }

        [Function("GetTasks")]
        public async Task<HttpResponseData> GetTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks")] HttpRequestData req,
            FunctionContext context)
        {
            var tasks = await _repo.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(tasks);
            return response;
        }

         [Function("GetTaskById")]
        public async Task<HttpResponseData> GetTaskById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/{id:int}")] HttpRequestData req,
            int id,
            FunctionContext context)
        {
            if (!IsAuthorized(req, out var user, out var unauthorized)) return unauthorized!;
            var task = await _service.GetByIdAsync(id);
            var response = req.CreateResponse(task is null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (task != null) await response.WriteAsJsonAsync(task);
            return response;
        }

        [Function("CreateTask")]
        public async Task<HttpResponseData> CreateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequestData req,
            FunctionContext context)
        {
            if (!IsAuthorized(req, out var user, out var unauthorized)) return unauthorized!;
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var task = JsonSerializer.Deserialize<TaskItem>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (task is null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid task data");
                return badResponse;
            }

            await _repo.CreateAsync(task);
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(task);
            return response;
        }

        [Function("UpdateTask")]
        public async Task<HttpResponseData> UpdateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tasks/{id:int}")] HttpRequestData req,
            int id,
            FunctionContext context)
        {
            if (!IsAuthorized(req, out var user, out var unauthorized)) return unauthorized!;
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedTask = JsonSerializer.Deserialize<TaskItem>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (updatedTask is null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid task data");
                return badResponse;
            }

            updatedTask.Id = id;
            await _repo.UpdateAsync(updatedTask);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updatedTask);
            return response;
        }

        [Function("DeleteTask")]
        public async Task<HttpResponseData> DeleteTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/{id:int}")] HttpRequestData req,
            int id,
            FunctionContext context)
        {
            if (!IsAuthorized(req, out var user, out var unauthorized)) return unauthorized!;
            await _repo.DeleteAsync(id);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
    }
}
