namespace DbContext.SettingsDatabase
{
    public class Helpers
    {
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
        public static List<string> GetPrintablePrefixes()
        {
            using (var db = new SettingsDbContext())
            {
                return db.printable_prefixes.OrderBy(p => p.Prefix).Select(p => p.Prefix).ToList();
            }
        }
        //Module Recommender options, one per model with a build order: key is the PrimaryPrefix shown, value is a real prefix of that model.
        //A model can have several prefixes; printable ones win over legacy ones, then alphabetical (CV1 for Volt)
        public static List<KeyValuePair<string, string>> GetPrimaryPrefixes()
        {
            using (var db = new SettingsDbContext())
            {
                return db.models
                    .Where(m => m.ModuleOrBattery == "Module" && !string.IsNullOrEmpty(m.PrimaryPrefix))
                    .Where(m => db.build_orders.Any(b => b.Model == m.Model))
                    .OrderBy(m => m.PrimaryPrefix)
                    .Select(m => new
                    {
                        m.PrimaryPrefix,
                        Prefix = db.prefixes
                            .Where(p => p.Model == m.Model)
                            .OrderByDescending(p => db.printable_prefixes.Any(pp => pp.Prefix == p.Prefix))
                            .ThenBy(p => p.Prefix)
                            .Select(p => p.Prefix)
                            .FirstOrDefault()
                    })
                    .AsEnumerable()
                    .Where(m => m.Prefix != null)
                    .Select(m => KeyValuePair.Create(m.PrimaryPrefix!, m.Prefix!))
                    .ToList();
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

        public static List<string> GetAllDistinctTypes(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.build_orders.Where(b => b.Model == model)
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
                //longest match wins, so NIROEV-... resolves to NIROEV rather than NIRO
                var prefix = db.prefixes.Where(p => barcode.StartsWith(p.Prefix))
                    .OrderByDescending(p => p.Prefix.Length)
                    .FirstOrDefault();
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

        public static List<string> GetBuildOrder(string model)
        {
            using (var db = new SettingsDbContext())
            {
                var order = db.build_orders.Where(b => b.Model == model)
                    .OrderBy(b => b.OrderId)
                    .Select(b => b.ModuleType)
                    .ToList();
                return order;
            }
        }

        public static Dictionary<string, int> GetTypesPerModel(string model)
        {
            using (var db = new SettingsDbContext())
            {
                return db.build_orders.Where(b => b.Model == model)
                    .GroupBy(b => b.ModuleType)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        }
    }
}
