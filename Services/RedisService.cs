using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using CookConsumer.Models;

namespace CookConsumer.Services
{
    public class RedisService
    {
        private readonly string _connectionString;

        public RedisService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task StoreRecipeAsync(RecipeReturnDto recipe)
        {
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redisPass = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "5pdXlgnIhX";

            var options = ConfigurationOptions.Parse($"{redisHost}:{redisPort}");
            options.Password = redisPass;

            var redis = await ConnectionMultiplexer.ConnectAsync(options);
            var db = redis.GetDatabase();
            string recipeJson = JsonSerializer.Serialize(recipe);
            // Push to the left of the list.
            db.ListLeftPush("recentRecipes", recipeJson);
            // Trim the list to keep only a fixed number of recipes.
            db.ListTrim("recentRecipes", 0, 5000);
        }

        public async Task DeleteRecipeAsync(int recipeId)
        {
            using var redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            var db = redis.GetDatabase();
            await db.KeyDeleteAsync($"recipe:{recipeId}");
        }
    }
}
