using System.ComponentModel.DataAnnotations;

namespace QuicklyCook.Dtos
{
    public class MfaDto
    {
        [Required(ErrorMessage = "The MFA code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The MFA code must be a 6-digit number.")]
        public string Code { get; set; } = string.Empty;
    }
}