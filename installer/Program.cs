using System;
using LauPas.Common;

namespace Installer
{
    
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Starter.Create()
                    .AddModule<InstallerModule, string[]>(args)
                    .Build(args)
                    .Resolve<installer.Installer>().Install();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}