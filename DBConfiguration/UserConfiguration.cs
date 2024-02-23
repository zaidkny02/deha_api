using deha_api_exam.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace deha_api_exam.DBConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // auto = DeleteBehavior.Cascade  => auto delete child when parent get deleted
            builder.HasMany(e => e.Posts)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserID)
            .HasPrincipalKey(e => e.Id);

            builder.HasMany(e => e.Comments)
             .WithOne(e => e.User)
             .HasForeignKey(e => e.UserID)
             .HasPrincipalKey(e => e.Id)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Votes)
             .WithOne(e => e.User)
             .HasForeignKey(e => e.UserID)
             .HasPrincipalKey(e => e.Id)
             .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
