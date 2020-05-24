using Installer.Helper;

namespace Installer
{
    internal interface ICommonTargets
    {
        void CheckKubeCtl(InstallerProcess installerProcess);
        void InstallCertificates(InstallerProcess installerProcess);
        void ShowInfo(InstallerProcess installerProcess);
        void CheckHelm(InstallerProcess installerProcess);
    }
}