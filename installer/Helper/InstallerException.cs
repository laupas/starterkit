using System;

namespace Installer.Helper
{
    internal class InstallerException : Exception
    {
        public InstallerException(string message) : base(message)
        {
            
        }
    }
}