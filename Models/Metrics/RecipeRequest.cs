namespace CookConsumer.Models
{
    public class RecipeRequest
    {
        public int RecipeRequestId { get; set; }        
        public string RecipeRequestDescription { get; set; } = "";
        public string RequestIpAddress { get; set; } = "";
        public string? UserId { get; set; }
        public DateTime RecipeRequestDate { get; set; } = DateTime.Now;
    }
}