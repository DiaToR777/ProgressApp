using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using ProgressApp.Core.Data;
using ProgressApp.Core.Services;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Settings;
using ProgressApp.Core.Exceptions;

namespace ProgressAppTest.ServiceTests
{
    [TestClass]
    public class SettingsServiceTests
    {
        private Mock<IServiceScopeFactory> _scopeFactoryMock;
        private Mock<IDbState> _dbStateMock;
        private ProgressDbContext _dbContext;
        private SqliteConnection _connection;
        private SettingsService _service;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ProgressDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbStateMock = new Mock<IDbState>();
            _dbStateMock.Setup(x => x.GetConnectionString(It.IsAny<string>())).Returns("DataSource=:memory:");

            _dbContext = new ProgressDbContext(options, _dbStateMock.Object);
            _dbContext.Database.EnsureCreated();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(ProgressDbContext))).Returns(_dbContext);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            _service = new SettingsService(_scopeFactoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
            _connection.Close();
        }

        [TestMethod]
        public async Task GetGoalAsync_WhenNoGoal_ReturnsEmptyString()
        {
            var result = await _service.GetGoalAsync();

            result.Should().Be("");
        }

        [TestMethod]
        public async Task SaveGoalAsync_ShouldCreateNewSetting_WhenNoneExists()
        {
            var myGoal = "Стать Senior .NET Developer";

            await _service.SaveGoalAsync(myGoal);

            var setting = await _dbContext.Settings.FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
            setting.Should().NotBeNull();
            setting!.Value.Should().Be(myGoal);
        }

        [TestMethod]
        public async Task SaveGoalAsync_ShouldUpdateExistingSetting_WhenAlreadyExists()
        {
            await _service.SaveGoalAsync("Old Goal");

            await _service.SaveGoalAsync("New Cool Goal");

            var allSettings = await _dbContext.Settings.ToListAsync();
            allSettings.Should().HaveCount(1); 
            allSettings[0].Value.Should().Be("New Cool Goal");
        }

        [TestMethod]
        public async Task SaveGoalAsync_EmptyGoal_ThrowsAppException()
        {
            Func<Task> act = async () => await _service.SaveGoalAsync("   ");

            await act.Should().ThrowAsync<AppException>()
                .Where(e => e.Message == "Msg_GoalEmpty");
        }

        [TestMethod]
        public async Task GetGoalAsync_ShouldReturnCorrectValue()
        {
            _dbContext.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = "Saved Success" });
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetGoalAsync();

            result.Should().Be("Saved Success");
        }
    }
}
