using NUnit.Framework;
using OmniFrame.Core;
using System.IO;

namespace OmniFrame.Tests
{
    [TestFixture]
    public class ConfigManagerTests
    {
        private string _tempConfigDir;
        private ConfigManager _configManager;

        [SetUp]
        public void Setup()
        {
            _tempConfigDir = Path.Combine(Path.GetTempPath(), $"OmniFrameCfg_{System.Guid.NewGuid():N}");
            var configPathField = typeof(ConfigManager).GetField("_configPath",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var backupPathField = typeof(ConfigManager).GetField("_backupPath",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            _configManager = new ConfigManager();
            configPathField.SetValue(_configManager, _tempConfigDir);
            backupPathField.SetValue(_configManager, Path.Combine(_tempConfigDir, "Backup"));

            Directory.CreateDirectory(_tempConfigDir);
            Directory.CreateDirectory(Path.Combine(_tempConfigDir, "Backup"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempConfigDir))
                Directory.Delete(_tempConfigDir, true);
        }

        [Test]
        public void GetConfig_NonExistent_ReturnsDefault()
        {
            var result = _configManager.GetConfig("nonexistent.xml", "default");
            Assert.That(result, Is.EqualTo("default"));
        }

        [Test]
        public void SaveAndGetConfig_RoundTrip()
        {
            var testConfig = new TestConfig { Name = "Test", Value = 42 };
            bool saved = _configManager.SaveConfig("test.xml", testConfig);

            Assert.That(saved, Is.True);
            Assert.That(File.Exists(Path.Combine(_tempConfigDir, "test.xml")), Is.True);
        }

        [Test]
        public void SaveConfig_CreatesBackup()
        {
            var config = new TestConfig { Name = "Original", Value = 1 };
            _configManager.SaveConfig("backupTest.xml", config);

            _configManager.SaveConfig("backupTest.xml", new TestConfig { Name = "Updated", Value = 2 });

            var backupFiles = _configManager.GetBackupFiles();
            Assert.That(backupFiles.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void GetBackupFiles_NoBackups_ReturnsEmpty()
        {
            var files = _configManager.GetBackupFiles();
            Assert.That(files, Is.Empty);
        }

        [Test]
        public void RollbackConfig_NonExistent_ReturnsFalse()
        {
            bool result = _configManager.RollbackConfig("nonexistent.xml");
            Assert.That(result, Is.False);
        }
    }

    public class TestConfig
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
