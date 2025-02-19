namespace CookConsumer.Models;
public class RecipeStep
{
    public int RecipeStepId { get; set; }
    public int RecipeId { get; set; }    
    // The numeric order of the step
    public int StepNumber { get; set; }    
    // The textual instruction
    public string Instruction { get; set; } = string.Empty;
    
    // Navigation
    public Recipe? Recipe { get; set; }
}