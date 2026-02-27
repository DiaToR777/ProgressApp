using FluentAssertions;
using Moq;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.ViewModels.Table;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class TableViewModelTests
    {
        private Mock<IJournalService> _serviceMock;

        [TestInitialize]
        public void TestInit()
        {
            _serviceMock = new Mock<IJournalService>();
        }

        [TestMethod]
        public void Constructor_ShouldPopulateEntriesCollection()
        {
            var testEntries = new List<JournalEntry>
            {
                  new JournalEntry { Id = 1, Date = DateTime.Today, Description = "Entry 1" },
                  new JournalEntry { Id = 2, Date = DateTime.Today.AddDays(-1), Description = "Entry 2" }
            };

            _serviceMock.Setup(s => s.GetAllEntries()).Returns(testEntries);

            var vm = new TableViewModel(_serviceMock.Object);

            vm.Entries.Should().HaveCount(2);
            vm.Entries.Should().ContainSingle(e => e.Description == "Entry 1");
        }
    }
}
