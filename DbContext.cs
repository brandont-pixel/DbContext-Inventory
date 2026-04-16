using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace DbContext
{
    public class SqlDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connectionString;
        public SqlDbContext(SqlSettings sqlSettings)
        {
            _connectionString =
                $"Server={sqlSettings.Server};" +
                $"Port={sqlSettings.Port};" +
                $"Database={sqlSettings.Database};" +
                $"User Id={sqlSettings.UserId};" +
                $"Password={sqlSettings.Password};";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));


        public DbSet<Barcodes> barcodes { get; set; }
    }



    public class Barcodes
    {
        [Key]
        public string SerialNo { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class SqlSettings
    {
        public string Server { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
