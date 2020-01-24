using System.Net;
using System.Runtime.CompilerServices;
using installer.Helper;

namespace Installer
{
    static class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            Starter.Build(args);
            Starter.Get<Installer>().Install();
        }
    }
}