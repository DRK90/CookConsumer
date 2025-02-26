using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class RecipeType
{
    public int RecipeTypeId { get; set; }
    public string RecipeTypeName { get; set; } = string.Empty;
}

public class RecipeTypeConfiguration : IEntityTypeConfiguration<RecipeType>
{
    public void Configure(EntityTypeBuilder<RecipeType> builder)
    {
        builder.HasKey(e => e.RecipeTypeId);
        //seed data
        builder.HasData(
            new RecipeType { RecipeTypeId = 1, RecipeTypeName = "Breakfast" },
            new RecipeType { RecipeTypeId = 2, RecipeTypeName = "Lunch" },
            new RecipeType { RecipeTypeId = 3, RecipeTypeName = "Dinner" },
            new RecipeType { RecipeTypeId = 4, RecipeTypeName = "Dessert" },
            new RecipeType { RecipeTypeId = 5, RecipeTypeName = "Snack" },
            new RecipeType { RecipeTypeId = 6, RecipeTypeName = "Drink" },
            new RecipeType { RecipeTypeId = 7, RecipeTypeName = "Other" }
        );
    }
}
