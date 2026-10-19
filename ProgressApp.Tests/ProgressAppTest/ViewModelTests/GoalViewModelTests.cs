using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Goal;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class GoalViewModelTests
{
    private Mock<IGoalService> _goalServiceMock = null!;
    private Mock<ICheckinService> _checkinServiceMock = null!;
    private Mock<IMessageService> _messageServiceMock = null!;
    private readonly Goal _testGoal = CreateTestGoal();

    [TestInitialize]
    public void TestInit()
    {
        _goalServiceMock = new Mock<IGoalService>();
        _checkinServiceMock = new Mock<ICheckinService>();
        _messageServiceMock = new Mock<IMessageService>();
        _goalServiceMock.Setup(s => s.GetActiveGoalAsync()).ReturnsAsync(_testGoal);
    }

    [TestMethod]
    public async Task Initialize_ShouldLoadActiveGoalData()
    {
        var vm = await CreateAndInitVmAsync();
        vm.ActiveGoal.Should().Be(_testGoal);
        vm.CurrentActions.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task SaveGoal_ShouldUpdateAndCloseEdit_WhenValid()
    {
        var vm = await CreateAndInitVmAsync();
        vm.StartEditGoalCommand.Execute(null);
        vm.EditGoalTitle = "New Title";

        await ((IAsyncRelayCommand)vm.SaveGoalCommand).ExecuteAsync(null);

        _goalServiceMock.Verify(s => s.UpdateGoalAsync(_testGoal.Id, "New Title", null), Times.Once);
        vm.IsEditingGoal.Should().BeFalse();
    }

    [TestMethod]
    public async Task SaveAction_ShouldAddOrUpdateAction_AndResetFields()
    {
        var vm = await CreateAndInitVmAsync();
        var milestoneId = _testGoal.Milestones.First().Id;

        vm.NewActionTitle = "New Action";
        await ((IAsyncRelayCommand)vm.SaveActionCommand).ExecuteAsync(null);
        _goalServiceMock.Verify(s => s.AddActionAsync(milestoneId, "New Action", 7), Times.Once);

        var existingAction = _testGoal.Milestones.First().Actions.First();
        vm.StartEditActionCommand.Execute(existingAction);
        vm.NewActionTitle = "Updated Action";
        await ((IAsyncRelayCommand)vm.SaveActionCommand).ExecuteAsync(null);
        
        _goalServiceMock.Verify(s => s.UpdateActionAsync(existingAction.Id, "Updated Action", 7), Times.Once);
        vm.EditingAction.Should().BeNull();
    }

    [TestMethod]
    public async Task RemoveAction_WhenConfirmed_ShouldRemove()
    {
        var vm = await CreateAndInitVmAsync();
        var action = _testGoal.Milestones.First().Actions.First();
        _messageServiceMock.Setup(m => m.ShowConfirmationAsync(It.IsAny<string>(), action.Title)).ReturnsAsync(true);

        await ((IAsyncRelayCommand)vm.RemoveActionCommand).ExecuteAsync(action);

        _goalServiceMock.Verify(s => s.RemoveActionAsync(action.Id), Times.Once);
    }

    private async Task<GoalViewModel> CreateAndInitVmAsync()
    {
        var vm = new GoalViewModel(_goalServiceMock.Object, _checkinServiceMock.Object, _messageServiceMock.Object);
        await vm.Initialization;
        return vm;
    }

    private static Goal CreateTestGoal() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test Goal",
        Milestones = [new Milestone { Id = Guid.NewGuid(), Actions = [new GoalAction { Id = Guid.NewGuid(), Title = "Action 1", TargetCountPerWeek = 7 }] }]
    };
}