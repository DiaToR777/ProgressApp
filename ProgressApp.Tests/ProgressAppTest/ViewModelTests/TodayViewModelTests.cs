using CommunityToolkit.Mvvm.Input;
using Moq;
using FluentAssertions;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class TodayViewModelTests
{
    private Mock<ICheckinService> _serviceMock;
    private Mock<IMessageService> _messageMock;
    private Mock<IAnalyticsService> _analyticsMock;

    [TestInitialize]
    public void TestInit()
    {
        _serviceMock = new Mock<ICheckinService>();
        _messageMock = new Mock<IMessageService>();
        _analyticsMock = new Mock<IAnalyticsService>();
        _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(5);
        _serviceMock.Setup(s => s.GetActiveActionsAsync()).ReturnsAsync(new List<GoalAction>());
    }

    [TestMethod]
    public async Task Initialize_ShouldLoadStreakAndSetDefaultResult_WhenNoEntry()
    {
        _serviceMock.Setup(s => s.GetTodayAsync()).ReturnsAsync((DailyCheckin)null);
        var vm = CreateViewModel();
        
        await vm.Initialization;
        vm.CurrentStreak.Should().Be(5);
        vm.SelectedResult.Should().Be(DayResult.PartialSuccess);
        vm.Description.Should().BeEmpty();
    }

    [TestMethod]
    public async Task SaveCommand_ShouldUpdateStreakAfterSaving()
    {
        _serviceMock.Setup(s => s.GetTodayAsync()).ReturnsAsync((DailyCheckin)null);
        _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(10);
        var vm = CreateViewModel();

        await vm.Initialization;

        vm.Description = "Valid description";

        _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(11);
        await ((IAsyncRelayCommand)vm.SaveCommand).ExecuteAsync(null);
        
        _serviceMock.Verify(s => s.SaveTodayAsync("Valid description", It.IsAny<DayResult>(), It.IsAny<Dictionary<Guid, bool>>()), Times.Once);
        vm.CurrentStreak.Should().Be(11);
        _messageMock.Verify(m => m.ShowInfoAsync("Msg_RecordSaved"), Times.Once);
    }

    private TodayViewModel CreateViewModel()
    {
        return new TodayViewModel(_serviceMock.Object, _messageMock.Object, _analyticsMock.Object);
    }
}