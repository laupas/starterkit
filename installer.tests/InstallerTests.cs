using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace installer.tests
{
    [TestClass]
    public class InstallerTests : BaseTest
    {
        [TestMethod]
        public void CreateTargets_FullIDefault_CorrectTargets()
        {
            // arrange
            this.Start();
            var targets = new List<string>();
            
            // act
            this.Get<Installer.Installer>().CreateTargets(targets);
            
            // Assert
            targets.Should().HaveElementAt(0, "checkKubeclt");
            targets.Should().HaveElementAt(1, "showInfo");
        }
        
        [TestMethod]
        public void CreateTargets_SSLHack_FullIInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("-f");
            this.Arguments.Add("--install-root-certificates");
            this.Start();
            var targets = new List<string>();
            
            // act
            this.Get<Installer.Installer>().CreateTargets(targets);
            
            // Assert
            targets.Should().HaveElementAt(0, "checkKubeclt");
            targets.Should().HaveElementAt(1, "installCertificates");
            targets.Should().HaveElementAt(2, "installIngress");
            targets.Should().HaveElementAt(3, "installRancher");
            targets.Should().HaveElementAt(4, "configureRancher-step-1");
            targets.Should().HaveElementAt(5, "installStarterkitCommon");
            targets.Should().HaveElementAt(6, "installLdap");
            targets.Should().HaveElementAt(7, "installJenkins");
            targets.Should().HaveElementAt(8, "configureRancher-step-2");
            targets.Should().HaveElementAt(9, "showInfo");
        }
        
        [TestMethod]
        public void CreateTargets_FullInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("-f");
            this.Start();
            var targets = new List<string>();
            
            // act
            this.Get<Installer.Installer>().CreateTargets(targets);
            
            // Assert
            targets.Should().HaveElementAt(0, "checkKubeclt");
            targets.Should().HaveElementAt(1, "installIngress");
            targets.Should().HaveElementAt(2, "installRancher");
            targets.Should().HaveElementAt(3, "configureRancher-step-1");
            targets.Should().HaveElementAt(4, "installStarterkitCommon");
            targets.Should().HaveElementAt(5, "installLdap");
            targets.Should().HaveElementAt(6, "installJenkins");
            targets.Should().HaveElementAt(7, "configureRancher-step-2");
            targets.Should().HaveElementAt(8, "showInfo");
        }
        
        [TestMethod]
        public void CreateTargets_RancherOnlyInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("--install-rancher");
            this.Start();
            var targets = new List<string>();
            
            // act
            this.Get<Installer.Installer>().CreateTargets(targets);
            
            // Assert
            targets.Should().HaveElementAt(0, "checkKubeclt");
            targets.Should().HaveElementAt(1, "installIngress");
            targets.Should().HaveElementAt(2, "installRancher");
            targets.Should().HaveElementAt(3, "configureRancher-step-1");
            targets.Should().HaveElementAt(4, "configureRancher-step-2");
            targets.Should().HaveElementAt(5, "showInfo");
        }
        
        [TestMethod]
        public void CreateTargets_StarterkitOnlyInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("--install-starterkit");
            this.Start();
            var targets = new List<string>();
            
            // act
            this.Get<Installer.Installer>().CreateTargets(targets);
            
            // Assert
            targets.Should().HaveElementAt(0, "checkKubeclt");
            targets.Should().HaveElementAt(1, "installStarterkitCommon");
            targets.Should().HaveElementAt(2, "installLdap");
            targets.Should().HaveElementAt(3, "installJenkins");
            targets.Should().HaveElementAt(4, "showInfo");        
        }
    }
}