using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CookConsumer.Models;

namespace CookConsumer.Services
{
    public class MessageConsumer
    {
        private readonly RecipeService _recipeService;
        private readonly RedisService _redisService;
        private readonly AppConfiguration _config;

        public MessageConsumer(RecipeService recipeService, RedisService redisService, AppConfiguration config)
        {
            _recipeService = recipeService;
            _redisService = redisService;
            _config = config;
        }

        public async Task StartConsumingAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "user",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "changeme"
            };            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare a topic exchange for recipes..
            string exchangeName = "recipe_exchange";
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: "topic",
                durable: false,
                autoDelete: false,
                arguments: null
            );

            // Declare a queue and bind it to the exchange using a wildcard routing key.
            var queueName = await channel.QueueDeclareAsync();
            await channel.QueueBindAsync(queue: queueName,
                              exchange: exchangeName,
                              routingKey: "recipe.*");

            Console.WriteLine(" [*] Waiting for messages from RabbitMQ...");
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                Console.WriteLine(" [x] Received message: {0} with routing key: {1}", messageJson, routingKey);

                try
                {
                    switch (routingKey)
                    {
                        case "recipe.create":
                            {
                                // Deserialize and process creation
                                var recipeMessage = JsonSerializer.Deserialize<RecipeMessage>(messageJson);
                                if (recipeMessage == null)
                                {
                                    Console.WriteLine(" [!] Could not deserialize create message.");
                                    break;
                                }
                                int recipeId = await _recipeService.ProcessRecipeMessage(recipeMessage);
                                recipeMessage.Recipe.recipeId = recipeId;
                                await _redisService.StoreRecipeAsync(recipeMessage.Recipe);
                            }
                            break;
                        case "recipe.update":
                            {
                                //Deserialize and process update.
                                var recipeMessage = JsonSerializer.Deserialize<RecipeMessage>(messageJson);
                                if (recipeMessage == null)
                                {
                                    Console.WriteLine(" [!] Could not deserialize update message.");
                                    break;
                                }
                                // First Delete the recipe.
                                await _recipeService.DeleteRecipeMessage(recipeMessage);
                                await _redisService.DeleteRecipeAsync(recipeMessage.Recipe.recipeId ?? throw new Exception("Recipe ID is null"));
                                // Then create the recipe.
                                int recipeId = await _recipeService.ProcessRecipeMessage(recipeMessage);
                                recipeMessage.Recipe.recipeId = recipeId;
                                await _redisService.StoreRecipeAsync(recipeMessage.Recipe);
                            }
                            break;
                        case "recipe.delete":
                            {
                                // Deserialize and process deletion.
                                var recipeMessage = JsonSerializer.Deserialize<RecipeMessage>(messageJson);
                                if (recipeMessage == null)
                                {
                                    Console.WriteLine(" [!] Could not deserialize delete message.");
                                    break;
                                }
                                await _recipeService.DeleteRecipeMessage(recipeMessage);
                                await _redisService.DeleteRecipeAsync(recipeMessage.Recipe.recipeId ?? throw new Exception("Recipe ID is null"));
                            }
                            break;
                        default:
                            Console.WriteLine(" [!] Unknown routing key: " + routingKey);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" [!] Error processing message: " + ex.Message);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );

            // Keep the consumer running.
            await Task.Delay(Timeout.Infinite);
        }
    }
}
