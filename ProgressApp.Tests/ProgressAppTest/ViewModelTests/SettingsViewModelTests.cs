using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.WpfUI.ViewModels.Settings;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class SettingsViewModelTests
    {
        private Mock<ISettingsService> _serviceMock;
        private Mock<IMessageService> _messageMock;
        private Mock<ILocalizationService> _localizationMock;
        private Mock<IAppThemeService> _themeMock;
        private Mock<IAppConfigService> _configMock;
        private Mock<IDataExchangeService> _dataExchangeMock;
        private Mock<IAuthService> _authMock;

        [TestInitialize]
        public void TestInit()
        {
            _messageMock = new Mock<IMessageService>();
            _serviceMock = new Mock<ISettingsService>();
            _localizationMock = new Mock<ILocalizationService>();
            _themeMock = new Mock<IAppThemeService>();
            _configMock = new Mock<IAppConfigService>();
            _dataExchangeMock = new Mock<IDataExchangeService>();
            _authMock = new Mock<IAuthService>();

            var config = new AppConfig
            {
                Username = "TestName",
                Theme = "Dark",
                Language = "uk-UA"
            };
            _configMock.Setup(c => c.Load()).Returns(config);
            _serviceMock.Setup(s => s.GetGoalAsync()).ReturnsAsync("TestDescription");
            _authMock.Setup(a => a.GetDbStatusAsync()).ReturnsAsync(DbStatus.Encrypted);
        }

        [TestMethod]
        public async Task Constructor_ShouldLoadSettings_AndHandleParallelLoading()
        {
            var vm = new SettingsViewModel(
                _serviceMock.Object, _configMock.Object, _messageMock.Object,
                _localizationMock.Object, _themeMock.Object, _dataExchangeMock.Object, _authMock.Object);

            await Task.Delay(50);

            vm.Username.Should().Be("TestName");
            vm.Goal.Should().Be("TestDescription");
            vm.SelectedTheme.Should().Be(AppTheme.Dark);
            vm.IsDbEncrypted.Should().BeTrue();
        }

        [TestMethod]
        public void SaveSettingsCommand_ShouldBeDisabled_WhenUsernameIsEmpty()
        {
            var vm = CreateViewModel();
            vm.Username = "";
            vm.SaveSettingsCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public async Task SaveSettingsCommand_ShouldInvokeEverything_AndShowSuccess()
        {
            var vm = CreateViewModel();
            await Task.Delay(50); 

            vm.Username = "NewName";
            vm.Goal = "NewGoal";
            var newLang = LanguageConfig.AvailableLanguages.Last();
            vm.SelectedLanguage = newLang;

            vm.SaveSettingsCommand.Execute(null);
            await Task.Delay(50); 

            _configMock.Verify(c => c.Save(It.Is<AppConfig>(acc => acc.Username == "NewName")), Times.Once);
            _serviceMock.Verify(s => s.SaveGoalAsync("NewGoal"), Times.Once);
            _messageMock.Verify(m => m.ShowInfoAsync("Msg_SettingsSaved"), Times.Once);
        }

        [TestMethod]
        public async Task ApplyNewPassword_WhenPasswordsMatch_ShouldChangeAndReset()
        {
            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.NewDbPassword = "pass";
            vm.ConfirmDbPassword = "pass";
            vm.IsDbEncrypted = true;

            vm.ApplyNewPasswordCommand.Execute(null);
            await Task.Delay(50);

            _authMock.Verify(a => a.ChangePasswordAsync("pass"), Times.Once);
            vm.NewDbPassword.Should().BeEmpty();
            vm.IsChangingPassword.Should().BeFalse();
        }

        private SettingsViewModel CreateViewModel()
        {
            return new SettingsViewModel(
                _serviceMock.Object, _configMock.Object, _messageMock.Object,
                _localizationMock.Object, _themeMock.Object, _dataExchangeMock.Object, _authMock.Object);
        }
    }
}
