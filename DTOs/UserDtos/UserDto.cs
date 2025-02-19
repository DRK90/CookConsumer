using System.Diagnostics.Eventing.Reader;

namespace QuicklyCook.Dtos{
    public class UserDto
    {
        public string id { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public bool mfaEnabled { get; set; }
        public DateTime createdAt { get; set; }

    }
}