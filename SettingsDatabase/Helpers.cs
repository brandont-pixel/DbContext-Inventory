namespace DbContext.SettingsDatabase
{
    public class Helpers
    {
        public static List<string> GetBuildOrderFromPrefix(string prefix)
        {
            using (var db = new SettingsDbContext())
            {
                var order = db.build_orders.Where(b => b.Prefix == prefix)
                    .OrderBy(b => b.OrderId)
                    .Select(b => b.ModuleType)
                    .ToList();
                return order;
            }
        }
        public static Dictionary<string, double> GetIrMultipliers(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.ir_multipliers.Where(m => m.Model == model)
                    .ToDictionary(m => m.ModuleType, m => m.Multiplier);
            }
        }
        public static List<string> GetAllModulePrefixes()
        {
            using (var db = new SettingsDbContext())
            {
                List<string> models = db.models.Where(r => r.ModuleOrBattery == "Module").Select(m => m.Model).ToList();
                return db.prefixes.Where(p => models.Contains(p.Model)).Select(p => p.Prefix).ToList();
            }
        }
        public static string? GetEditPassword()
        {
            using (var db = new SettingsDbContext())
            {
                return db.misc_settings.FirstOrDefault(r => r.Field1 == "EditPassword")?.Field2;
            }
        }
        public static List<string> GetAllBatteryModels()
        {
            using (var db = new SettingsDbContext())
            {
                return db.models.Where(m => m.ModuleOrBattery == "Battery").Select(m => m.Model).ToList();
            }
        }

        public static List<string> GetAllDistinctTypes(string prefix)
        {
            using (var db = new SettingsDbContext())
            {
                return db.build_orders.Where(b => b.Prefix == prefix)
                    .Select(b => b.ModuleType)
                    .Distinct()
                    .ToList();
            }
        }

        public static List<string> GetAllModuleModels()
        {
            using (var db = new SettingsDbContext())
            {
                return db.models.Where(m => m.ModuleOrBattery == "Module").Select(m => m.Model).ToList();
            }
        }

        public static ModuleSettingRow? GetModuleSettings(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.module_settings.FirstOrDefault(m => m.Model == model) ?? null;
            }
        }

        public static Dictionary<string, double> GetIRMultipliers(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.ir_multipliers.Where(m => m.Model == model)
                    .ToDictionary(m => m.ModuleType, m => m.Multiplier);
            }
        }

        public static List<string> GetPrefixes(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.prefixes.Where(p => p.Model == model)
                    .Select(p => p.Prefix)
                    .ToList();
            }
        }

        public static (string?, string?) GetModelFromSerial(string barcode)
        {
            using (var db = new SettingsDbContext())
            {
                var prefix = db.prefixes.Where(p => barcode.StartsWith(p.Prefix)).FirstOrDefault();
                if (prefix == null) return (null, null);
                return (prefix.Prefix, prefix.Model);
            }
        }

        public static string? GetModelFromPrefix(string prefix)
        {
            using (var db = new SettingsDbContext())
            {
                return db.prefixes.FirstOrDefault(p => p.Prefix == prefix)?.Model;
            }
        }

        public static BatterySettingRow? GetBatterySetting(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.battery_settings.FirstOrDefault(b => b.Model == model);
            }
        }

        public static List<string> GetBatteryTypesFromModel(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.battery_limits.Where(b => b.Model == model).Select(b => b.BatteryType).ToList();
            }
        }

        public static BatteryLimitRow? GetBatteryLimit(string model, string type)
        {
            using (var db = new SettingsDbContext())
            {
                return db.battery_limits.FirstOrDefault(b => b.Model == model && b.BatteryType == type);
            }
        }

        public static List<string> GetBuildOrder(string prefix)
        {
            using (var db = new SettingsDbContext())
            {
                var order = db.build_orders.Where(b => b.Prefix == prefix)
                    .OrderBy(b => b.OrderId)
                    .Select(b => b.ModuleType)
                    .ToList();
                return order;
            }
        }

        public static Dictionary<string, int> GetTypesPerPrefix(string prefix)
        {
            using (var db = new SettingsDbContext())
            {
                return db.build_orders.Where(b => b.Prefix == prefix)
                    .GroupBy(b => b.ModuleType)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        }
    }
}
