using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.Core;
using System.Text.Json;
using CookConsumer.Models;
using CookConsumer.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CookConsumer
{
    public class Function
    {
        private readonly RecipeService _recipeService;
        private readonly RedisService _redisService;

        public Function()
        {
            // Load environment variables
            var config = new AppConfiguration();
            _recipeService = new RecipeService(config.BuildDbContextOptions());
            //_redisService = new RedisService(config.RedisConnectionString);
        }

        public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        {
            foreach (var record in sqsEvent.Records)
            {
                try
                {
                    var messageJson = record.Body;
                    Console.WriteLine($" [x] Received message: {messageJson}");

                    var recipeMessage = JsonSerializer.Deserialize<RecipeMessage>(messageJson);
                    if (recipeMessage == null)
                    {
                        Console.WriteLine(" [!] Could not deserialize message.");
                        continue;
                    }

                    var messageType = DetectRoutingKey(recipeMessage);

                    switch (messageType)
                    {
                        case "recipe.create":
                            int recipeId = await _recipeService.ProcessRecipeMessage(recipeMessage);
                            recipeMessage.Recipe.recipeId = recipeId;
                            break;

                        case "recipe.update":
                            await _recipeService.DeleteRecipeMessage(recipeMessage);
                            await _redisService.DeleteRecipeAsync(recipeMessage.Recipe.recipeId ?? throw new Exception("Recipe ID is null"));
                            int updatedId = await _recipeService.ProcessRecipeMessage(recipeMessage);
                            recipeMessage.Recipe.recipeId = updatedId;
                            break;

                        case "recipe.delete":
                            await _recipeService.DeleteRecipeMessage(recipeMessage);
                            break;

                        default:
                            Console.WriteLine(" [!] Unknown message type.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [!] Error processing message: {ex.Message}");
                }
            }
        }

        // Helper method to determine message type
        private string DetectRoutingKey(RecipeMessage msg)
        {
            try{
                return "recipe.create"; // default fallback
            }
            catch(Exception ex){
                Console.WriteLine($" [!] Error detecting message type: {ex.Message}");
                return "recipe.create";
            }

        }
    }
}
