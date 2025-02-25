using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookConsumer.Models;
public class LoginHistory
{
    public int LoginHistoryId { get; set; }
    public string UserId { get; set; } = String.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? IpAddress { get; set; }

    // Navigation properties
    public ApplicationUser? User { get; set; }
}

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(e => e.LoginHistoryId);
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

    }
}