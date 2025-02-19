using System;
namespace CookConsumer
{
    public class RecipeMessage
    {
        public string RecipeRequestDescription { get; set; } = "";
        public string? RequestIpAddress { get; set; }
        public DateTime RecipeRequestDate { get; set; }
        public RecipeReturnDto Recipe { get; set; } = new RecipeReturnDto();
    }
}
