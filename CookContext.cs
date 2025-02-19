using Microsoft.EntityFrameworkCore;
using CookConsumer.Models;
namespace CookConsumer
{
    public class CookContext : DbContext
    {
        public CookContext(DbContextOptions<CookContext> options) 
            : base(options)
        {
        }

        // Only include the tables (entities) that you need:
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        

    }
}
