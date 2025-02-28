using System;
using System.Threading.Tasks;
using DotNetEnv;
using CookConsumer.Services;

namespace CookConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load environment variables.
            Env.Load();

            // Initialize configuration.
            var config = new AppConfiguration();

            // Build the DbContext options.
            var dbOptions = config.BuildDbContextOptions();

            // Instantiate services.
            var recipeService = new RecipeService(dbOptions);
            var redisService = new RedisService(config.RedisConnectionString);
            var consumer = new MessageConsumer(recipeService, redisService, config);

            // Start consuming messages.
            await consumer.StartConsumingAsync();

            // Prevent the app from exiting.
            await Task.Delay(-1);
        }
    }
}
