using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolFileServer.Models;

namespace SchoolFileServer.Data
{
    public class SchoolFileContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public SchoolFileContext(DbContextOptions<SchoolFileContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<SchoolFile> Files { get; set; }
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserAccount configuration
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            });

            // SchoolFile configuration
            modelBuilder.Entity<SchoolFile>(entity =>
            {
                entity.HasKey(e => e.FileId);
                entity.HasIndex(e => new { e.Class, e.Subject });
                entity.HasIndex(e => e.UploadDate);
                entity.Property(e => e.UploadDate).HasDefaultValueSql("datetime('now')");

                entity.HasOne(e => e.UploadedByUser)
                    .WithMany(u => u.UploadedFiles)
                    .HasForeignKey(e => e.UploadedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SchoolClass configuration
            modelBuilder.Entity<SchoolClass>(entity =>
            {
                entity.HasKey(e => e.ClassId);
                entity.HasIndex(e => e.ClassName).IsUnique();
                entity.Property(e => e.ClassName).IsRequired();
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.SubjectId);
                entity.HasIndex(e => new { e.ClassId, e.SubjectName }).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Subjects)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedSubjects)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Classes
            var classes = new[]
            {
                new SchoolClass { ClassId = 1, ClassName = "VI", DisplayName = "Class VI", SortOrder = 1 },
                new SchoolClass { ClassId = 2, ClassName = "VII", DisplayName = "Class VII", SortOrder = 2 },
                new SchoolClass { ClassId = 3, ClassName = "VIII", DisplayName = "Class VIII", SortOrder = 3 },
                new SchoolClass { ClassId = 4, ClassName = "IX", DisplayName = "Class IX", SortOrder = 4 },
                new SchoolClass { ClassId = 5, ClassName = "X", DisplayName = "Class X", SortOrder = 5 },
                new SchoolClass { ClassId = 6, ClassName = "XI", DisplayName = "Class XI", SortOrder = 6 },
                new SchoolClass { ClassId = 7, ClassName = "XII", DisplayName = "Class XII", SortOrder = 7 }
            };

            modelBuilder.Entity<SchoolClass>().HasData(classes);

            // Seed default admin user from configuration
            var adminUsername = _configuration["DefaultCredentials:AdminUsername"] ?? "admin";
            var adminPassword = _configuration["DefaultCredentials:AdminPassword"] ?? "admin123";
            
            var adminUser = new UserAccount
            {
                UserId = 1,
                Username = adminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                UserType = UserType.Admin,
                AssignedClasses = "[\"VI\",\"VII\",\"VIII\",\"IX\",\"X\",\"XI\",\"XII\"]",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            modelBuilder.Entity<UserAccount>().HasData(adminUser);
        }
    }
}
