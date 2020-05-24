using Installer.Helper;

namespace Installer
{
    internal interface IRancherTargets
    {
        void InstallRancher(InstallerProcess installerProcess);
    }
}