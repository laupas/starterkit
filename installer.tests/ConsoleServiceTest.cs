using FluentAssertions;
using Installer.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Installer.Tests
{
    [TestClass]
    public class ConsoleServiceTest : BaseTest
    {
        [TestMethod]
        public void AskFormConfirmation_ShowMessage_CorrectMessagePrintedt()
        {
            // Arrange
            this.StartAllServices();

            // Act
            var console = this.CatchConsole(() =>
            {
                this.Get<IConsoleService>().AskFormConfirmation("Hello Test?", "myanswer");
            }, "myanswer");

            //Assert
            console.Should().Contain("Hello Test?");
        }

        [TestMethod]
        public void AskFormConfirmation_ExpectedAnswer_Return_true()
        {
            // Arrange
            this.StartAllServices();
            bool answer = false;

            // Act
            this.CatchConsole(() =>
            { 
                answer = this.Get<IConsoleService>().AskFormConfirmation("Hello Test?", "y");
            }, "y");

            //Assert
            answer.Should().Be(true);
        }
        
        [TestMethod]
        public void AskFormConfirmation_NotExpectedAnswer_Return_false()
        {
            // Arrange
            this.StartAllServices();
            bool answer = false;

            // Act
            this.CatchConsole(() =>
            { 
                answer = this.Get<IConsoleService>().AskFormConfirmation("Hello Test?", "n");
            }, "y");

            //Assert
            answer.Should().Be(false);
        }
    }
}