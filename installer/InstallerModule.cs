using LauPas.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Installer
{
    internal class InstallerModule : IModule<string[]>
    {
        public void Extend(IServiceCollection serviceCollection, string[] args)
        {
            serviceCollection.AddSingleton(Options.SetOptions(args));
        }
    }
}