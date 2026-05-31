using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using OmniFrame.Common;

namespace OmniFrame.DataAccess
{
    public class ProductRecord
    {
        public int Id { get; set; }
        public DateTime ProductTime { get; set; }
        public string SerialNumber { get; set; }
        public string ProductModel { get; set; }
        public string WorkOrder { get; set; }
        public string Station { get; set; }
        public bool Result { get; set; }
        public string DefectCode { get; set; }
        public double CycleTime { get; set; }
        public string Operator { get; set; }
    }

    public class ProductDb : IProductDb
    {
        private SqliteHelper _db;
        private readonly string _defaultDbPath = "Data/Products.db";

        public ProductDb() : this(null)
        {
        }

        public ProductDb(string dbPath)
        {
            string path = string.IsNullOrEmpty(dbPath) ? _defaultDbPath : dbPath;
            // 确保目录存在
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            _db = new SqliteHelper(path);
        }

        public bool Open(string dbPath = null)
        {
            if (!string.IsNullOrEmpty(dbPath))
            {
                _db = new SqliteHelper(dbPath);
            }
            bool result = _db.Open();
            if (result)
            {
                CreateProductTable();
            }
            return result;
        }

        public void Close()
        {
            _db.Close();
        }

        private bool CreateProductTable()
        {
            string columns = @"
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductTime DATETIME NOT NULL,
                SerialNumber TEXT NOT NULL,
                ProductModel TEXT,
                WorkOrder TEXT,
                Station TEXT,
                Result INTEGER NOT NULL,
                DefectCode TEXT,
                CycleTime REAL,
                Operator TEXT
            ";
            return _db.CreateTable("ProductHistory", columns);
        }

        public bool AddProduct(ProductRecord record)
        {
            string sql = @"
                INSERT INTO ProductHistory (ProductTime, SerialNumber, ProductModel, WorkOrder, Station, Result, DefectCode, CycleTime, Operator)
                VALUES (@ProductTime, @SerialNumber, @ProductModel, @WorkOrder, @Station, @Result, @DefectCode, @CycleTime, @Operator);
            ";
            return _db.Execute(sql, new
            {
                record.ProductTime,
                record.SerialNumber,
                ProductModel = record.ProductModel ?? "",
                WorkOrder = record.WorkOrder ?? "",
                Station = record.Station ?? "",
                Result = record.Result ? 1 : 0,
                DefectCode = record.DefectCode ?? "",
                record.CycleTime,
                Operator = record.Operator ?? ""
            }) > 0;
        }

        public DataTable GetProductHistory(DateTime startTime, DateTime endTime, string workOrder = null, bool? result = null)
        {
            string sql = "SELECT * FROM ProductHistory WHERE ProductTime BETWEEN @StartTime AND @EndTime";

            if (!string.IsNullOrEmpty(workOrder))
                sql += " AND WorkOrder = @WorkOrder";

            if (result.HasValue)
                sql += " AND Result = @Result";

            sql += " ORDER BY ProductTime DESC;";

            var records = _db.Query<ProductRecord>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                WorkOrder = workOrder,
                Result = result.HasValue ? (result.Value ? 1 : 0) : (int?)null
            });

            return ToDataTable(records);
        }

        public int GetProductCount(DateTime startTime, DateTime endTime, bool? result = null)
        {
            string sql = "SELECT COUNT(*) FROM ProductHistory WHERE ProductTime BETWEEN @StartTime AND @EndTime";

            if (result.HasValue)
                sql += " AND Result = @Result";

            return _db.ExecuteScalar<int>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                Result = result.HasValue ? (result.Value ? 1 : 0) : (int?)null
            });
        }

        public double GetAverageCycleTime(DateTime startTime, DateTime endTime)
        {
            string sql = "SELECT AVG(CycleTime) FROM ProductHistory WHERE ProductTime BETWEEN @StartTime AND @EndTime AND Result = 1;";
            return _db.ExecuteScalar<double?>(sql, new { StartTime = startTime, EndTime = endTime }) ?? 0;
        }

        public double GetYieldRate(DateTime startTime, DateTime endTime)
        {
            int total = GetProductCount(startTime, endTime);
            if (total == 0)
                return 0;

            int pass = GetProductCount(startTime, endTime, true);
            return (double)pass / total * 100;
        }

        public DataTable GetYieldByHour(DateTime date)
        {
            string sql = @"
                SELECT
                    strftime('%H', ProductTime) as Hour,
                    COUNT(*) as TotalCount,
                    SUM(CASE WHEN Result = 1 THEN 1 ELSE 0 END) as PassCount,
                    SUM(CASE WHEN Result = 0 THEN 1 ELSE 0 END) as FailCount,
                    CAST(SUM(CASE WHEN Result = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS REAL) as YieldRate
                FROM ProductHistory
                WHERE date(ProductTime) = date(@Date)
                GROUP BY strftime('%H', ProductTime)
                ORDER BY Hour;
            ";
            var rows = _db.Query<ProductDb>(sql, new { Date = date });
            return ToDataTable(null);
        }

        public List<ProductRecord> GetProductsByTime(DateTime startTime, DateTime endTime)
        {
            string sql = "SELECT * FROM ProductHistory WHERE ProductTime BETWEEN @StartTime AND @EndTime ORDER BY ProductTime DESC;";
            return _db.Query<ProductRecord>(sql, new { StartTime = startTime, EndTime = endTime });
        }

        public ProductRecord GetProductBySerialNumber(string serialNumber)
        {
            string sql = "SELECT * FROM ProductHistory WHERE SerialNumber = @SerialNumber ORDER BY ProductTime DESC LIMIT 1;";
            return _db.Query<ProductRecord>(sql, new { SerialNumber = serialNumber }).FirstOrDefault();
        }

        public List<ProductRecord> GetAllProducts()
        {
            string sql = "SELECT * FROM ProductHistory ORDER BY ProductTime DESC LIMIT 1000;";
            return _db.Query<ProductRecord>(sql);
        }

        private static DataTable ToDataTable(List<ProductRecord> records)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("ProductTime", typeof(DateTime));
            dt.Columns.Add("SerialNumber", typeof(string));
            dt.Columns.Add("ProductModel", typeof(string));
            dt.Columns.Add("WorkOrder", typeof(string));
            dt.Columns.Add("Station", typeof(string));
            dt.Columns.Add("Result", typeof(int));
            dt.Columns.Add("DefectCode", typeof(string));
            dt.Columns.Add("CycleTime", typeof(double));
            dt.Columns.Add("Operator", typeof(string));

            if (records == null)
                return dt;

            foreach (var r in records)
            {
                dt.Rows.Add(
                    r.Id, r.ProductTime, r.SerialNumber,
                    r.ProductModel ?? "", r.WorkOrder ?? "",
                    r.Station ?? "", r.Result ? 1 : 0,
                    r.DefectCode ?? "", r.CycleTime,
                    r.Operator ?? "");
            }
            return dt;
        }
    }
}
