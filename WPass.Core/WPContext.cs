using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
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

#if !DEBUG
                    // If main.db doesn't exist, copy template.db to ApplicationData folder
                    if (!File.Exists(dbPath))
                    {
                        var templateDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "init.db");
                        SetSecureFilePermissions(templateDbPath);

                        if (File.Exists(templateDbPath))
                        {
                            File.Copy(templateDbPath, dbPath);
                            SetSecureFilePermissions(dbPath); // Set permissions after copying
                        }
                        else
                        {
                            throw new Exception("Data lost. Please re-install the application.");
                        }
                    }
#endif

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
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = "J!b&>5F;^MZuCn\"ehP{'z/WrBq3sLam-9R:}tA`KkV,~U)YGX?"// PRAGMA key is being sent from EF Core directly after opening the connection
            };
            return connectionString.ToString();
        }

        private static void SetSecureFilePermissions(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                var security = fileInfo.GetAccessControl();

                // Remove all users except the owner and the system
                security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

                // Grant access to the current user and system
                var currentUser = WindowsIdentity.GetCurrent().Name;
                security.AddAccessRule(new FileSystemAccessRule(currentUser,
                    FileSystemRights.FullControl, AccessControlType.Allow));

                // Optionally grant access to SYSTEM
                security.AddAccessRule(new FileSystemAccessRule("SYSTEM",
                    FileSystemRights.FullControl, AccessControlType.Allow));

                // Apply the permissions to the file
                fileInfo.SetAccessControl(security);
            }
        }
    }
}
