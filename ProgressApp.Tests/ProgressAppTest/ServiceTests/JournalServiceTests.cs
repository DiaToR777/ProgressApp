using FluentAssertions;
using Moq;
using ProgressApp.Application.Services;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Journal;

namespace ProgressAppTest.ServiceTests;

[TestClass]
public class JournalServiceTests
{
    private Mock<IJournalRepository> _journalRepositoryMock;
    private JournalService _service;

    [TestInitialize]
    public void Setup()
    {
        _journalRepositoryMock = new Mock<IJournalRepository>();
        _service = new JournalService(_journalRepositoryMock.Object);
    }


    [TestMethod]
    public async Task SaveTodayAsync_EmptyDescription_ThrowsAppException()
    {
        Func<Task> act = async () => await _service.SaveTodayAsync("", DayResult.Success);

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.Message == "Msg_DescriptionEmpty");

        _journalRepositoryMock.Verify(r =>
            r.SaveTodayAsync(It.IsAny<string>(), It.IsAny<DayResult>()), Times.Never);
    }
}