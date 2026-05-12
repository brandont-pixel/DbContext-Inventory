using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DbContext.SettingsDatabase
{
    public class SettingsDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connString;
        public SettingsDbContext()
        {
            _connString =
                "Server=192.168.50.200;" +
                "Port=3306;" +
                "Database=settings;" +
                "User Id=admin;" +
                "Password=admin;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(_connString, ServerVersion.AutoDetect(_connString));

        public DbSet<ModelRow> models { get; set; }
        public DbSet<PrefixRow> prefixes { get; set; }
        public DbSet<IRMultiplierRow> ir_multipliers { get; set; }
        public DbSet<ModuleSettingRow> module_settings { get; set; }
        public DbSet<VoltageDataSourceRow> voltage_data_sources { get; set; }
        public DbSet<BatterySettingRow> battery_settings { get; set; }
        public DbSet<BatteryLimitRow> battery_limits { get; set; }
        public DbSet<BuildOrderRow> build_orders { get; set; }
        public DbSet<MiscRow> misc_settings { get; set; }
    }


    public class MiscRow
    {
        [Key]
        public int id { get; set; }
        public string Field1 { get; set; } = "";
        public string Field2 { get; set; } = "";
    }

    public class ModelRow
    {
        [Key]
        public string Model { get; set; } = "";
        public string ModuleOrBattery { get; set; } = "";
    }

    public class PrefixRow
    {
        [Key]
        public string Prefix { get; set; } = "";
        public string Model { get; set; } = "";
    }

    [PrimaryKey(nameof(Model), nameof(ModuleType))]
    public class IRMultiplierRow
    {
        public string Model { get; set; } = "";
        public string ModuleType { get; set; } = "";
        public double Multiplier { get; set; }
    }

    public class ModuleSettingRow
    {
        [Key]
        public string Model { get; set; } = "";
        public double? CapacityLowerLimit { get; set; }
        public double? IRUpperLimit { get; set; }
        public string VoltageDataSource { get; set; } = "";
        public double CutoffVoltage { get; set; }
        public double ClusterCutoffVoltage { get; set; }
        public double DeltaVLimit { get; set; }
        public double DeltaIRLimit { get; set; }
        public double DeltaCapLimit { get; set; }
        public int MinTestLength { get; set; }
        public int IRWeight { get; set; }
        public int CapWeight { get; set; }
        public int VoltWeight { get; set; }
    }

    public class BatterySettingRow
    {
        [Key]
        public string Model { get; set; } = "";
        public string VoltageDataSource { get; set; } = "";
        public int MinTestLength { get; set; }
    }

    public class BuildOrderRow
    {
        [Key]
        public int OrderId { get; set; }
        public string Prefix { get; set; } = "";
        public string ModuleType { get; set; } = "";
    }

    public class BatteryLimitRow
    {
        [Key]
        public string BatteryType { get; set; } = "";
        public double DVLimit { get; set; }
        public double CapLimit { get; set; }
        public string Model { get; set; } = "";
    }

    public class VoltageDataSourceRow
    {
        [Key]
        public string VoltageDataSource { get; set; } = "";
    }
}
