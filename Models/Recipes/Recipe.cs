using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class Recipe
{
    public int RecipeId { get; set; }
    //Parent RecipeId
    public int? ParentRecipeId { get; set; }    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecipeTypeId { get; set; }
    // Default Servings for the recipe
    public int Servings { get; set; }    
    public TimeSpan? PrepTime { get; set; }
    public TimeSpan? CookTime { get; set; }
    public int UsageCount { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    // Navigation properties:
    public ICollection<RecipeStep>? Steps { get; set; }
    public ICollection<RecipeRating>? Ratings { get; set; }

    public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
}

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(e => e.RecipeId);
        builder.HasMany(e => e.Steps)
            .WithOne(e => e.Recipe)
            .HasForeignKey(e => e.RecipeId);
        builder.HasMany(e => e.RecipeIngredients)
            .WithOne(e => e.Recipe)
            .HasForeignKey(e => e.RecipeId);
    }
}