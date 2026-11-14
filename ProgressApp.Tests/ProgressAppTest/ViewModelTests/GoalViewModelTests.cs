using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels.Common;
using ProgressApp.WpfUI.ViewModels.Goal;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class GoalViewModelTests
{
    private Mock<IGoalService> _goalServiceMock = null!;
    private Mock<IAnalyticsService> _analyticsServiceMock = null!;
    private Mock<IMessageService> _messageServiceMock = null!;
    private readonly Goal _testGoal = CreateTestGoal();

    [TestInitialize]
    public void TestInit()
    {
        _goalServiceMock = new Mock<IGoalService>();
        _analyticsServiceMock = new Mock<IAnalyticsService>();
        _messageServiceMock = new Mock<IMessageService>();

        _goalServiceMock.Setup(s => s.GetActiveGoalAsync()).ReturnsAsync(_testGoal);
        _analyticsServiceMock.Setup(s => s.GetActiveMilestoneProgressAsync()).ReturnsAsync((MilestoneProgress?)null);
    }

    [TestMethod]
    public async Task Initialize_ShouldLoadActiveGoalAndUpdateSubViewModels()
    {
        var vm = await CreateAndInitVmAsync();

        vm.ActiveGoal.Should().Be(_testGoal);
        vm.HasActiveGoal.Should().BeTrue();
        vm.Actions.Items.Should().HaveCount(1);
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
    public async Task CreateGoal_ShouldFilterEmptyActions_AndResetForm()
    {
        var vm = await CreateAndInitVmAsync();
        vm.NewGoalTitle = "Learn C#";
        vm.NewMilestoneDays = 30;
        vm.NewActions.Add(new ActionInputViewModel { Title = "Read docs", TargetCountPerWeek = 5 });
        vm.NewActions.Add(new ActionInputViewModel { Title = "   ", TargetCountPerWeek = 7 });

        await ((IAsyncRelayCommand)vm.CreateGoalCommand).ExecuteAsync(null);

        _goalServiceMock.Verify(s => s.CreateGoalAsync(
            "Learn C#",
            null,
            30,
            It.Is<List<(string, int)>>(list => list.Count == 1 && list[0].Item1 == "Read docs")),
            Times.Once);

        vm.NewActions.Should().BeEmpty();
        vm.NewMilestoneDays.Should().Be(40); 
        
        vm.NewGoalTitle.Should().BeEmpty();
    }

    private async Task<GoalViewModel> CreateAndInitVmAsync()
    {
        var completionVm = new MilestoneCompletionViewModel(_goalServiceMock.Object, _messageServiceMock.Object);
        var actionsVm = new GoalActionsViewModel(_goalServiceMock.Object, _messageServiceMock.Object);

        var vm = new GoalViewModel(
            _goalServiceMock.Object,
            _analyticsServiceMock.Object,
            _messageServiceMock.Object,
            completionVm,
            actionsVm);

        await vm.Initialization;
        return vm;
    }

    private static Goal CreateTestGoal() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test Goal",
        Milestones = [new Milestone
        {
            Id = Guid.NewGuid(),
            Actions = [new GoalAction { Id = Guid.NewGuid(), Title = "Action 1", TargetCountPerWeek = 7 }]
        }]
    };
}