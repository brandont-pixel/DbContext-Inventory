

namespace DbContext
{
    public class Helpers
    {
        public static int GetLatestIncrementFromBarcodes(string prefix, SqlSettings sqlSettings)
        {
            int highestIncrement = 0;
            using (var db = new SqlDbContext(sqlSettings))
            {
                var serials = db.barcodes
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

        public static void SaveToBarcodes(List<string> barcodes, SqlSettings sqlSettings)
        {
            using (var db = new SqlDbContext(sqlSettings))
            {
                foreach (var barcode in barcodes)
                {
                    var serialToAdd = new Barcodes
                    {
                        SerialNo = barcode,
                        Date = DateTime.Today
                    };

                    db.barcodes.Add(serialToAdd);
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
