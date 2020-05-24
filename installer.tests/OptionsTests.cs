using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Installer.Tests
{
    [TestClass]
    public class OptionsTests : BaseTest
    {
        [TestMethod]
        public void CreateOptions_Verbose_LongWord()
        {
            // Arrange
            this.Arguments.Add("--verbose");
            this.StartAllServices();
            
            // Act
            var options = Options.SetOptions(this.Arguments.ToArray());

            //Assert
            options.Verbose.Should().Be(true);
        }

        [TestMethod]
        public void CreateOptions_Verbose_ShortWord()
        {
            // Arrange
            this.Arguments.Add("-v");
            this.StartAllServices();
            
            // Act
            var options = Options.SetOptions(this.Arguments.ToArray());

            //Assert
            options.Verbose.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_DNS_LongWord()
        {
            // Arrange
            this.Arguments.Add("--dns");
            this.Arguments.Add("test.local");
            this.StartAllServices();
            
            // Act
            var options = Options.SetOptions(this.Arguments.ToArray());

            //Assert
            options.Dns.Should().Be("test.local");
        }

        [TestMethod]
        public void CreateOptions_DNS_ShortWord()
        {
            // Arrange
            this.Arguments.Add("-d");
            this.Arguments.Add("test.local");
            this.StartAllServices();
            
            // Act
            var options = Options.SetOptions(this.Arguments.ToArray());

            //Assert
            options.Dns.Should().Be("test.local");
        }
    }
}