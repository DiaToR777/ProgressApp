using Moq;
using FluentAssertions;
using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.Core.Interfaces.IService;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class TodayViewModelTests
    {
        private Mock<IJournalService> _serviceMock;
        private Mock<IMessageService> _messageMock;
        private Mock<IAnalyticsService> _analyticsMock;

        [TestInitialize]
        public void TestInit()
        {
            _serviceMock = new Mock<IJournalService>();
            _messageMock = new Mock<IMessageService>();
            _analyticsMock = new Mock<IAnalyticsService>();

            _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(5);
        }

        [TestMethod]
        public async Task Initialize_ShouldLoadStreakAndSetDefaultResult_WhenNoEntry()
        {
            _serviceMock.Setup(s => s.GetTodayAsync()).ReturnsAsync((JournalEntry)null);

            var vm = CreateViewModel();
            await Task.Delay(100); 

            vm.CurrentStreak.Should().Be(5);
            vm.SelectedResult.Should().Be(DayResult.PartialSuccess);
            vm.Description.Should().BeEmpty();
        }

        [TestMethod]
        public async Task SaveCommand_ShouldUpdateStreakAfterSaving()
        {
            _serviceMock.Setup(s => s.GetTodayAsync()).ReturnsAsync((JournalEntry)null);
            _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(10); 

            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.Description = "Valid description";

            _analyticsMock.Setup(a => a.GetCurrentStreakAsync()).ReturnsAsync(11);

            vm.SaveCommand.Execute(null);
            await Task.Delay(100);

            _serviceMock.Verify(s => s.SaveTodayAsync("Valid description", It.IsAny<DayResult>()), Times.Once);
            vm.CurrentStreak.Should().Be(11); 
            _messageMock.Verify(m => m.ShowInfoAsync("Msg_RecordSaved"), Times.Once);
        }

        private TodayViewModel CreateViewModel()
        {
            return new TodayViewModel(_serviceMock.Object, _messageMock.Object, _analyticsMock.Object);
        }
    }
}
