using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using deha_api_exam.Models;

namespace deha_api_exam.DBConfiguration
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            // auto = DeleteBehavior.Cascade  => auto delete child when parent get deleted
            builder.HasMany(e => e.Attachments)
            .WithOne(e => e.Post)
            .HasForeignKey(e => e.PostID)
            .HasPrincipalKey(e => e.Id);
          

            builder.HasMany(e => e.Comments)
             .WithOne(e => e.Post)
             .HasForeignKey(e => e.PostID)
             .HasPrincipalKey(e => e.Id);

            builder.HasMany(e => e.Vote)
             .WithOne(e => e.Post)
             .HasForeignKey(e => e.PostID)
             .HasPrincipalKey(e => e.Id);
        }
    }
}
