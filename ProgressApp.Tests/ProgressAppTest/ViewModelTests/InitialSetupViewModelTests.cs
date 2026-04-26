using FluentAssertions;
using Moq;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;
using ProgressApp.WpfUI.ViewModels.InitialSetup;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class InitialSetupViewModelTests
{
    private Mock<ISettingsService> _settingsService;
    private Mock<IMessageService> _messageService;
    private Mock<ILocalizationService> _localizationService;
    private Mock<IAuthService> _authService;
    private Mock<IAppConfigService> _appConfigService;

    [TestInitialize]
    public void TestInit()
    {
        _settingsService = new Mock<ISettingsService>();
        _messageService = new Mock<IMessageService>();
        _localizationService = new Mock<ILocalizationService>();
        _authService = new Mock<IAuthService>();
        _appConfigService = new Mock<IAppConfigService>();
    }

    [TestMethod]
    public void Constructor_ShouldSetDefaultLanguage()
    {
        var vm = CreateViewModel();
        vm.SelectedLanguage.Should().NotBeNull();
        vm.SelectedLanguage.CultureCode.Should().Be("en-US"); //first language in config
    }

    [TestMethod]
    public void FinishCommand_CanExecute_ShouldReturnFalse_WhenPasswordsDoNotMatch()
    {
        var vm = CreateViewModel();
        vm.Username = "test";
        vm.Goal = "test";
        vm.Password = "testpass";
        vm.ConfirmPassword = "differentpass";

        vm.FinishCommand.CanExecute(null).Should().BeFalse();
    }

    [TestMethod]
    public async Task FinishCommand_WhenValid_ShouldFlowCorrect()
    {
        var vm = CreateViewModel();
        vm.Username = "test";
        vm.Goal = "test";
        vm.Password = "testpass";
        vm.ConfirmPassword = "testpass";

        bool completedCalled = false;
        vm.Completed = () => completedCalled = true;

        _authService.Setup(a => a.RegisterAsync(vm.Password)).ReturnsAsync(true);

        vm.FinishCommand.Execute(null);

        await Task.Delay(100);

        _authService.Verify(a => a.RegisterAsync("testpass"), Times.Once);
        _settingsService.Verify(s => s.SaveGoalAsync("test"), Times.Once);
        _appConfigService.Verify(c => c.Save(It.IsAny<AppConfig>()), Times.Once);
        completedCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task FinishCommand_WhenException_ShouldShowError()
    {
        var vm = CreateViewModel();
        vm.Username = "username";
        vm.Goal = "test";
        vm.Password = "123";
        vm.ConfirmPassword = "123";

        var ex = new AppException("Registration Failed", isCritical: false, "Error");
        _authService.Setup(a => a.RegisterAsync(It.IsAny<string>())).ThrowsAsync(ex);

        vm.FinishCommand.Execute(null);
        await Task.Delay(100);

        _messageService.Verify(m => m.ShowErrorAsync(ex), Times.Once);
    }

    private InitialSetupViewModel CreateViewModel()
    {
        return new InitialSetupViewModel(_settingsService.Object, _appConfigService.Object, _localizationService.Object, _messageService.Object, _authService.Object);
    }
}
