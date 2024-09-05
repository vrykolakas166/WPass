using Microsoft.EntityFrameworkCore;
using WPass.Core.Model;

namespace WPass.Core
{
    public class WPContext : DbContext
    {
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Setting> Settings{ get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<BrowserElement> BrowserElements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WPass");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var dbPath = Path.Combine(directory, "main.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entry>()
                .HasMany(e => e.Websites)
                .WithOne(w => w.Entry)
                .HasForeignKey(w => w.EntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
