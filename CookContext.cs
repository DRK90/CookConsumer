using Microsoft.EntityFrameworkCore;
using CookConsumer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CookConsumer
{
    public class CookContext : IdentityDbContext<ApplicationUser>
    {
        public CookContext(DbContextOptions<CookContext> options) 
            : base(options)
        {
        }

        // Identity
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        // Only include the tables (entities) that you need:
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        public DbSet<UserRecipe> UserRecipes { get; set; }
        public DbSet<RecipeRequest> RecipeRequests { get; set; }

    }
}
