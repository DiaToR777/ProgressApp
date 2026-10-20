using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Enums;
using ProgressApp.Domain.Models.User;
using ProgressApp.Infrastructure.Configuration;
using ProgressApp.WpfUI.Localization;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.Themes;
using ProgressApp.WpfUI.ViewModels.Settings;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class SettingsViewModelTests
{
    private Mock<IMessageService> _messageMock;
    private Mock<ILocalizationService> _localizationMock;
    private Mock<IAppThemeService> _themeMock;
    private Mock<IAppConfigService> _configMock;
    private Mock<IAuthService> _authMock;
    private Mock<IUserService> _userMock;


    private readonly AppConfig _config = new() { Theme = "Dark", Language = "uk-UA" };
    private readonly User _testUser = new() { Id = Guid.NewGuid(), Username = "TestName" };

    [TestInitialize]
    public void TestInit()
    {
        _messageMock = new Mock<IMessageService>();
        _localizationMock = new Mock<ILocalizationService>();
        _themeMock = new Mock<IAppThemeService>();
        _configMock = new Mock<IAppConfigService>();
        _authMock = new Mock<IAuthService>();
        _userMock = new Mock<IUserService>();

        _userMock.Setup(u => u.GetCurrentUserAsync()).ReturnsAsync(_testUser);
        _configMock.Setup(c => c.Load()).Returns(_config);
        _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.Encrypted);
    }

    [TestMethod]
    public async Task Constructor_ShouldLoadSettings_AndHandleParallelLoading()
    {
        var vm = await CreateAndInitVmAsync();

        vm.Username.Should().Be("TestName");
        vm.SelectedTheme.Should().Be(AppTheme.Dark);
        vm.IsDbEncrypted.Should().BeTrue();
    }

    [TestMethod]
    public async Task SaveSettingsCommand_ShouldBeDisabled_WhenUsernameIsEmpty()
    {
        var vm = await CreateAndInitVmAsync();
        vm.Username = "";
        vm.SaveSettingsCommand.CanExecute(null).Should().BeFalse();
    }

    [TestMethod]
    public async Task SaveSettingsCommand_WhenValid_ShouldSaveAndApplySettings()
    {
        var vm = await CreateAndInitVmAsync();
        vm.Username = "NewName";
        var newLang = LanguageConfig.AvailableLanguages.Last();
        vm.SelectedLanguage = newLang;

        await ((IAsyncRelayCommand)vm.SaveSettingsCommand).ExecuteAsync(null);

        _userMock.Verify(u => u.UpdateUsernameAsync("NewName"), Times.Once);
        _configMock.Verify(c => c.Save(It.Is<AppConfig>(cfg => cfg.Language == newLang.CultureCode)), Times.Once);
        _localizationMock.Verify(l => l.ChangeLanguage(newLang.CultureCode), Times.Once);
        _messageMock.Verify(m => m.ShowInfoAsync("Msg_SettingsSaved"), Times.Once);
    }

    [TestMethod]
    public async Task ApplyNewPassword_WhenPasswordsMatch_ShouldChangeAndReset()
    {
        var vm= await CreateAndInitVmAsync();

        vm.NewDbPassword = "pass";
        vm.ConfirmDbPassword = "pass";
        vm.IsDbEncrypted = true;

        await ((IAsyncRelayCommand)vm.ApplyNewPasswordCommand).ExecuteAsync(null);

        _authMock.Verify(a => a.ChangePasswordAsync("pass"), Times.Once);
        vm.NewDbPassword.Should().BeEmpty();
        vm.IsChangingPassword.Should().BeFalse();
    }

    private async Task<SettingsViewModel> CreateAndInitVmAsync()
    {
        var vm = new SettingsViewModel(
            _configMock.Object,
            _messageMock.Object,
            _localizationMock.Object,
            _themeMock.Object,
            _authMock.Object,
            _userMock.Object);

        await vm.Initialization;

        return vm;
    }
}