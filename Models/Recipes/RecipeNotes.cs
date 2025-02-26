using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class RecipeNotes
{
    public int RecipeNotesId { get; set; }
    public int RecipeId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    // Navigation
    public Recipe? Recipe { get; set; }
    public ApplicationUser? User { get; set; }
}

public class RecipeNotesConfiguration : IEntityTypeConfiguration<RecipeNotes>
{
    public void Configure(EntityTypeBuilder<RecipeNotes> builder)
    {
        builder.HasKey(e => e.RecipeNotesId);
        builder.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey(e => e.RecipeId);
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}