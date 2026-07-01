using FluentAssertions;
using Moq;
using ProgressApp.Application.Services;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;

namespace ProgressAppTest.ServiceTests;

[TestClass]
public class SettingsServiceTests
{
    private Mock<ISettingsRepository> _settingsRepositoryMock;
    private SettingsService _service;

    [TestInitialize]
    public void Setup()
    {
        _settingsRepositoryMock = new Mock<ISettingsRepository>();
        _service = new SettingsService(_settingsRepositoryMock.Object);
    }

    [TestMethod]
    public async Task SaveGoalAsync_EmptyGoal_ThrowsAppException()
    {
        Func<Task> act = async () => await _service.SaveGoalAsync("   ");

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.Message == "Msg_GoalEmpty");

        _settingsRepositoryMock.Verify(r => r.SaveGoalAsync(It.IsAny<string>()), Times.Never);
    }
}