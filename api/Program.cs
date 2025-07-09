using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using TaskManager.api.Repositories;
using TaskManager.api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManager.api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices(services =>
                {
                    services.AddScoped<ITaskRepository, TaskRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddSingleton<JwtService>();

                })
                .Build();

            await host.RunAsync();
        }
    }
}
