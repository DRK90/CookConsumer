using Microsoft.EntityFrameworkCore;
using CookConsumer.Models;

namespace CookConsumer
{
    public class AppConfiguration
    {
        public string EnvironmentName { get; set; } = "";
        public string Server { get; set; } = "";
        public string Database { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
        public string RabbitMqHost { get; set; } = "localhost";
        public string RedisConnectionString { get; set; } = "localhost:6379";

        public AppConfiguration()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (EnvironmentName == "Development")
            {
                Server = Environment.GetEnvironmentVariable("DEV_SERVER") ?? throw new Exception("DEV_SERVER is not set");
                Database = Environment.GetEnvironmentVariable("DEV_DATABASE") ?? throw new Exception("DEV_DATABASE is not set");
                UserId = Environment.GetEnvironmentVariable("DEV_USERID") ?? throw new Exception("DEV_USERID is not set");
                Password = Environment.GetEnvironmentVariable("DEV_PASSWORD") ?? throw new Exception("DEV_PASSWORD is not set");
            }
            else if (EnvironmentName == "Debug")
            {
                Server = Environment.GetEnvironmentVariable("DEBUG_SERVER") ?? throw new Exception("DEBUG_SERVER is not set");
                Database = Environment.GetEnvironmentVariable("DEBUG_DATABASE") ?? throw new Exception("DEBUG_DATABASE is not set");
                UserId = Environment.GetEnvironmentVariable("DEBUG_USERID") ?? throw new Exception("DEBUG_USERID is not set");
                Password = Environment.GetEnvironmentVariable("DEBUG_PASSWORD") ?? throw new Exception("DEBUG_PASSWORD is not set");
            }
            else if (EnvironmentName == "Staging")
            {
                Server = Environment.GetEnvironmentVariable("DEV_SERVER") ?? throw new Exception("DEV_SERVER is not set");
                Database = Environment.GetEnvironmentVariable("DEV_DATABASE") ?? throw new Exception("DEV_DATABASE is not set");
                UserId = Environment.GetEnvironmentVariable("DEV_USERID") ?? throw new Exception("DEV_USERID is not set");
                Password = Environment.GetEnvironmentVariable("DEV_PASSWORD") ?? throw new Exception("DEV_PASSWORD is not set");
            }
        }

        public DbContextOptions<CookContext> BuildDbContextOptions()
        {
            var connectionString = $"Server={Server};Database={Database};User Id={UserId};Password={Password};";
            var optionsBuilder = new DbContextOptionsBuilder<CookContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            return optionsBuilder.Options;
        }
    }
}
