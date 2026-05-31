using NUnit.Framework;
using OmniFrame.Core;
using OmniFrame.DataAccess;
using Moq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class PerformanceTests
    {
        private const int IterationCount = 1000;

        [Test]
        public void SqliteHelper_BulkInsertPerformance()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"PerfTest_{Guid.NewGuid():N}.db");
            try
            {
                var helper = new SqliteHelper(dbPath);
                helper.CreateTable("PerfData",
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, Value1 REAL, Value2 TEXT, Value3 INTEGER");

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < IterationCount; i++)
                {
                    helper.Execute("INSERT INTO PerfData (Value1, Value2, Value3) VALUES (@V1, @V2, @V3)",
                        new { V1 = i * 1.5, V2 = $"Item_{i}", V3 = i });
                }
                sw.Stop();

                var count = helper.ExecuteScalar<int>("SELECT COUNT(*) FROM PerfData");
                helper.Close();

                Assert.That(count, Is.EqualTo(IterationCount));
                Assert.That(sw.ElapsedMilliseconds, Is.LessThan(10000),
                    $"{IterationCount} 次插入耗时 {sw.ElapsedMilliseconds}ms（预期 < 10000ms）");
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Test]
        public void SqliteHelper_ConcurrentReadPerformance()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"ConcurPerf_{Guid.NewGuid():N}.db");
            try
            {
                var helper = new SqliteHelper(dbPath);
                helper.CreateTable("ConcurData",
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, Data TEXT");
                for (int i = 0; i < 100; i++)
                {
                    helper.Execute("INSERT INTO ConcurData (Data) VALUES (@D)",
                        new { D = $"Row_{i}" });
                }

                var sw = Stopwatch.StartNew();
                int threadCount = 4;
                var threads = new Thread[threadCount];
                int totalReads = 0;
                object counterLock = new object();

                for (int t = 0; t < threadCount; t++)
                {
                    threads[t] = new Thread(() =>
                    {
                        for (int i = 0; i < 250; i++)
                        {
                            using (var conn = helper.GetConnection())
                            {
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "SELECT COUNT(*) FROM ConcurData";
                                    cmd.ExecuteScalar();
                                }
                            }
                            lock (counterLock) { totalReads++; }
                        }
                    });
                    threads[t].Start();
                }

                foreach (var t in threads) t.Join();
                sw.Stop();

                Assert.That(totalReads, Is.EqualTo(threadCount * 250));
                Assert.That(sw.ElapsedMilliseconds, Is.LessThan(30000),
                    $"4 线程并发读取耗时 {sw.ElapsedMilliseconds}ms（预期 < 30000ms）");
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Test]
        public void ProductionManager_RecordPerformance()
        {
            var mgr = new ProductionManager();
            mgr.LoadProductionOrders();
            var orders = mgr.GetProductionOrders();
            mgr.StartProductionOrder(orders[0].OrderId);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < IterationCount; i++)
            {
                mgr.RecordProductionData(i % 2 == 0);
            }
            sw.Stop();

            var stats = mgr.GetProductionStats();
            Assert.That(stats.TotalQuantity, Is.EqualTo(IterationCount));
            int expectedPass = IterationCount / 2 + (IterationCount % 2 == 0 ? 0 : 1);
            Assert.That(stats.PassQuantity, Is.EqualTo(expectedPass));
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000),
                $"{IterationCount} 次数据记录耗时 {sw.ElapsedMilliseconds}ms（预期 < 2000ms）");
        }

        [Test]
        public void UserManager_LoginPerformance()
        {
            var userMgr = new UserManager();
            var users = userMgr._users;
            var testUser = new UserInfo
            {
                UserId = "perf_user",
                UserName = "Perf User",
                Level = UserLevel.Operator,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                IsActive = true
            };
            users.Add(testUser);

            var sw = Stopwatch.StartNew();
            int loginCount = 50;
            for (int i = 0; i < loginCount; i++)
            {
                users[0].IsActive = true;
                if (i > 0)
                {
                    var field = typeof(UserManager).GetField("_currentUser",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    field.SetValue(userMgr, null);
                }
                var result = userMgr.Login("perf_user", "testpassword");
                Assert.That(result.Success, Is.True);
            }
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(15000),
                $"{loginCount} 次登录耗时 {sw.ElapsedMilliseconds}ms（含 bcrypt 哈希，预期 < 15000ms）");
        }
    }
}
