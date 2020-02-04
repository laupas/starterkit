using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Installer;
using installer.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace installer.tests
{
    [TestClass]
    public class InstallerTests : BaseTest
    {
        [TestMethod]
        public void CreateTargets_FullIDefault_CorrectTargets()
        {
            // arrange
            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "ShowInfo");
        }
        
        [TestMethod]
        public void CreateTargets_Filter_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("-f");
            this.Arguments.Add("--filter=CheckKubeCtl InstallIngress");

            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "InstallIngress");
            targets.Should().HaveCount(2);
        }
        
        [TestMethod]
        public void CreateTargets_SSLHack_FullIInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("-f");
            this.Arguments.Add("--install-root-certificates");
            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "InstallCertificates");
            targets.Should().HaveElementAt(2, "InstallIngress");
            targets.Should().HaveElementAt(3, "InstallRancher");
            targets.Should().HaveElementAt(4, "WaitUntilIsUpAndReady");
            targets.Should().HaveElementAt(5, "Login");
            targets.Should().HaveElementAt(6, "ChangePassword");
            targets.Should().HaveElementAt(7, "SetServerUrl");
            targets.Should().HaveElementAt(8, "InstallStarterkitCommon");
            targets.Should().HaveElementAt(9, "InstallStorage");
            targets.Should().HaveElementAt(10, "InstallLdap");
            targets.Should().HaveElementAt(11, "InstallJenkins");
            targets.Should().HaveElementAt(12, "WaitUntilRancherIsActive");
            targets.Should().HaveElementAt(13, "ExecuteUpdatesForDockerDesktop");
            targets.Should().HaveElementAt(14, "ShowInfo");
        }
        
        [TestMethod]
        public void CreateTargets_FullInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("-f");
            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "InstallIngress");
            targets.Should().HaveElementAt(2, "InstallRancher");
            targets.Should().HaveElementAt(3, "WaitUntilIsUpAndReady");
            targets.Should().HaveElementAt(4, "Login");
            targets.Should().HaveElementAt(5, "ChangePassword");
            targets.Should().HaveElementAt(6, "SetServerUrl");
            targets.Should().HaveElementAt(7, "InstallStarterkitCommon");
            targets.Should().HaveElementAt(8, "InstallStorage");
            targets.Should().HaveElementAt(9, "InstallLdap");
            targets.Should().HaveElementAt(10, "InstallJenkins");
            targets.Should().HaveElementAt(11, "WaitUntilRancherIsActive");
            targets.Should().HaveElementAt(12, "ExecuteUpdatesForDockerDesktop");
            targets.Should().HaveElementAt(13, "ShowInfo");
        }
        
        [TestMethod]
        public void CreateTargets_RancherOnlyInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("--install-rancher");
            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "InstallIngress");
            targets.Should().HaveElementAt(2, "InstallRancher");
            targets.Should().HaveElementAt(3, "WaitUntilIsUpAndReady");
            targets.Should().HaveElementAt(4, "Login");
            targets.Should().HaveElementAt(5, "ChangePassword");
            targets.Should().HaveElementAt(6, "SetServerUrl");
            targets.Should().HaveElementAt(7, "WaitUntilRancherIsActive");
            targets.Should().HaveElementAt(8, "ExecuteUpdatesForDockerDesktop");
            targets.Should().HaveElementAt(9, "ShowInfo");
        }
        
        [TestMethod]
        public void CreateTargets_StarterkitOnlyInstallation_CorrectTargets()
        {
            // arrange
            this.Arguments.Add("--install-starterkit");
            this.StartAllServices();
            
            // act
            var targets = this.Get<Helper.Installer>().CreateTargetsList().Select(x => x.Method.Name).ToList();
            
            // Assert
            targets.Should().HaveElementAt(0, "CheckKubeCtl");
            targets.Should().HaveElementAt(1, "InstallStarterkitCommon");
            targets.Should().HaveElementAt(2, "InstallStorage");
            targets.Should().HaveElementAt(3, "InstallLdap");
            targets.Should().HaveElementAt(4, "InstallJenkins");
            targets.Should().HaveElementAt(5, "ShowInfo");
        }

        [TestMethod]
        public void Install_AllTargetsSuceeded_VerifyOrder()
        {
            // Arrange
            Stack<string> results = new Stack<string>();
            Mock<ITargetsBase> target1Mock = null;
            this.StartSpecial(((collection, repository) =>
            {
                target1Mock = repository.Create<ITargetsBase>();
                collection.AddSingleton(target1Mock.Object);
                collection.AddSingleton(new Options());
                collection.AddSingleton<Helper.Installer>();
            }));

            IDictionary<int,Action> actionList = new Dictionary<int, Action>();
            actionList.Add(1, () => results.Push("Action1"));
            actionList.Add(2, () => results.Push("Action2"));
            target1Mock.Setup(x => x.DefineTargetToExecute()).Returns(actionList);

            // Act
            this.Get<Helper.Installer>().Install();

            //Assert
            results.Pop().Should().Be("Action2");
            results.Pop().Should().Be("Action1");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InstallerException))]
        public void Install_NotAllTargetsSuceeded_Throw()
        {
            // Arrange
            Mock<ITargetsBase> target1Mock = null;
            this.StartSpecial(((collection, repository) =>
            {
                target1Mock = repository.Create<ITargetsBase>();
                collection.AddSingleton(target1Mock.Object);
                collection.AddSingleton(new Options());
                collection.AddSingleton<Helper.Installer>();
            }));

            IDictionary<int,Action> actionList = new Dictionary<int, Action>();
            actionList.Add(1, () => throw new Exception("Error"));
            target1Mock.Setup(x => x.DefineTargetToExecute()).Returns(actionList);

            // Act
            this.Get<Helper.Installer>().Install();

            //Assert
        }
    }
}