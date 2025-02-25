using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CookConsumer.Models;
namespace CookConsumer.Models;
public class UserRecipe
{
    public int UserRecipeId { get; set; }
    public string IdOfUser { get; set; } = string.Empty;
    public PermissionLevel PermissionLevel { get; set; }
    public int RecipeId { get; set; }

    // Navigation properties:
    public Recipe? Recipe { get; set; }
}

public enum PermissionLevel
{
    Owner,
    Editor,
    Viewer
}

public class UserRecipeConfiguration : IEntityTypeConfiguration<UserRecipe>
{
    public void Configure(EntityTypeBuilder<UserRecipe> builder)
    {
        builder.HasKey(e => e.UserRecipeId);

        builder.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey(e => e.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}