using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OmniFrame.Common;

namespace OmniFrame.DataAccess
{
    public class AlarmRecord
    {
        public int Id { get; set; }
        public DateTime AlarmTime { get; set; }
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public string AlarmLevel { get; set; }
        public string Source { get; set; }
        public bool IsCleared { get; set; }
        public DateTime? ClearTime { get; set; }
        public string ClearUser { get; set; }
    }

    public class AlarmDb : IAlarmDb
    {
        private SqliteHelper _db;
        private readonly string _defaultDbPath = "Data/Alarms.db";

        public AlarmDb() : this(null)
        {
        }

        public AlarmDb(string dbPath)
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
                CreateAlarmTable();
            }
            return result;
        }

        public void Close()
        {
            _db.Close();
        }

        private bool CreateAlarmTable()
        {
            string columns = @"
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AlarmTime DATETIME NOT NULL,
                AlarmCode TEXT NOT NULL,
                AlarmMessage TEXT NOT NULL,
                AlarmLevel TEXT NOT NULL,
                Source TEXT,
                IsCleared INTEGER DEFAULT 0,
                ClearTime DATETIME,
                ClearUser TEXT
            ";
            return _db.CreateTable("AlarmHistory", columns);
        }

        public bool AddAlarm(AlarmRecord record)
        {
            string sql = @"
                INSERT INTO AlarmHistory (AlarmTime, AlarmCode, AlarmMessage, AlarmLevel, Source, IsCleared)
                VALUES (@AlarmTime, @AlarmCode, @AlarmMessage, @AlarmLevel, @Source, 0);
            ";
            return _db.Execute(sql, new
            {
                record.AlarmTime,
                record.AlarmCode,
                record.AlarmMessage,
                record.AlarmLevel,
                Source = record.Source ?? ""
            }) > 0;
        }

        public bool ClearAlarm(int alarmId, string clearUser)
        {
            string sql = @"
                UPDATE AlarmHistory
                SET IsCleared = 1, ClearTime = @ClearTime, ClearUser = @ClearUser
                WHERE Id = @Id;
            ";
            return _db.Execute(sql, new
            {
                ClearTime = DateTime.Now,
                ClearUser = clearUser,
                Id = alarmId
            }) > 0;
        }

        public DataTable GetAlarmHistory(DateTime startTime, DateTime endTime, string alarmLevel = null, bool? isCleared = null)
        {
            // 使用 Dapper 查询后转为 DataTable（IAlarmDb 接口要求 DataTable）
            string sql = "SELECT * FROM AlarmHistory WHERE AlarmTime BETWEEN @StartTime AND @EndTime";

            if (!string.IsNullOrEmpty(alarmLevel))
                sql += " AND AlarmLevel = @AlarmLevel";

            if (isCleared.HasValue)
                sql += " AND IsCleared = @IsCleared";

            sql += " ORDER BY AlarmTime DESC;";

            var records = _db.Query<AlarmRecord>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                AlarmLevel = alarmLevel,
                IsCleared = isCleared.HasValue ? (isCleared.Value ? 1 : 0) : (int?)null
            });

            return ToDataTable(records);
        }

        public DataTable GetActiveAlarms()
        {
            string sql = "SELECT * FROM AlarmHistory WHERE IsCleared = 0 ORDER BY AlarmTime DESC;";
            var records = _db.Query<AlarmRecord>(sql);
            return ToDataTable(records);
        }

        public int GetAlarmCount(DateTime startTime, DateTime endTime, string alarmLevel = null)
        {
            string sql = "SELECT COUNT(*) FROM AlarmHistory WHERE AlarmTime BETWEEN @StartTime AND @EndTime";

            if (!string.IsNullOrEmpty(alarmLevel))
                sql += " AND AlarmLevel = @AlarmLevel";

            return _db.ExecuteScalar<int>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                AlarmLevel = alarmLevel
            });
        }

        public bool ClearAllAlarms(string clearUser)
        {
            string sql = @"
                UPDATE AlarmHistory
                SET IsCleared = 1, ClearTime = @ClearTime, ClearUser = @ClearUser
                WHERE IsCleared = 0;
            ";
            return _db.Execute(sql, new
            {
                ClearTime = DateTime.Now,
                ClearUser = clearUser
            }) > 0;
        }

        private static DataTable ToDataTable(List<AlarmRecord> records)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("AlarmTime", typeof(DateTime));
            dt.Columns.Add("AlarmCode", typeof(string));
            dt.Columns.Add("AlarmMessage", typeof(string));
            dt.Columns.Add("AlarmLevel", typeof(string));
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("IsCleared", typeof(int));
            dt.Columns.Add("ClearTime", typeof(DateTime));
            dt.Columns.Add("ClearUser", typeof(string));

            foreach (var r in records)
            {
                dt.Rows.Add(
                    r.Id, r.AlarmTime, r.AlarmCode, r.AlarmMessage,
                    r.AlarmLevel, r.Source, r.IsCleared ? 1 : 0,
                    r.ClearTime.HasValue ? (object)r.ClearTime.Value : DBNull.Value,
                    r.ClearUser ?? (object)DBNull.Value);
            }
            return dt;
        }
    }
}
