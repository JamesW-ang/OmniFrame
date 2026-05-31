using NUnit.Framework;
using OmniFrame.Core;
using System.Linq;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class ProductionManagerTests
    {
        private ProductionManager _productionManager;

        [SetUp]
        public void Setup()
        {
            _productionManager = new ProductionManager();
        }

        [Test]
        public void LoadProductionOrders_PopulatesOrders()
        {
            var result = _productionManager.LoadProductionOrders();

            Assert.That(result, Is.True);
            Assert.That(_productionManager.GetProductionOrders().Count, Is.GreaterThan(0));
        }

        [Test]
        public void StartProductionOrder_ValidOrder_UpdatesStatus()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();

            var result = _productionManager.StartProductionOrder(order.OrderId);

            Assert.That(result, Is.True);
            Assert.That(order.Status, Is.EqualTo("生产中"));
            Assert.That(_productionManager.GetCurrentOrder(), Is.SameAs(order));
        }

        [Test]
        public void StartProductionOrder_InvalidOrder_ReturnsFalse()
        {
            _productionManager.LoadProductionOrders();

            var result = _productionManager.StartProductionOrder("NONEXISTENT");

            Assert.That(result, Is.False);
        }

        [Test]
        public void EndProductionOrder_ValidOrder_UpdatesStatus()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();
            _productionManager.StartProductionOrder(order.OrderId);

            var result = _productionManager.EndProductionOrder(order.OrderId);

            Assert.That(result, Is.True);
            Assert.That(order.Status, Is.EqualTo("已完成"));
            Assert.That(order.EndTime, Is.Not.Null);
        }

        [Test]
        public void EndProductionOrder_InvalidOrder_ReturnsFalse()
        {
            _productionManager.LoadProductionOrders();

            var result = _productionManager.EndProductionOrder("NONEXISTENT");

            Assert.That(result, Is.False);
        }

        [Test]
        public void RecordProductionData_WithoutActiveOrder_ReturnsFalse()
        {
            var result = _productionManager.RecordProductionData(true);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RecordProductionData_Pass_IncrementsPassQuantity()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();
            _productionManager.StartProductionOrder(order.OrderId);

            _productionManager.RecordProductionData(true);

            Assert.That(order.PassQuantity, Is.EqualTo(1));
            Assert.That(order.ActualQuantity, Is.EqualTo(1));
        }

        [Test]
        public void RecordProductionData_Fail_IncrementsFailQuantity()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();
            _productionManager.StartProductionOrder(order.OrderId);

            _productionManager.RecordProductionData(false);

            Assert.That(order.FailQuantity, Is.EqualTo(1));
            Assert.That(order.ActualQuantity, Is.EqualTo(1));
        }

        [Test]
        public void RecordProductionData_MultipleRecords_AccumulatesCorrectly()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();
            _productionManager.StartProductionOrder(order.OrderId);

            _productionManager.RecordProductionData(true);
            _productionManager.RecordProductionData(true);
            _productionManager.RecordProductionData(false);

            Assert.That(order.PassQuantity, Is.EqualTo(2));
            Assert.That(order.FailQuantity, Is.EqualTo(1));
            Assert.That(order.ActualQuantity, Is.EqualTo(3));
        }

        [Test]
        public void GetProductionStats_ReturnsCorrectYield()
        {
            _productionManager.LoadProductionOrders();
            var order = _productionManager.GetProductionOrders().First();
            _productionManager.StartProductionOrder(order.OrderId);

            _productionManager.RecordProductionData(true);
            _productionManager.RecordProductionData(true);
            _productionManager.RecordProductionData(false);

            var stats = _productionManager.GetProductionStats();

            Assert.That(stats.PassQuantity, Is.EqualTo(2));
            Assert.That(stats.FailQuantity, Is.EqualTo(1));
            Assert.That(stats.TotalQuantity, Is.EqualTo(3));
            Assert.That(stats.YieldRate, Is.EqualTo(2.0 / 3.0 * 100).Within(0.01));
        }

        [Test]
        public void GetCurrentOrder_WithoutStart_ReturnsNull()
        {
            _productionManager.LoadProductionOrders();

            var order = _productionManager.GetCurrentOrder();

            Assert.That(order, Is.Null);
        }

        [Test]
        public void GetProductionOrders_AfterLoad_ReturnsOrders()
        {
            _productionManager.LoadProductionOrders();

            var orders = _productionManager.GetProductionOrders();

            Assert.That(orders, Is.Not.Null);
            Assert.That(orders.Count, Is.GreaterThan(0));
            Assert.That(orders[0].OrderId, Is.Not.Null.And.Not.Empty);
        }
    }
}
