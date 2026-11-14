using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Goal;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class GoalActionsViewModelTests
{
    private Mock<IGoalService> _goalServiceMock = null!;
    private Mock<IMessageService> _messageServiceMock = null!;
    private GoalActionsViewModel _vm = null!;
    private readonly Guid _milestoneId = Guid.NewGuid();
    private readonly GoalAction _existingAction = new() { Id = Guid.NewGuid(), Title = "Action 1", TargetCountPerWeek = 7 };

    [TestInitialize]
    public void TestInit()
    {
        _goalServiceMock = new Mock<IGoalService>();
        _messageServiceMock = new Mock<IMessageService>();
        _vm = new GoalActionsViewModel(_goalServiceMock.Object, _messageServiceMock.Object);

        _vm.Update(new Goal
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Milestones = [new Milestone { Id = _milestoneId, Actions = [_existingAction] }]
        });
    }

    [TestMethod]
    public async Task SaveAction_ShouldAddAndUpdateAction_AndResetFields()
    {
        var changedCalled = false;
        _vm.Changed = () => { changedCalled = true; return Task.CompletedTask; };

        _vm.NewActionTitle = "New Action";
        await ((IAsyncRelayCommand)_vm.SaveActionCommand).ExecuteAsync(null);
        _goalServiceMock.Verify(s => s.AddActionAsync(_milestoneId, "New Action", 7), Times.Once);
        changedCalled.Should().BeTrue();

        _vm.StartEditActionCommand.Execute(_existingAction);
        _vm.NewActionTitle = "Updated Action";
        await ((IAsyncRelayCommand)_vm.SaveActionCommand).ExecuteAsync(null);

        _goalServiceMock.Verify(s => s.UpdateActionAsync(_existingAction.Id, "Updated Action", 7), Times.Once);
        _vm.EditingAction.Should().BeNull();
    }

    [TestMethod]
    public async Task RemoveAction_WhenConfirmed_ShouldRemove()
    {
        _messageServiceMock.Setup(m => m.ShowConfirmationAsync("Msg_ConfirmRemoveAction")).ReturnsAsync(true);

        await ((IAsyncRelayCommand)_vm.RemoveActionCommand).ExecuteAsync(_existingAction);

        _goalServiceMock.Verify(s => s.RemoveActionAsync(_existingAction.Id), Times.Once);
    }

    [TestMethod]
    public async Task RemoveAction_WhenCancelled_ShouldNotRemove()
    {
        _messageServiceMock.Setup(m => m.ShowConfirmationAsync("Msg_ConfirmRemoveAction")).ReturnsAsync(false);

        await ((IAsyncRelayCommand)_vm.RemoveActionCommand).ExecuteAsync(_existingAction);

        _goalServiceMock.Verify(s => s.RemoveActionAsync(It.IsAny<Guid>()), Times.Never);
    }
}