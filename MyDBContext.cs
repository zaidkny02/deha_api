using deha_api_exam.DBConfiguration;
using deha_api_exam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace deha_api_exam
{
    public class MyDBContext : IdentityDbContext<User>
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityRole>().ToTable("Roles").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            modelBuilder.Entity<User>().ToTable("Users").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

        public DbSet<Post> Post { get; set; } = null!;
        public DbSet<Attachment> Attachment { get; set; } = null!;

        public DbSet<Comment> Comment { get; set; } = null!;
        public DbSet<Vote> Vote { get; set; } = null!;

    }
}
