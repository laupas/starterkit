using FluentAssertions;
using Installer.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Installer.Tests
{
    [TestClass]
    public class KubernetesHelperTest : BaseTest
    {
        [TestMethod]
        public void CreateNameSpace_NonExisting()
        {
            // Arrange
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "some string";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);


            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().CreateNameSpace(nameSapce);

            //Assert
            processMock.Verify(p => p.Run("kubectl", $"create namespace {nameSapce}", It.IsAny<string>(), true, false));
        }
        
        [TestMethod]
        public void CreateNameSpace_Existing()
        {
            // Arrange
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyNameSpace string";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().CreateNameSpace(nameSapce);

            //Assert
            processMock.Verify(p => p.Run("kubectl", $"create namespace {nameSapce}", It.IsAny<string>(), true, false), Times.Never);
        }

        [TestMethod]
        public void InstallResourceIfNotExists_NonExisting()
        {
            // Arrange
            var name = "MyName";
            var resourceType = "MyResourceType";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "some string";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().InstallResourceIfNotExists(name, resourceType, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("kubectl", $"create {resourceType} {name} --namespace {nameSapce}", It.IsAny<string>(), true, false));
        }
        
        [TestMethod]
        public void InstallResourceIfNotExists_Existing()
        {
            // Arrange
            var name = "MyName";
            var resourceType = "MyResourceType";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyName string";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().InstallResourceIfNotExists(name, resourceType, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("kubectl", $"create {resourceType} {name} --namespace {nameSapce}", It.IsAny<string>(), true, false), Times.Never);
        }

        [TestMethod]
        public void InstallApplicationeIfNotExists_NonExisting()
        {
            // Arrange
            var name = "MyName";
            var repoName = "stable/MyApp";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "some string";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().InstallApplicationeIfNotExists(name, repoName, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("helm", $"install {name} {repoName} --namespace {nameSapce}", It.IsAny<string>(), true, false));
        }

        [TestMethod]
        public void InstallApplicationeIfNotExists_Existing()
        {
            // Arrange
            var name = "MyName";
            var repoName = "stable/MyApp";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyName string";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().InstallApplicationeIfNotExists(name, repoName, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("helm", $"install {name} {repoName} --namespace {nameSapce}", It.IsAny<string>(), true, false), Times.Never);
        }

        [TestMethod]
        public void UnInstallApplicationeIfNotExists_NonExisting()
        {
            // Arrange
            var name = "MyName";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "somestring default";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().UnInstallApplicationeIfExists(name, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("helm", $"uninstall {name} --namespace {nameSapce}", It.IsAny<string>(), true, false), Times.Never);
        }

        [TestMethod]
        public void UnInstallApplicationeIfNotExists_Existing()
        {
            // Arrange
            var name = "MyName";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyName default";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            Starter.Get<IKubernetesHelper>().UnInstallApplicationeIfExists(name, nameSapce);

            //Assert
            processMock.Verify(p => p.Run("helm", $"uninstall {name} --namespace {nameSapce}", It.IsAny<string>(), true, false));
        }
        
        [TestMethod]
        public void CheckIfResourceExists_NonExisting()
        {
            // Arrange
            var name = "MyName";
            var resourceType = "MyResourceType";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "some string";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            var existing = Starter.Get<IKubernetesHelper>().CheckIfResourceExists(name, resourceType, nameSapce);

            //Assert
            existing.Should().BeFalse();
            processMock.Verify(p => p.Read("kubectl", $"get {resourceType} --namespace {nameSapce}", It.IsAny<string>(), true, true));
        }
        
        [TestMethod]
        public void CheckIfResourceExists_Existing()
        {
            // Arrange
            var name = "MyName";
            var nameSapce = "MyNameSpace";
            var resourceType = "MyResourceType";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyName Active 11m";
            processMock.Setup(x => x.Read("kubectl", It.IsRegex("get .*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            var existing = Starter.Get<IKubernetesHelper>().CheckIfResourceExists(name, resourceType, nameSapce);

            //Assert
            existing.Should().BeTrue();
            processMock.Verify(p => p.Read("kubectl", $"get {resourceType} --namespace {nameSapce}", It.IsAny<string>(), true, true));
        }
        
        [TestMethod]
        public void CheckIfApplicationExists_NonExisting()
        {
            // Arrange
            var name = "MyName";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "some string";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            var existing = Starter.Get<IKubernetesHelper>().CheckIfApplicationExists(name, nameSapce);

            //Assert
            existing.Should().BeFalse();
            processMock.Verify(p => p.Read("helm", $"list --namespace {nameSapce}", It.IsAny<string>(), true, true));
        }

        [TestMethod]
        public void CheckIfApplicationExists_Existing()
        {
            // Arrange
            var name = "MyName";
            var nameSapce = "MyNameSpace";
            var processMock = this.RegisterMock<IProcessHelper>();
            var retournString = "MyName default";
            processMock.Setup(x => x.Read("helm", It.IsRegex("list.*"), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(retournString);

            this.StartAllServices();

            // Act
            var existing = Starter.Get<IKubernetesHelper>().CheckIfApplicationExists(name, nameSapce);

            //Assert
            existing.Should().BeTrue();
            processMock.Verify(p => p.Read("helm", $"list --namespace {nameSapce}", It.IsAny<string>(), true, true));
        }
        
    }
}