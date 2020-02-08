namespace Installer.Helper
{
    public interface IProcessHelper
    {
        int Run(string command, string args = null, string workingDirectory = null, bool throwOnError = true, bool noEcho = false);
        string Read(string command, string args = null, string workingDirectory = null, bool throwOnError = true, bool noEcho = false);
    }
}