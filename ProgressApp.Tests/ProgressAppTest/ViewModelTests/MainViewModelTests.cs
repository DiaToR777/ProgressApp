using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Enums;
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
        await Task.Delay(100); 

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
        await Task.Delay(100);

        vm.CurrentView.Should().BeOfType<InitialSetupViewModel>();
        vm.IsNavigationVisible.Should().BeFalse();
    }

    [TestMethod]
    public async Task ShowToday_ShouldSetNavigationVisible()
    {
        _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.Unencrypted);
        _spMock.Setup(sp => sp.GetService(typeof(TodayViewModel))).Returns(new Mock<TodayViewModel>(null, null, null).Object);

        var vm = new MainViewModel(_authMock.Object, _spMock.Object);
        await Task.Delay(100);

        vm.IsNavigationVisible.Should().BeTrue();
    }
}