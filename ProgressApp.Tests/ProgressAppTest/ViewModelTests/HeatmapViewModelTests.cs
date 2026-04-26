using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces.IService;
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
        var vm = new HeatmapViewModel(_analyticsMock.Object, _messageMock.Object);

        vm.CellSize.Should().Be(40);

        vm.SelectedRange = HeatmapRange.AllTime;

        vm.CellSize.Should().Be(13);
        _analyticsMock.Verify(a => a.GetHeatmapCells(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
    }

    [TestMethod]
    public async Task Navigation_ShouldBeDisabled_WhenNoFirstEntryDate()
    {
        _analyticsMock.Setup(a => a.GetFirstEntryDateAsync()).ReturnsAsync((DateTime?)null);

        var vm = new HeatmapViewModel(_analyticsMock.Object, _messageMock.Object);
        await Task.Delay(100);

        vm.PreviousPeriodCommand.CanExecute(null).Should().BeFalse();
    }
}
