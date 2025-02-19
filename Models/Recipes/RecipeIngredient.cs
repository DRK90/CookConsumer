namespace CookConsumer.Models;
public class RecipeIngredient
{
    public int RecipeIngredientId { get; set; }
    
    public int RecipeId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string MeasurementUnit { get; set; } = string.Empty;
    // Navigation
    public Recipe? Recipe { get; set; }
    public Ingredient? Ingredient { get; set; }
}