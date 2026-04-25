using FluentAssertions;
using Moq;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.WpfUI.ViewModels.Login;

namespace ProgressAppTest.ViewModelTests;

[TestClass]
public sealed class LoginViewModelTests
{
    private Mock<IAuthService> _authMock;
    private Mock<IMessageService> _messageMock;

    [TestInitialize]
    public void TestInit()
    {
        _authMock = new Mock<IAuthService>();
        _messageMock = new Mock<IMessageService>();
    }

    [TestMethod]
    public async Task LoginCommand_WhenPasswordIsCorrect_ShouldInvokeCompleted()
    {
        var vm = new LoginViewModel(_authMock.Object, _messageMock.Object);
        vm.Password = "correct_pass";

        bool completedCalled = false;
        vm.Completed = () => completedCalled = true;

        _authMock.Setup(a => a.LoginAsync("correct_pass")).ReturnsAsync(true);

        vm.LoginCommand.Execute(null);
        await Task.Delay(100); 

        completedCalled.Should().BeTrue();
        _authMock.Verify(a => a.LoginAsync("correct_pass"), Times.Once);
    }

    [TestMethod]
    public async Task LoginCommand_WhenPasswordIsWrong_ShouldClearPasswordAndShowError()
    {
        var vm = new LoginViewModel(_authMock.Object, _messageMock.Object);
        vm.Password = "wrong_pass";

        _authMock.Setup(a => a.LoginAsync("wrong_pass")).ReturnsAsync(false);

        vm.LoginCommand.Execute(null);
        await Task.Delay(100);

        vm.Password.Should().BeEmpty(); 
        _messageMock.Verify(m => m.ShowErrorIncorrectPasswordAsync(), Times.Once);
        _authMock.Verify(a => a.LoginAsync("wrong_pass"), Times.Once);
    }

    [TestMethod]
    public void LoginCommand_CanExecute_ShouldReturnFalse_WhenPasswordIsEmpty()
    {
        var vm = new LoginViewModel(_authMock.Object, _messageMock.Object);

        vm.Password = "  "; 

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }
}