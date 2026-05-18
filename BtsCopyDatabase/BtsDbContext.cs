using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.BtsCopyDatabase
{
    public class BtsDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connectionString;
        public BtsDbContext()
        {
            string? userId = Environment.GetEnvironmentVariable("DB_USER_ID");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            if (userId == null || password == null) throw new Exception("Database credentials not found in environment variables");
            _connectionString =
                "Server=192.168.50.200;" +
                "Port=3306;" +
                "Database=bts_copy;" +
                $"User Id={userId};" +
                $"Password={password};";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecordSliceRow>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            modelBuilder.Entity<RecordRow>()
                .HasKey(r => new { r.ChId, r.ComputerName, r.TestId, r.SeqId });
        }

        public DbSet<RecordRow> record { get; set; }

        public DbSet<RecordSliceRow> slice { get; set; }
    }

    public class RecordSliceRow
    {
        public string CellBarcode { get; set; } = "";
        public int SeqId { get; set; }
        public string StepName { get; set; } = "";
        public string? DBCSig { get; set; }
        public double? AuxVmax { get; set; }
        public double? AuxVmin { get; set; }
        public double? AuxDiffVolt { get; set; }
        public string TestID { get; set; } = "";
    }

    public class RecordRow
    {
        [Column("CH_ID")]
        public string ChId { get; set; } = string.Empty;
        [Column("computer_name")]
        public string ComputerName { get; set; } = string.Empty;
        [Column("test_id")]
        public string TestId { get; set; } = string.Empty;
        [Column("seq_id")]
        public int SeqId { get; set; }
        [Column("Cell_Barcode")]
        public string? CellBarcode { get; set; }
        [Column("step_num")]
        public string? StepNum { get; set; }
        [Column("Original_Step")]
        public string? OgStep { get; set; }
        [Column("record_id")]
        public string? RecordId { get; set; }
        [Column("Record_Time")]
        public string? RecordTime { get; set; }
        [Column("Voltage")]
        public double? Voltage { get; set; }
        [Column("Current")]
        public string? Current { get; set; }
        [Column("Capacity")]
        public double? Capacity { get; set; }
        [Column("Energy")]
        public string? Energy { get; set; }
        [Column("Power")]
        public string? Power { get; set; }
        [Column("Date")]
        public string? Date { get; set; }
        [Column("Aux_Vmax")]
        public double? AuxVmax { get; set; }
        [Column("Aux_Vmin")]
        public double? AuxVmin { get; set; }
        [Column("Step_Name")]
        public string? StepName { get; set; }
        [Column("DataUploadTag")]
        public string? DataUploadTag { get; set; }
        [Column("AuxVolt")]
        public string? AuxVolt { get; set; }
        [Column("AuxDiffVolt")]
        public double? AuxDiffVolt { get; set; }
        [Column("DBCSig")]
        public string? DBCSig { get; set; }
    }
}
