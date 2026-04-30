namespace DbContext.InventoryDatabase
{
    public class Helpers
    {
        public static List<string> GetLocations()
        {
            using (var db = new CapacityDbContext())
            {
                return db.warranty_table.Select(w => w.Location).Distinct().ToList();
            }
        }

        public static List<string> GetHeaders(string location)
        {
            using (var db = new CapacityDbContext())
            {
                return db.warranty_table.Where(w => w.Location == location).Select(w => w.Header).Distinct().ToList();
            }
        }
        public static List<string> GetWarranties(string header, string location)
        {
            using (var db = new CapacityDbContext())
            {
                return db.warranty_table.Where(w => w.Header == header && w.Location == location).Select(w => w.Warranty).ToList();
            }
        }

        public static CapacityRow? LookupModule(string serialNo)
        {
            using (var db = new CapacityDbContext())
            {
                return db.capacity_table.FirstOrDefault(m => m.SerialNo == serialNo);
            }
        }

        public static void EditModule(string serialNo, string model, string? type,
            double capacity, double ir, string? location = null, bool discarded = false, string? notes = null)
        {
            using (var db = new CapacityDbContext())
            {
                var module = db.capacity_table.FirstOrDefault(m => m.SerialNo == serialNo);
                if (type == "N/A" || string.IsNullOrEmpty(type)) type = null;
                if (location == "N/A" || string.IsNullOrEmpty(location)) location = null;
                if (string.IsNullOrEmpty(notes)) notes = null;
                if (module == null) return;
                module.Model = model;
                module.Type = type;
                module.Capacity = capacity;
                module.IR = ir;
                module.Location = location;
                module.Discarded = discarded;
                module.Notes = notes;
                db.SaveChanges();
            }
        }

        public static bool IsModuleInDatabase(string serialNo)
        {
            using (var db = new CapacityDbContext())
            {
                return db.capacity_table.Any(m => m.SerialNo == serialNo);
            }
        }

        public static bool DiscardSelectedModules(List<string> serialNos, string note)
        {
            using (var db = new CapacityDbContext())
            {
                foreach (var serial in serialNos)
                {
                    var module = db.capacity_table.FirstOrDefault(m => m.SerialNo == serial);
                    if (module != null)
                    {
                        module.Discarded = true;
                        module.Notes = note;
                    }
                }
                db.SaveChanges();
            }
            return true;
        }

        public static int GetLatestIncrementFromBarcodes(string prefix)
        {
            int highestIncrement = 0;
            using (var db = new CapacityDbContext())
            {
                var serials = db.barcode_table
                    .Where(s => s.SerialNo.StartsWith(prefix))
                    .Select(s => s.SerialNo)
                    .ToList();

                if (!serials.Any())
                    return 0;

                foreach (var serial in serials)
                {
                    int increment = FindLastIncrement(serial);
                    if (increment > highestIncrement) highestIncrement = increment;
                }
            }
            return highestIncrement;
        }

        public static void SaveToBarcodes(List<string> barcodes)
        {
            using (var db = new CapacityDbContext())
            {
                foreach (string barcode in barcodes)
                {
                    var serialToAdd = new BarcodeRow
                    {
                        SerialNo = barcode,
                        Date = DateTime.Today
                    };
                    db.barcode_table.Add(serialToAdd);
                    db.SaveChanges();
                }
            }
        }

        public static int FindLastIncrement(string serialNo)
        {
            string increment = serialNo.Substring(serialNo.Length - 4);
            if (int.TryParse(increment, out int result))
                return result;
            else throw new Exception("Cannot parse serial No");
        }
    }
}
