using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DbContext.BtsCopyDatabase
{
    public class Helpers
    {
        public static List<RecordSliceRow> LoadRecordByChunks(BtsDbContext btsDb, List<string> barcodes, int chunkSize = 500)
        {
            var allRows = new List<RecordSliceRow>();

            foreach (var chunk in barcodes.Chunk(chunkSize))
            {
                string[] names = chunk.Select((_, i) => $"@p{i}").ToArray();
                string inClause = string.Join(", ", names);

                string sql = $@"
                    WITH latest AS ( 
                        SELECT 
                            r.Cell_Barcode AS CellBarcode, 
                            r.Test_ID      AS TestId, 
                            ROW_NUMBER() OVER ( 
                                PARTITION BY r.Cell_Barcode 
                                ORDER BY r.`Date` DESC, r.Test_ID DESC 
                            ) AS rn 
                        FROM bts_copy.record r 
                        WHERE r.DataUploadTag = '1' 
                          AND r.Cell_Barcode IN ({inClause}) 
                    ) 
                    SELECT 
                        r.Cell_Barcode AS CellBarcode, 
                        r.seq_id       AS SeqId, 
                        r.Step_Name    AS StepName, 
                        r.DBCSig       AS DBCSig, 
                        r.Aux_Vmax     AS AuxVmax, 
                        r.Aux_Vmin     AS AuxVmin, 
                        r.AuxDiffVolt  AS AuxDiffVolt, 
                        r.Test_ID      AS TestId 
                    FROM bts_copy.record r 
                    JOIN latest l 
                      ON l.CellBarcode = r.Cell_Barcode 
                     AND l.TestId      = r.Test_ID 
                     AND l.rn          = 1 
                    WHERE r.Step_Name = 'cc_dchg' 
                    ORDER BY r.Cell_Barcode, r.seq_id;";
                var sqlParams = chunk.Select((barcode, i) => new MySqlParameter($"@p{i}", barcode)).ToArray();
                var rows = btsDb.Set<RecordSliceRow>()
                    .FromSqlRaw(sql, sqlParams)
                    .AsNoTracking()
                    .ToList();

                allRows.AddRange(rows);
            }
            return allRows;
        }

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
