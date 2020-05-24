using Installer.Helper;

namespace Installer
{
    internal interface IStarterkitTargets
    {
        void InstallLdap(InstallerProcess installerProcess);
        void InstallStorage(InstallerProcess installerProcess);
        void InstallJenkins(InstallerProcess installerProcess);
        void UninstallStarterKit(InstallerProcess installerProcess);
    }
}