using FluentAssertions;
using Moq;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Analytics.Table;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class TableViewModelTests
{
    private Mock<ICheckinService> _serviceMock;
    private Mock<IMessageService> _messageServiceMock;

    [TestInitialize]
    public void TestInit()
    {
        _serviceMock = new Mock<ICheckinService>();
        _messageServiceMock = new Mock<IMessageService>();
    }

    [TestMethod]
    public async Task GetEntries_WhenDataExists_ShouldShowTableAndHideEmptyState()
    {
        var testEntries = new List<DailyCheckin>
        {
            new DailyCheckin()
            {
                Id = Guid.NewGuid(),
                MilestoneId = Guid.NewGuid(),
                Description = "Day 1",
                Result = DayResult.Success,
                Date = DateTime.Now,
            }
        };
        _serviceMock.Setup(s => s.GetAllEntriesAsync()).ReturnsAsync(testEntries);
        var vm = new TableViewModel(_serviceMock.Object, _messageServiceMock.Object);
        await vm.Initialization;
        vm.Entries.Should().HaveCount(1);
        vm.ShowTable.Should().BeTrue();
        vm.ShowEmptyState.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetEntries_WhenNoData_ShouldShowEmptyState()
    {
        _serviceMock.Setup(s => s.GetAllEntriesAsync()).ReturnsAsync(new List<DailyCheckin>());
        var vm = new TableViewModel(_serviceMock.Object, _messageServiceMock.Object);
        await vm.Initialization;
        vm.Entries.Should().BeEmpty();
        vm.ShowTable.Should().BeFalse();
        vm.ShowEmptyState.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetEntries_WhenServiceFails_ShouldShowError()
    {
        var ex = new AppException("DB Error", isCritical: true, "Title");
        _serviceMock.Setup(s => s.GetAllEntriesAsync()).ThrowsAsync(ex);
        var vm = new TableViewModel(_serviceMock.Object, _messageServiceMock.Object);
        _messageServiceMock.Verify(m => m.ShowErrorAsync(ex), Times.Once);
    }
}