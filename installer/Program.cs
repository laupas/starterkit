using LauPas.Common;

namespace Installer
{
    
    static class Program
    {
        static void Main(string[] args)
        {
            Starter.Create()
                .AddModule<InstallerModule, string[]>(args)
                .Build(args)
                .Resolve<installer.Installer>().Install();
        }
    }
}