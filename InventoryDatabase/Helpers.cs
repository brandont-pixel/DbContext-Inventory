namespace DbContext.InventoryDatabase
{
    public class Helpers
    {

        public static bool IsModuleInDatabase(string serialNo)
        {
            using (var db = new CapacityDbContext())
            {
                return db.capacity_table.Any(m => m.SerialNo.ToUpper() == serialNo.ToUpper());
            }
        }

        public static bool DiscardSelectedModules(List<string> serialNos, string note)
        {
            using (var db = new CapacityDbContext())
            {
                foreach (var serial in serialNos)
                {
                    var module = db.capacity_table.FirstOrDefault(m => m.SerialNo.ToUpper() == serial.ToUpper());
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
                foreach (var barcode in barcodes)
                {
                    var serialToAdd = new BarcodeRow
                    {
                        SerialNo = barcode,
                        Date = DateTime.Today
                    };

                    db.barcode_table.Add(serialToAdd);
                }
                db.SaveChanges();
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
