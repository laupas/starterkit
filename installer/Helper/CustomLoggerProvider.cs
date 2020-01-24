using System;
using Microsoft.Extensions.Logging;

namespace Installer.Helper
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        private class CustomConsoleLogger : ILogger
        {
            private readonly string categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                this.categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!this.IsEnabled(logLevel))
                {
                    return;
                }

                var color = Console.ForegroundColor;

                switch (logLevel)
                {
                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }

                Console.Write($"{logLevel.ToString().Substring(0,4)}: {this.categoryName}: ");
                Console.ForegroundColor = color;
                Console.WriteLine($"{formatter(state, exception)}");
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}