using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<EventReport> EventReports { get; set; }
        public DbSet<EventFavorite> EventFavorites { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventFavorite>()
                .HasKey(ef => new { ef.EventId, ef.UserId });

            // 1. Quan hệ từ EventFavorite đến Event
            builder.Entity<EventFavorite>()
                .HasOne(ef => ef.Event)
                .WithMany() // Nếu bạn chưa khai báo ICollection trong model Event
                .HasForeignKey(ef => ef.EventId);

            // 2. Quan hệ từ EventFavorite đến ApplicationUser (UserId)
            // ApplicationUser là lớp User của bạn
            builder.Entity<EventFavorite>()
                .HasOne(ef => ef.User) // Giả sử bạn đặt tên Navigation Property là User
                .WithMany() // Nếu bạn chưa khai báo ICollection trong model ApplicationUser
                .HasForeignKey(ef => ef.UserId);
        }
        public ICollection<EventFavorite> FavoriteEvents { get; set; } = new List<EventFavorite>();
        public ICollection<EventReport> SubmittedReports { get; set; } = new List<EventReport>();
    }
}