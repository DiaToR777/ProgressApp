using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.ViewModels.Settings;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class SettingsViewModelTests
    {
        private Mock<ISettingsService> _serviceMock;
        private Mock<IMessageService> _messageMock;
        private Mock<ILocalizationService> _localizationMock;
        private Mock<IThemeService> _themeMock;


        [TestInitialize]
        public void TestInit()
        {
            _messageMock = new Mock<IMessageService>();
            _serviceMock = new Mock<ISettingsService>();
            _localizationMock = new Mock<ILocalizationService>();
            _themeMock = new Mock<IThemeService>();

            _serviceMock.Setup(s => s.GetUserName()).Returns("TestName");
            _serviceMock.Setup(s => s.GetGoal()).Returns("TestDescription");
            _serviceMock.Setup(s => s.GetTheme()).Returns(AppTheme.Dark);
            _serviceMock.Setup(s => s.GetLanguage()).Returns(new LanguageModel { CultureCode = "uk-UA" });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // This method is called after each test method.
        }

        [TestMethod]
        public void Constructor_ShouldLoadSettingsFromService()
        {
            var vm = new SettingsViewModel(_serviceMock.Object, _messageMock.Object, _localizationMock.Object, _themeMock.Object);

            vm.Username.Should().Be("TestName");
            vm.Goal.Should().Be("TestDescription");
            vm.SelectedTheme.Should().Be(AppTheme.Dark);
        }

        [TestMethod]
        public void SaveSettingsCommand_ShouldBeDisabled_WhenUsernameIsEmpty()
        {
            var vm = new SettingsViewModel(_serviceMock.Object, _messageMock.Object, _localizationMock.Object, _themeMock.Object);

            vm.Username = "";

            vm.SaveSettingsCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void SaveSettingsCommand_ShouldInvokeServiceSave_AndShowSuccessMessage()
        {
            var vm = new SettingsViewModel(_serviceMock.Object, _messageMock.Object, _localizationMock.Object, _themeMock.Object);
            vm.Username = "NewName";
            vm.Goal = "NewGoal";
            var newLang = new LanguageModel { CultureCode = "en-US" };
            vm.SelectedLanguage = newLang;

            vm.SaveSettingsCommand.Execute(null);

            _serviceMock.Verify(s => s.SaveSettings("NewName", "NewGoal", It.IsAny<AppTheme>(), newLang), Times.Once);

            _messageMock.Verify(m => m.ShowInfo("Msg_SettingsSaved"), Times.Once);
        }


    }
}
