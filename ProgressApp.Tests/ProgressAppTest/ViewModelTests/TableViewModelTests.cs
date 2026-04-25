using FluentAssertions;
using Moq;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.ViewModels.Analytics.Table;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class TableViewModelTests
    {
        private Mock<IJournalService> _serviceMock;
        private Mock<IMessageService> _messageServiceMock;

        [TestInitialize]
        public void TestInit()
        {
            _serviceMock = new Mock<IJournalService>();
            _messageServiceMock = new Mock<IMessageService>();
        }

        [TestMethod]
        public async Task GetEntries_WhenDataExists_ShouldShowTableAndHideEmptyState()
        {
            var testEntries = new List<JournalEntry>
            {
                 new JournalEntry { Id = 1, Description = "Day 1" }
            };
            _serviceMock.Setup(s => s.GetAllEntriesAsync()).ReturnsAsync(testEntries);

            var vm = new TableViewModel(_serviceMock.Object, _messageServiceMock.Object);
            await Task.Delay(50); 

            vm.Entries.Should().HaveCount(1);
            vm.ShowTable.Should().BeTrue();
            vm.ShowEmptyState.Should().BeFalse();
        }

        [TestMethod]
        public async Task GetEntries_WhenNoData_ShouldShowEmptyState()
        {
            _serviceMock.Setup(s => s.GetAllEntriesAsync()).ReturnsAsync(new List<JournalEntry>());

            var vm = new TableViewModel(_serviceMock.Object, _messageServiceMock.Object);
            await Task.Delay(50);

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
            await Task.Delay(50);

            _messageServiceMock.Verify(m => m.ShowErrorAsync(ex), Times.Once);
        }
    }
}
