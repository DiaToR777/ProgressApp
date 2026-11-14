using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Goal;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class MilestoneCompletionViewModelTests
{
    private Mock<IGoalService> _goalServiceMock = null!;
    private Mock<IMessageService> _messageServiceMock = null!;
    private MilestoneCompletionViewModel _vm = null!;
    private readonly Guid _milestoneId = Guid.NewGuid();

    [TestInitialize]
    public void TestInit()
    {
        _goalServiceMock = new Mock<IGoalService>();
        _messageServiceMock = new Mock<IMessageService>();
        _vm = new MilestoneCompletionViewModel(_goalServiceMock.Object, _messageServiceMock.Object);
        _vm.Update(new MilestoneProgress { MilestoneId = _milestoneId });
    }

    [TestMethod]
    public async Task ExtendMilestone_WhenConfirmed_ShouldExtendAndNotify()
    {
        var changedCalled = false;
        _vm.Changed = () => { changedCalled = true; return Task.CompletedTask; };
        _vm.ExtendDays = 14;
        _messageServiceMock.Setup(m => m.ShowConfirmationAsync("Msg_ConfirmExtendMilestone")).ReturnsAsync(true);

        await ((IAsyncRelayCommand)_vm.ExtendMilestoneCommand).ExecuteAsync(null);

        _goalServiceMock.Verify(s => s.ExtendMilestoneAsync(_milestoneId, 14), Times.Once);
        changedCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task ExtendMilestone_WhenCancelled_ShouldDoNothing()
    {
        _messageServiceMock.Setup(m => m.ShowConfirmationAsync("Msg_ConfirmExtendMilestone")).ReturnsAsync(false);

        await ((IAsyncRelayCommand)_vm.ExtendMilestoneCommand).ExecuteAsync(null);

        _goalServiceMock.Verify(s => s.ExtendMilestoneAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }
}