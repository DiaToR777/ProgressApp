using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.ViewModels.InitialSetup;

namespace ProgressAppTest.ViewModelTests
{
    [TestClass]
    public sealed class InitialSetupViewModelTests
    {
        private Mock<ISettingsService> _settingsService;
        private Mock<IMessageService> _messageService;
        private Mock<ILocalizationService> _localizationService;

        private LanguageModel _defaultLanguage;


        [TestInitialize]
        public void TestInit()
        {
            _settingsService = new Mock<ISettingsService>();
            _messageService = new Mock<IMessageService>();
            _localizationService = new Mock<ILocalizationService>();

            _defaultLanguage = new LanguageModel { CultureCode = "en-US", Name = "English" };

            _settingsService.Setup(s => s.GetLanguage()).Returns(_defaultLanguage);
        }

        [TestMethod]
        public void Constructor_ShouldLoadSettingsAndSetLanguage()
        {
            var vm = new InitialSetupViewModel(_settingsService.Object, _localizationService.Object, _messageService.Object);

            vm.SelectedLanguage.Should().Be(_defaultLanguage);
            _settingsService.Verify(s => s.GetLanguage(), Times.Once);
        }

        [TestMethod]
        public void FinishCommand_WhenValidData_ShouldSaveSettingsAndCallCompleted()
        {
            var vm = new InitialSetupViewModel(_settingsService.Object, _localizationService.Object, _messageService.Object);
            vm.Username = "User";
            vm.Goal = "Get Fit";
            bool completedCalled = false;
            vm.Completed = () => completedCalled = true;

            vm.FinishCommand.Execute(null);

            _settingsService.Verify(s => s.SaveSettings(
                "User",
                "Get Fit", 
                AppTheme.Light,
                It.IsAny<LanguageModel>()), Times.Once);

            _localizationService.Verify(l => l.ChangeLanguage(It.IsAny<string>()), Times.Once);
            completedCalled.Should().BeTrue();
        }

        [TestMethod]
        public void FinishCommand_WhenInvalidData_ShouldShowError()
        {
            _settingsService.Setup(s => s.SaveSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AppTheme>(), It.IsAny<LanguageModel>()))
                         .Throws(new ArgumentException("Database error"));

            var vm = new InitialSetupViewModel(_settingsService.Object, _localizationService.Object, _messageService.Object);
            vm.Username = "User";
            vm.Goal = "Goal";

            vm.FinishCommand.Execute(null);

            _messageService.Verify(m => m.ShowError("Database error"), Times.Once);
        }
    }
}
