using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CookConsumer
{
    public class AppConfiguration
    {
        public string RabbitMqHost { get; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq";
        public int RabbitMqPort { get; } = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
        public string RabbitMqUser { get; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "quicklycook";
        public string RabbitMqPassword { get; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "changeme";
        public string RedisConnectionString { get; }

        private readonly string _connectionString;

        public AppConfiguration()
        {
            var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? throw new InvalidOperationException("DATABASE_URL environment variable is not set.");

            _connectionString = ParseDatabaseUrl(rawUrl);

            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "";
            RedisConnectionString = string.IsNullOrEmpty(redisPassword)
                ? $"{redisHost}:{redisPort}"
                : $"{redisHost}:{redisPort},password={redisPassword}";
        }

        public DbContextOptions<CookContext> BuildDbContextOptions()
        {
            var dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            var optionsBuilder = new DbContextOptionsBuilder<CookContext>();
            optionsBuilder.UseNpgsql(dataSource);
            return optionsBuilder.Options;
        }

        // Converts a postgresql:// URI to an Npgsql connection string.
        // If already in key=value format, returns as-is.
        private static string ParseDatabaseUrl(string url)
        {
            if (!url.StartsWith("postgresql://") && !url.StartsWith("postgres://"))
                return url;

            var uri = new Uri(url);
            var userParts = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userParts[0]);
            var password = userParts.Length > 1 ? Uri.UnescapeDataString(userParts[1]) : "";
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            var sslMode = "Require";
            var trustCert = true;
            foreach (var param in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = param.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    sslMode = kv[1].ToLower() switch
                    {
                        "disable" => "Disable",
                        "prefer" => "Prefer",
                        "verify-ca" => "VerifyCA",
                        "verify-full" => "VerifyFull",
                        _ => "Require"
                    };
                    trustCert = sslMode == "Require";
                }
            }

            var trust = trustCert ? ";Trust Server Certificate=true" : "";
            return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode}{trust}";
        }
    }
}
