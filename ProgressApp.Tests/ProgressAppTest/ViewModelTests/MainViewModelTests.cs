using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Enums;
using ProgressApp.Infrastructure.Configuration;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.ViewModels;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Login;
using ProgressApp.WpfUI.ViewModels.Today;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class MainViewModelTests
{
    private Mock<IAuthService> _authMock;
    private Mock<IServiceProvider> _spMock;

    [TestInitialize]
    public void TestInit()
    {
        _authMock = new Mock<IAuthService>();
        _spMock = new Mock<IServiceProvider>();
    }

    [TestMethod]
    public async Task Initialize_WhenDbEncrypted_ShouldShowLoginAndHideNavigation()
    {
        _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.Encrypted);
        var loginVm = new LoginViewModel(_authMock.Object, new Mock<IMessageService>().Object);
        _spMock.Setup(sp => sp.GetService(typeof(LoginViewModel))).Returns(loginVm);

        var vm = new MainViewModel(_authMock.Object, _spMock.Object);

        await vm.InitializeNavigationAsync();

        vm.CurrentView.Should().BeOfType<LoginViewModel>();
        vm.IsNavigationVisible.Should().BeFalse();
    }

    [TestMethod]
    public async Task Initialize_WhenDbNotCreated_ShouldShowSetup()
    {
        _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.NotCreated);

        var setupVm = new InitialSetupViewModel(
            new Mock<ISettingsService>().Object,
            new Mock<IAppConfigService>().Object,
            new Mock<ILocalizationService>().Object,
            new Mock<IMessageService>().Object,
            new Mock<IAuthService>().Object);

        _spMock.Setup(sp => sp.GetService(typeof(InitialSetupViewModel))).Returns(setupVm);

        var vm = new MainViewModel(_authMock.Object, _spMock.Object);

        await vm.InitializeNavigationAsync();

        vm.CurrentView.Should().BeOfType<InitialSetupViewModel>();
        vm.IsNavigationVisible.Should().BeFalse();
    }

    [TestMethod]
    public async Task ShowToday_ShouldSetNavigationVisible()
    {
        var todayVm = new Mock<TodayViewModel>(
                new Mock<IJournalService>().Object,
                new Mock<IMessageService>().Object,
                new Mock<IAnalyticsService>().Object
            ).Object;

        _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.Unencrypted);
        _spMock.Setup(sp => sp.GetService(typeof(TodayViewModel))).Returns(todayVm);

        var vm = new MainViewModel(_authMock.Object, _spMock.Object);

        await vm.InitializeNavigationAsync();

        vm.ShowTodayCommand.Execute(null);

        vm.IsNavigationVisible.Should().BeTrue();
        vm.CurrentView.Should().Be(todayVm);
    }
}