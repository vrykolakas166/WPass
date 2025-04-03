using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;
using WPass.Core.Model;

namespace WPass.Core
{
    [SupportedOSPlatform("windows10.0")]
    public class WPContext : DbContext
    {
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<BrowserElement> BrowserElements { get; set; }

        public WPContext()
        {

        }

        public WPContext(DbContextOptions<WPContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WPass");

                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var dbPath = Path.Combine(directory, "main.db");

                    optionsBuilder.UseSqlite(InitializeSQLiteConnectionString(dbPath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error configuring DbContext: {ex.Message}");
                }
            }
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

        private static string InitializeSQLiteConnectionString(string databaseFile)
        {

#if RELEASE

            if (!File.Exists("en_key.dat"))
            {
                DpapiHelper.SavePassword(DpapiHelper.GenerateStrongPassword(32), "en_key.dat");
                DpapiHelper.SavePassword(DpapiHelper.GenerateStrongPassword(32), "db_pwd.dat");
            }
#endif

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                // PRAGMA key is being sent from EF Core directly after opening the connection

#if DEBUG
                Password = Environment.GetEnvironmentVariable("MY_DATABASE_PASSWORD") ?? throw new InvalidOperationException("Database password is not set in the environment variables.")
#else

                Password = DpapiHelper.LoadPassword("db_pwd.dat") ?? throw new InvalidOperationException("Database password file is not found.")
#endif
            };
            return connectionString.ToString();
        }
    }
}
