using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class RecipeRating
{
    public int RecipeRatingId { get; set; }
    public int RecipeId { get; set; }
    
    // You can store a user ID if you have an identity system
    public string UserId { get; set; } = string.Empty;
    
    public Rating RatingValue { get; set; }  // e.g., 1–5
    public string Comments { get; set; } = string.Empty;    
    // Navigation
    public Recipe? Recipe { get; set; }
    public ApplicationUser? User { get; set; }
}

public class RecipeRatingConfiguration : IEntityTypeConfiguration<RecipeRating>
{
    public void Configure(EntityTypeBuilder<RecipeRating> builder)
    {
        builder.HasKey(e => e.RecipeRatingId);
        builder.HasOne(e => e.Recipe)
            .WithMany(r => r.Ratings)
            .HasForeignKey(e => e.RecipeId);
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}

public enum Rating
{
    OneStar,
    TwoStars,
    ThreeStars,
    FourStars,
    FiveStars
}