using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Heatmap;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class HeatmapViewModelTests
{
    private Mock<IAnalyticsService> _analyticsMock;
    private Mock<IMessageService> _messageMock;

    [TestInitialize]
    public void TestInit()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        _messageMock = new Mock<IMessageService>();
    }

    [TestMethod]
    public async Task SelectedRange_Change_ShouldTriggerLoadAndChangeCellSize()
    {
        _analyticsMock.Setup(a => a.GetHeatmapCells(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<DayCell>());

        _analyticsMock.Setup(a => a.GetFirstEntryDateAsync())
                      .ReturnsAsync(DateTime.Today);

        var vm = new HeatmapViewModel(_analyticsMock.Object, _messageMock.Object);

        vm.CellSize.Should().Be(40);

        await vm.LoadAsync(HeatmapRange.AllTime);

        vm.SelectedRange = HeatmapRange.AllTime;

        vm.CellSize.Should().Be(13);
        _analyticsMock.Verify(a => a.GetHeatmapCells(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
    }
    [TestMethod]
    public async Task Navigation_ShouldBeDisabled_WhenNoFirstEntryDate()
    {
        _analyticsMock.Setup(a => a.GetFirstEntryDateAsync()).ReturnsAsync((DateTime?)null);

        _analyticsMock.Setup(a => a.GetHeatmapCells(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<DayCell>());

        var vm = new HeatmapViewModel(_analyticsMock.Object, _messageMock.Object);
        await vm.InitializeAsync();

        vm.PreviousPeriodCommand.CanExecute(null).Should().BeFalse();
    }
}
