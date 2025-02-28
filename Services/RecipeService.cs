using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CookConsumer.Models;
using CookConsumer.Helpers;

namespace CookConsumer.Services
{
    public class RecipeService
    {
        private readonly DbContextOptions<CookContext> _dbOptions;

        public RecipeService(DbContextOptions<CookContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task<int> ProcessRecipeMessage(RecipeMessage recipeMessage)
        {
            var dto = recipeMessage.Recipe;

            using var dbContext = new CookContext(_dbOptions);

            // Create the Recipe entity.
            var recipeEntity = new Recipe
            {
                Title = dto.recipeName,
                Description = dto.recipeDescription,
                RecipeTypeId = dbContext.RecipeTypes.FirstOrDefault(rt => rt.RecipeTypeName == dto.recipeType)?.RecipeTypeId ?? 0,
                Servings = dto.servings,
                PrepTime = TimeSpan.FromMinutes(dto.recipePrepTime ?? 0),
                CookTime = TimeSpan.FromMinutes(dto.recipeCookTime ?? 0),
                UsageCount = 0,
                AverageRating = 0,
                IsPublic = true,
                CreatedDate = dto.recipeCreatedDate,
                UpdatedDate = DateTime.Now,
            };

            dbContext.Recipes.Add(recipeEntity);
            await dbContext.SaveChangesAsync();

            // Associate the recipe with the user.
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
            await dbContext.SaveChangesAsync();

            // Log the recipe request.
            var recipeRequestEntity = new RecipeRequest
            {
                UserId = recipeMessage.Recipe.userId ?? "",
                RecipeRequestDescription = recipeMessage.RecipeRequestDescription,
                RecipeRequestDate = DateTime.Now,
                RequestIpAddress = recipeMessage.RequestIpAddress ?? ""
            };
            dbContext.RecipeRequests.Add(recipeRequestEntity);
            await dbContext.SaveChangesAsync();

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
                // Find or create ingredient.
                var ingredient = dbContext.Ingredients.FirstOrDefault(i => i.Name == ingDto.ingredientName);
                if (ingredient == null)
                {
                    ingredient = new Ingredient { Name = ingDto.ingredientName };
                    dbContext.Ingredients.Add(ingredient);
                    await dbContext.SaveChangesAsync();
                }
                
                decimal quantity = Math.Round(ingDto.ingredientQuantity, 2);
                string measurementUnit = ingDto.ingredientUnit;


                var recipeIngredient = new RecipeIngredient
                {
                    RecipeId = recipeEntity.RecipeId,
                    IngredientId = ingredient.IngredientId,
                    Quantity = quantity,
                    MeasurementUnit = measurementUnit
                };

                dbContext.RecipeIngredients.Add(recipeIngredient);
            }

            // Process Notes.
            if (dto.recipeNotes != null)
            {
                foreach (var note in dto.recipeNotes)
                {
                    var recipeNote = new RecipeNotes
                    {
                        UserId = recipeMessage.Recipe.userId ?? "",
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
    
        public async Task DeleteRecipeMessage(RecipeMessage recipeMessage)
        {
            using var dbContext = new CookContext(_dbOptions);
            int recipeId = recipeMessage.Recipe.recipeId ?? throw new Exception("Recipe ID is null");
            string requestingUserId = recipeMessage.Recipe.userId ?? throw new Exception("User ID is null");

            // Attempt to find the recipe.
            var recipe = await dbContext.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                Console.WriteLine(" [!] Recipe not found.");
                return;
            }

            // Count how many active users are associated with this recipe.
            // Assuming your User entity has a bool property named "isActive"
            var activeUserCount = await (from ur in dbContext.UserRecipes
                                         join u in dbContext.ApplicationUsers on ur.IdOfUser equals u.Id
                                         where ur.RecipeId == recipeId && u.IsActive
                                         select ur).CountAsync();

            if (activeUserCount <= 1)
            {
                // Only one active user is associated:
                // Remove all user associations and delete the recipe.
                var userAssociations = dbContext.UserRecipes.Where(ur => ur.RecipeId == recipeId);
                dbContext.UserRecipes.RemoveRange(userAssociations);

                dbContext.Recipes.Remove(recipe);
                Console.WriteLine($" [x] Deleted recipe {recipeId} completely.");
            }
            else
            {
                // More than one active user: remove only the current user's association.
                var userRecipe = await dbContext.UserRecipes
                    .FirstOrDefaultAsync(ur => ur.RecipeId == recipeId && ur.IdOfUser == requestingUserId);
                if (userRecipe != null)
                {
                    dbContext.UserRecipes.Remove(userRecipe);
                    Console.WriteLine($" [x] Removed recipe association for user {requestingUserId}.");
                }
                else
                {
                    Console.WriteLine($" [!] No association found for user {requestingUserId} on recipe {recipeId}.");
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
