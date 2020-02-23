/*using FluentAssertions;
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
            var options = this.Get<Options>();

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
            var options = this.Get<Options>();

            //Assert
            options.Verbose.Should().Be(true);
        }

        [TestMethod]
        public void CreateOptions_TryRun_LongWord()
        {
            // Arrange
            this.Arguments.Add("--try-run");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.TryRun.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_DNS_LongWord()
        {
            // Arrange
            this.Arguments.Add("--dns");
            this.Arguments.Add("test.local");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

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
            var options = this.Get<Options>();

            //Assert
            options.Dns.Should().Be("test.local");
        }
        
        [TestMethod]
        public void CreateOptions_InstallRootCertificate_LongWord()
        {
            // Arrange
            this.Arguments.Add("--install-root-certificates");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.InstallRootCertificates.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_FullInstallation_LongWord()
        {
            // Arrange
            this.Arguments.Add("--full-installation");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.FullInstallation.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_FullInstallation_ShortWord()
        {
            // Arrange
            this.Arguments.Add("-f");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.FullInstallation.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_InstallRancher_LongWord()
        {
            // Arrange
            this.Arguments.Add("--install-rancher");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.InstallRancher.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_InstallStarterkit_LongWord()
        {
            // Arrange
            this.Arguments.Add("--install-starterkit");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.InstallStarterKit.Should().Be(true);
        }
        
        [TestMethod]
        public void CreateOptions_UnInstallStarterkit_LongWord()
        {
            // Arrange
            this.Arguments.Add("--uninstall-starterkit");
            this.StartAllServices();
            
            // Act
            var options = this.Get<Options>();

            //Assert
            options.UnInstallStarterKit.Should().Be(true);
        }
    }
}*/