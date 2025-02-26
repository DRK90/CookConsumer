using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class RecipeUsage
{
    public int RecipeUsageId { get; set; }
    public int RecipeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UsageType UsageType { get; set; }
    public DateTime UsageDate { get; set; }

    // Navigation properties:
    public Recipe? Recipe { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}

public enum UsageType
{
    View,
    Use
}

public class RecipeUsageConfiguration : IEntityTypeConfiguration<RecipeUsage>
{
    public void Configure(EntityTypeBuilder<RecipeUsage> builder)
    {
        builder.HasKey(e => e.RecipeUsageId);
        builder.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey(e => e.RecipeId);
        builder.HasOne(e => e.ApplicationUser)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}