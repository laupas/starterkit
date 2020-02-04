using System;
using Microsoft.Extensions.Logging;

namespace Installer.Helper
{
    internal static class LogExtensions
    {
        public static void AppendToLastLog(this ILogger logger, string text)
        {
            Console.Write(text);
        }
    }
}