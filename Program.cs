﻿using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetEnv;
using CookConsumer.Models;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using CookConsumer.Helpers;
namespace CookConsumer{
    class Program{

        static async Task Main(string[] args){
            Env.Load();
            var connectionString = "";

            Console.WriteLine("ASPNETCORE_ENVIRONMENT 2: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var server = Environment.GetEnvironmentVariable("DEV_SERVER");
                var database = Environment.GetEnvironmentVariable("DEV_DATABASE");
                var username = Environment.GetEnvironmentVariable("DEV_USERID");
                var password = Environment.GetEnvironmentVariable("DEV_PASSWORD");
                connectionString = $"Server={server};Database={database};User Id={username};Password={password};";
            } else if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Debug")
            {
                var server = Environment.GetEnvironmentVariable("DEBUG_SERVER");
                var database = Environment.GetEnvironmentVariable("DEBUG_DATABASE");
                var username = Environment.GetEnvironmentVariable("DEBUG_USERID");
                var password = Environment.GetEnvironmentVariable("DEBUG_PASSWORD");
                connectionString = $"Server={server};Database={database};User Id={username};Password={password};";
            }else if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Staging")
            {
                var server = Environment.GetEnvironmentVariable("DEV_SERVER");
                var database = Environment.GetEnvironmentVariable("DEV_DATABASE");
                var username = Environment.GetEnvironmentVariable("DEV_USERID");
                var password = Environment.GetEnvironmentVariable("DEV_PASSWORD");
                connectionString = $"Server={server};Database={database};User Id={username};Password={password};";
            }
            Console.WriteLine("Connection String: " + connectionString);

            var optionsBuilder = new DbContextOptionsBuilder<CookContext>();
            
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                
                await channel.QueueDeclareAsync(
                    queue: "recipe_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

            Console.WriteLine(" [*] Waiting for messages from RabbitMQ...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received message: {0}", messageJson);

                try
                {
                    // Deserialize the incoming message.
                    var recipeMessage = JsonSerializer.Deserialize<RecipeMessage>(messageJson);
                    if (recipeMessage == null)
                    {
                        Console.WriteLine(" [!] Could not deserialize message.");
                        return;
                    }

                    // Process the recipeMessage and write to the database.
                    var recipeId = -1;
                    try{
                        recipeId = await ProcessRecipeMessage(recipeMessage, optionsBuilder.Options);
                        recipeMessage.Recipe.recipeId = recipeId;
                    }
                    catch (Exception ex){
                        Console.WriteLine(" [!] Error processing message: " + ex.ToString());
                    }

                    // write to redis.  Even if it fails the db write.
                    try{
                        // Connect to Redis.
                        using (var redis = ConnectionMultiplexer.Connect("localhost:6379"))
                        {
                            var db = redis.GetDatabase();
                            string recipeJson = JsonSerializer.Serialize(recipeMessage.Recipe);
                            // Push to the left of the list.
                            db.ListLeftPush("recentRecipes", recipeJson);
                            // Trim the list to keep only the first 3 elements.
                            db.ListTrim("recentRecipes", 0, 5000);
                        }
                    }

                    catch (Exception ex){
                        Console.WriteLine(" [!] Error processing message: " + ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" [!] Error processing message: " + ex.Message);
                }
            };

            // Start consuming messages.
            await channel.BasicConsumeAsync(
                queue: "recipe_queue",
                autoAck: true,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite);
        }

        static async Task<int> ProcessRecipeMessage(RecipeMessage recipeMessage, DbContextOptions<CookContext> options)
        {
            // Map the RecipeReturnDto to our EF Core entities.
            var dto = recipeMessage.Recipe;

            using var dbContext = new CookContext(options);

            // Create the Recipe entity.
            var recipeEntity = new Recipe
            {
                Title = dto.recipeName,
                Description = dto.recipeDescription,
                RecipeTypeId = 0, //todo: get the type id from the database
                Servings = 1,
                PrepTime = TimeSpan.FromMinutes(dto.recipePrepTime ?? 0),
                CookTime = TimeSpan.FromMinutes(dto.recipeCookTime ?? 0),
                UsageCount = 0,
                AverageRating = 0,
                IsPublic = true,
                CreatedDate = dto.recipeCreatedDate,
                UpdatedDate = DateTime.Now
            };

            dbContext.Recipes.Add(recipeEntity);
            await dbContext.SaveChangesAsync(); // Save to generate RecipeId

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == recipeMessage.Recipe.userId);
            if (user == null)
            {
                Console.WriteLine(" [!] User not found.");
                return -1;
            }

            var userRecipeEntity = new UserRecipe
            {
                IdOfUser = recipeMessage.Recipe.userId ?? "",
                RecipeId = recipeEntity.RecipeId,
                PermissionLevel = PermissionLevel.Owner
            };
            dbContext.UserRecipes.Add(userRecipeEntity);
            await dbContext.SaveChangesAsync(); // Save to generate RecipeId                

            var recipeRequestEntity = new RecipeRequest
            {
                UserId = recipeMessage.Recipe.userId ?? "",
                RecipeRequestDescription = recipeMessage.RecipeRequestDescription,
                RecipeRequestDate = DateTime.Now,
                RequestIpAddress = recipeMessage.RequestIpAddress ?? ""
            };

            dbContext.RecipeRequests.Add(recipeRequestEntity);
            await dbContext.SaveChangesAsync(); // Save to generate RecipeId

            // Process Recipe Steps.
            foreach (var stepDto in dto.recipeSteps)
            {
                var step = new RecipeStep
                {
                    RecipeId = recipeEntity.RecipeId,
                    StepNumber = stepDto.stepNumber,
                    Instruction = stepDto.stepDescription
                };
                dbContext.RecipeSteps.Add(step);
            }

            // Process Ingredients and RecipeIngredients.
            foreach (var ingDto in dto.ingredients)
            {
                // Check if the ingredient already exists (by name).
                var ingredient = dbContext.Ingredients.FirstOrDefault(i => i.Name == ingDto.ingredientName);
                if (ingredient == null)
                {
                    ingredient = new Ingredient { Name = ingDto.ingredientName };
                    dbContext.Ingredients.Add(ingredient);
                    await dbContext.SaveChangesAsync();
                }

                decimal quantity = 0;
                string measurementUnit = ingDto.ingredientQuantity;

                // Extract the numeric part and convert it to a decimal
                var tokens = ingDto.ingredientQuantity.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 0)
                {
                    string numericPart = tokens[0];

                    if (tokens.Length > 1 && Regex.IsMatch(tokens[1], @"^\d+/\d+$")) 
                    {
                        // Handle mixed fraction (e.g., "1 1/2 cups")
                        numericPart += " " + tokens[1]; // Combine the whole number and fraction
                        measurementUnit = string.Join(" ", tokens.Skip(2)); // Rest is the unit
                    }
                    else
                    {
                        // Normal case where first token is the quantity, rest is unit
                        measurementUnit = string.Join(" ", tokens.Skip(1));
                    }

                    // Convert numeric part to decimal
                    if (FunctionHelpers.TryParseFraction(numericPart, out var parsedQuantity))
                    {
                        quantity = parsedQuantity;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Unable to parse quantity '{numericPart}'");
                    }
                }

                var recipeIngredient = new RecipeIngredient
                {
                    RecipeId = recipeEntity.RecipeId,
                    IngredientId = ingredient.IngredientId,
                    Quantity = quantity,
                    MeasurementUnit = measurementUnit
                };

                dbContext.RecipeIngredients.Add(recipeIngredient);


            }

            //foreach note in recipeMessage.RecipeNotes
            if (recipeMessage.Recipe?.recipeNotes != null){
                foreach (var note in recipeMessage.Recipe?.recipeNotes ?? new List<string>())
                {
                    var recipeNote = new RecipeNotes
                    {
                        UserId = recipeMessage?.Recipe?.userId ?? "",
                        RecipeId = recipeEntity.RecipeId,
                        Notes = note
                    };
                    dbContext.RecipeNotes.Add(recipeNote);
                }
            }           

            await dbContext.SaveChangesAsync();
            Console.WriteLine(" [x] Recipe and its details saved to the database.");
            return recipeEntity.RecipeId;
        }
    }
}
}