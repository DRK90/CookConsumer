using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookConsumer.Models;
public class Membership
{
    public int MembershipId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public MembershipLevel MembershipLevel { get; set; }

    // Navigation properties
    public ApplicationUser? User { get; set; }
}

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasKey(e => e.MembershipId);
        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Membership>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public enum MembershipLevel
{
    Free,
    Trial,
    Premium
}
