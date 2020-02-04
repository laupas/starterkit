using FluentAssertions;
using Installer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace installer.tests
{
    [TestClass]
    public class OptionsTests : BaseTest
    {
        [TestMethod]
        public void Verbose_LongKey_RightValueInOptions()
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
        public void Verbose_ShortKey_RightValueInOptions()
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
        public void TryRun_LongKey_RightValueInOptions()
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
        public void Dns_LongKey_RightValueInOptions()
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
        public void Dns_ShortKey_RightValueInOptions()
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
        public void InstallRootCertificate_LongKey_RightValueInOptions()
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
        public void FullInstallation_LongKey_RightValueInOptions()
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
        public void FullInstallation_ShortKey_RightValueInOptions()
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
        public void InstallRancher_LongKey_RightValueInOptions()
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
        public void InstallStarterkit_LongKey_RightValueInOptions()
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
        public void UnInstallStarterkit_LongKey_RightValueInOptions()
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
}