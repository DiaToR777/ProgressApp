using Moq;
using FluentAssertions;
using ProgressApp.Core.Services;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.ViewModels.Today;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class TodayViewModelTests
    {
        private Mock<IJournalService> _serviceMock;
        private Mock<IMessageService> _messageMock;


        [TestInitialize]
        public void TestInit()
        {
            _serviceMock = new Mock<IJournalService>();
            _messageMock = new Mock<IMessageService>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // This method is called after each test method.
        }

        [TestMethod]
        public void LoadToday_WhenNoEntry_ShouldSetDefaultResult()
        {
            _serviceMock.Setup(s => s.GetToday()).Returns((JournalEntry)null);

            var vm = new TodayViewModel(_serviceMock.Object, _messageMock.Object);

            vm.SelectedResult.Should().Be(DayResult.Relapse);
        }

        [TestMethod]
        public void LoadToday_WhenEntryExists_ShouldPopulateProperties()
        {
            var mockEntry = new JournalEntry
            {
                Description = "Мій успішний день",
                Result = DayResult.Success
            };

            _serviceMock.Setup(s => s.GetToday()).Returns(mockEntry);
            var vm = new TodayViewModel(_serviceMock.Object, _messageMock.Object);

            vm.Description.Should().Be("Мій успішний день");
            vm.SelectedResult.Should().Be(DayResult.Success);
        }
    }
}
