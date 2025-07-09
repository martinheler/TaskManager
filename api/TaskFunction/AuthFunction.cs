
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.api.Services;
using TaskManager.api.Models;
using TaskManager.api.Repositories;
using System;

namespace TaskManager.api.Functions
{
    public class AuthFunction
    {
        private readonly JwtService _jwt;
        private readonly IUserRepository _userRepo;

        public AuthFunction(JwtService jwt, IUserRepository userRepo)
        {
            _jwt = jwt;
            _userRepo = userRepo;
        }

        [Function("Login")]
        public async Task<HttpResponseData> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var login = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (login is null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid email or password");
                return bad;
            }

            var user = await _userRepo.GetByEmailAsync(login.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                var token = _jwt.GenerateToken(user.Id.ToString(), user.Email);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { token });
                return response;
            }

            var fail = req.CreateResponse(HttpStatusCode.Unauthorized);
            await fail.WriteStringAsync("Invalid credentials");
            return fail;
        }

        [Function("Register")]
        public async Task<HttpResponseData> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var register = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (register is null || string.IsNullOrWhiteSpace(register.Email) || string.IsNullOrWhiteSpace(register.Password))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid email or password");
                return bad;
            }

            var existingUser = await _userRepo.GetByEmailAsync(register.Email);
            if (existingUser is not null)
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync("Email already registered");
                return conflict;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(register.Password);
            await _userRepo.CreateUserAsync(register.Email, passwordHash);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("User registered");
            return response;
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
