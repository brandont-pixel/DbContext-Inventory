using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace DbContext.InventoryDatabase
{
    public class CapacityDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connectionString;
        public CapacityDbContext()
        {
            _connectionString =
                "Server=192.168.50.200;" +
                "Port=3306;" +
                "Database=inventory;" +
                "User Id=admin;" +
                "Password=admin;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));


        public DbSet<BarcodeRow> barcode_table { get; set; }
        public DbSet<CapacityRow> capacity_table { get; set; }
    }

    public class CapacityRow
    {
        [Key]
        public string SerialNo { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Type { get; set; }
        public double Capacity { get; set; }
        public double IR { get; set; }
        public string? Location { get; set; }
        public bool Discarded { get; set; }
        public string? Notes { get; set; }
    }

    public class BarcodeRow
    {
        [Key]
        public string SerialNo { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
