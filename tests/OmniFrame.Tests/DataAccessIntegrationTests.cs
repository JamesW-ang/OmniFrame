using NUnit.Framework;
using OmniFrame.DataAccess;
using System;
using System.Data;
using System.IO;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class DataAccessIntegrationTests
    {
        private string _dbPath;
        private SqliteHelper _helper;

        [SetUp]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"OmniFrameTest_{Guid.NewGuid():N}.db");
            _helper = new SqliteHelper(_dbPath);
            _helper.Open();
        }

        [TearDown]
        public void TearDown()
        {
            _helper.Close();
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        [Test]
        public void CreateTable_CreatesTable()
        {
            string columns = "Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Value REAL";
            bool result = _helper.CreateTable("TestTable", columns);

            Assert.That(result, Is.True);
            Assert.That(_helper.TableExists("TestTable"), Is.True);
        }

        [Test]
        public void CreateTable_ExistingTable_DoesNotThrow()
        {
            _helper.CreateTable("DupTable", "Id INTEGER PRIMARY KEY");
            bool result = _helper.CreateTable("DupTable", "Id INTEGER PRIMARY KEY");

            Assert.That(result, Is.True);
        }

        [Test]
        public void InsertAndQuery_SingleRow()
        {
            _helper.CreateTable("Products", "Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Price REAL");
            _helper.Execute("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)",
                new { Name = "Widget", Price = 9.99 });

            var results = _helper.Query<ProductTestRow>("SELECT * FROM Products WHERE Name = @Name",
                new { Name = "Widget" });

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo("Widget"));
            Assert.That(results[0].Price, Is.EqualTo(9.99));
        }

        [Test]
        public void InsertAndQuery_MultipleRows()
        {
            _helper.CreateTable("Orders", "Id INTEGER PRIMARY KEY AUTOINCREMENT, Item TEXT, Qty INTEGER");
            for (int i = 1; i <= 5; i++)
            {
                _helper.Execute("INSERT INTO Orders (Item, Qty) VALUES (@Item, @Qty)",
                    new { Item = $"Item{i}", Qty = i * 10 });
            }

            var results = _helper.Query<OrderRow>("SELECT * FROM Orders ORDER BY Id");

            Assert.That(results.Count, Is.EqualTo(5));
            Assert.That(results[4].Item, Is.EqualTo("Item5"));
            Assert.That(results[4].Qty, Is.EqualTo(50));
        }

        [Test]
        public void QueryDataTable_ReturnsDataTable()
        {
            _helper.CreateTable("Events", "Id INTEGER PRIMARY KEY AUTOINCREMENT, EventTime DATETIME, Level TEXT");
            _helper.Execute("INSERT INTO Events (EventTime, Level) VALUES (@Time, @Level)",
                new { Time = DateTime.Now, Level = "Info" });

            DataTable dt = _helper.QueryDataTable("SELECT * FROM Events");

            Assert.That(dt, Is.Not.Null);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0]["Level"], Is.EqualTo("Info"));
        }

        [Test]
        public void ExecuteScalar_ReturnsSingleValue()
        {
            _helper.CreateTable("Counter", "Id INTEGER PRIMARY KEY, Val INTEGER");
            _helper.Execute("INSERT INTO Counter VALUES (1, 42)");

            int result = _helper.ExecuteScalar<int>("SELECT Val FROM Counter WHERE Id = 1");

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void ExecuteInTransaction_Commit_PersistsData()
        {
            _helper.CreateTable("TxTest", "Id INTEGER PRIMARY KEY, Data TEXT");

            bool txResult = _helper.ExecuteInTransaction(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TxTest VALUES (1, 'committed')";
                    cmd.ExecuteNonQuery();
                }
            });

            Assert.That(txResult, Is.True);
            var rows = _helper.Query<object>("SELECT * FROM TxTest");
            Assert.That(rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteInTransaction_Rollback_OnFailure()
        {
            _helper.CreateTable("TxRollback", "Id INTEGER PRIMARY KEY, Data TEXT");

            bool txResult = _helper.ExecuteInTransaction(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TxRollback VALUES (1, 'data')";
                    cmd.ExecuteNonQuery();
                }
                throw new InvalidOperationException("force rollback");
            });

            Assert.That(txResult, Is.False);
            var rows = _helper.Query<object>("SELECT * FROM TxRollback");
            Assert.That(rows.Count, Is.EqualTo(0));
        }

        [Test]
        public void TableExists_NonExistent_ReturnsFalse()
        {
            bool exists = _helper.TableExists("NonExistentTable_" + Guid.NewGuid().ToString("N"));
            Assert.That(exists, Is.False);
        }

        public class ProductTestRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
        }

        public class OrderRow
        {
            public int Id { get; set; }
            public string Item { get; set; }
            public int Qty { get; set; }
        }
    }
}
