using System.Text.Json;
using System.Text.Json.Serialization;

namespace DbContext.BtsCopyDatabase
{
    public class Helpers
    {
        public static double? GetModuleCapacity(string moduleNumber)
        {
            double? capacity = null;
            using (var db = new BtsDbContext())
            {
                capacity = db.record
                    .Where(r => r.CellBarcode == moduleNumber
                             && r.StepName == "cc_dchg"
                             && r.TestId == db.record
                                 .Where(r2 => r2.CellBarcode == moduleNumber)
                                 .OrderByDescending(r2 => r2.Date)
                                 .Select(r2 => r2.TestId)
                                 .FirstOrDefault())
                    .OrderByDescending(r => r.SeqId)
                    .Select(r => r.Capacity)
                    .FirstOrDefault();
            }
            return capacity;
        }

        public static VoltageData? LoadSingleModuleDBCData(string moduleNumber, int minTestLength, BtsDbContext db)
        {
            VoltageData? voltageData = new VoltageData();
            Dictionary<int, double> cellVs = new();

            var result = db.record
                .Where(r => r.CellBarcode == moduleNumber
                         && r.StepName == "cc_dchg"
                         && r.TestId == db.record
                             .Where(r2 => r2.CellBarcode == moduleNumber
                                       && r2.DataUploadTag == "1")
                             .OrderByDescending(r2 => r2.Date)
                             .Select(r2 => r2.TestId)
                             .FirstOrDefault())
                .OrderBy(r => r.SeqId)
                .Select(r => new
                {
                    r.SeqId,
                    r.DBCSig
                })
                .ToList();
            foreach (var e in result)
            {
                if (e.DBCSig == null) throw new Exception($"Unable to find the dbc info for {moduleNumber}");
                DBCSig? dbcSig = JsonSerializer.Deserialize<DBCSig?>(e.DBCSig);
                if (dbcSig == null || dbcSig.LowCellVoltage == null || dbcSig.HighCellVoltage == null ||
                    dbcSig.LowCellVoltage == 0 || dbcSig.LowCellVoltage == 0)
                    return null;
                voltageData.LowCellVoltage.Add((double)dbcSig.LowCellVoltage);
                voltageData.HighCellVoltage.Add((double)dbcSig.HighCellVoltage);
                cellVs[e.SeqId] = ((double)dbcSig.HighCellVoltage - (double)dbcSig.LowCellVoltage);
            }
            if (voltageData.LowCellVoltage.Count < minTestLength || voltageData.HighCellVoltage.Count < minTestLength)
                return null;

            voltageData.TdeltaV = cellVs.MinBy(kvp => kvp.Key).Value;
            voltageData.BdeltaV = cellVs.MaxBy(kvp => kvp.Key).Value;
            return voltageData;
        }

        public static VoltageData? LoadSingleModuleAuxData(string moduleNumber, int mintestLength, BtsDbContext db)
        {
            VoltageData voltageData = new VoltageData();
            Dictionary<int, double> cellVs = new();
            var result = db.record
                .Where(r => r.CellBarcode == moduleNumber
                         && r.StepName == "cc_dchg"
                         && r.TestId == db.record
                             .Where(r2 => r2.CellBarcode == moduleNumber
                                       && r2.DataUploadTag == "1")
                             .OrderByDescending(r2 => r2.Date)
                             .Select(r2 => r2.TestId)
                             .FirstOrDefault())
                .OrderBy(r => r.SeqId)
                .Select(r => new
                {
                    r.AuxVmax,
                    r.AuxVmin,
                    r.AuxDiffVolt,
                    r.SeqId
                })
                .ToList();
            foreach (var e in result)
            {
                double? lowV = (double?)e.AuxVmin;
                double? highV = (double?)e.AuxVmax;
                double? diffVolt = (double?)e.AuxDiffVolt;
                if (lowV == null || highV == null || diffVolt == null) return null;
                voltageData.LowCellVoltage.Add((double)lowV);
                voltageData.HighCellVoltage.Add((double)highV);

                cellVs[e.SeqId] = (double)diffVolt;
            }

            if (voltageData.LowCellVoltage.Count < mintestLength || voltageData.LowCellVoltage.Count < mintestLength)
                return null;
            voltageData.TdeltaV = cellVs.MinBy(kvp => kvp.Key).Value;
            voltageData.BdeltaV = cellVs.MaxBy(kvp => kvp.Key).Value;
            return voltageData;
        }
    }

    public class DBCSig
    {
        [JsonPropertyName("Low_Cell_Voltage")]
        public double? LowCellVoltage { get; set; }
        [JsonPropertyName("High_Cell_Voltage")]
        public double? HighCellVoltage { get; set; }
        [JsonPropertyName("Low_Cell")]
        [JsonInclude]
        internal double? LowCell { set => LowCellVoltage = value; }
        [JsonPropertyName("High_Cell")]
        [JsonInclude]
        internal double? HighCell { set => HighCellVoltage = value; }
        [JsonPropertyName("MinCellV")]
        [JsonInclude]
        internal double? MinCellV { set => LowCellVoltage = value; }
        [JsonPropertyName("MaxCellV")]
        [JsonInclude]
        internal double? MaxCellV { set => HighCellVoltage = value; }

    }

    public class VoltageData
    {
        public List<double> LowCellVoltage { get; set; } = new List<double>();
        public List<double> HighCellVoltage { get; set; } = new List<double>();
        public double? TdeltaV { get; set; }
        public double? BdeltaV { get; set; }
    }
}
