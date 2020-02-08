using System;

namespace installer.Helper
{
    internal class InstallerException : Exception
    {
        public InstallerException(string message) : base(message)
        {
            
        }
    }
}