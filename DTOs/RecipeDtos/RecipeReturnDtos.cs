namespace CookConsumer
{
    public class RecipeReturnDto
    {
        public string? userId { get; set; }
        public string recipeName { get; set; } = "";
        public string recipeDescription { get; set; } = "";
        public string recipeType { get; set; } = "";
        public List<string>? recipeNotes { get; set; }
        public double? userRecipeRating { get; set; }
        public double? averageRecipeRating { get; set; }
        public double? recipePrepTime { get; set; }
        public double? recipeCookTime { get; set; }
        public DateTime recipeCreatedDate { get; set; } = DateTime.Now;
        public List<IngredientDto> ingredients { get; set; } = new List<IngredientDto>();
        public List<RecipeStepDto> recipeSteps { get; set; } = new List<RecipeStepDto>();
    }

    public class IngredientDto
    {
        public int ingredientId { get; set; }
        public string ingredientName { get; set; } = "";
        public string ingredientQuantity { get; set; } = "";
    }

    public class NutritionDto
    {
        public int nutritionId { get; set; }
        public int ingredientId { get; set; }
        public string nutritionName { get; set; } = "";
        public string nutritionValue { get; set; } = "";
    }

    public class RecipeStepDto
    {
        public int stepNumber { get; set; }
        public string stepDescription { get; set; } = "";
    }


}