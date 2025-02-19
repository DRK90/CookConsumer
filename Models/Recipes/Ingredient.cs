using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CookConsumer.Models;
public class Ingredient
{
    public int IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    // Optional link to NutritionInfo
    public int? NutritionInfoId { get; set; }
    public NutritionInfo? NutritionInfo { get; set; }
    // Navigation
    public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
}

public class NutritionInfo{
    public int NutritionInfoId { get; set; }
    public decimal? Calories { get; set; }
    public decimal? Carbohydrates { get; set; }
    public decimal? Protein { get; set; }
    public decimal? Fat { get; set; }
    public decimal? Fiber { get; set; }
    public decimal? Sugar { get; set; }
}