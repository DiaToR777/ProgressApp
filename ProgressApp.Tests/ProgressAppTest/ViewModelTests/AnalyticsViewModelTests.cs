using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.WpfUI.ViewModels.Analytics;
using ProgressApp.WpfUI.ViewModels.Analytics.Enums;
using ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class AnalyticsViewModelTests
{
    private Mock<IServiceProvider> _spMock;

    [TestInitialize]
    public void TestInit()
    {
        _spMock = new Mock<IServiceProvider>();
    }

    [TestMethod]
    public async Task SelectedViewOption_ChangeToHeatmap_ShouldSwitchView()
    {
        var anaMock = new Mock<IAnalyticsService>();
        var msgMock = new Mock<IMessageService>();

        anaMock.Setup(a => a.GetFirstEntryDateAsync()).ReturnsAsync(DateTime.Today);
        anaMock.Setup(a => a.GetHeatmapCells(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
               .ReturnsAsync(new List<DayCell>());

        var heatmapVm = new HeatmapViewModel(anaMock.Object, msgMock.Object);

        _spMock.Setup(sp => sp.GetService(typeof(HeatmapViewModel)))
               .Returns(heatmapVm);

        var vm = new AnalyticsViewModel(_spMock.Object);

        vm.SelectedViewOption = AnalyticsMode.Heatmap;

        await Task.Delay(300);

        vm.CurrentAnalyticsView.Should().NotBeNull("потому что UpdateView должен был завершиться");
        vm.CurrentAnalyticsView.Should().BeSameAs(heatmapVm);
    }
}