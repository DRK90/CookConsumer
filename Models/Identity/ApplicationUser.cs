   
   using Microsoft.AspNetCore.Identity;
   using CookConsumer.Helpers;
   namespace CookConsumer.Models;
   public class ApplicationUser : IdentityUser
   {
       public bool IsActive { get; set; } = true;
       public DateTime CreatedAt { get; set; } = DateTimeHelper.ConvertToEasternTime(DateTime.UtcNow);

       // Navigation properties
   }

