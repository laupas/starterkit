using System;
using Microsoft.Extensions.Logging;

namespace Installer.Helper
{
    class ConsoleService : IConsoleService
    {
        private readonly Options options;
        private readonly ILogger logger;

        public ConsoleService(ILoggerFactory loggerFactory, Options options)
        {
            this.options = options;
            this.logger = loggerFactory.CreateLogger(nameof(ConsoleService));
        }
        public bool AskFormConfirmation(string question, string expectedAnswer)
        {
            if (this.options.Silent)
            {
                return true;
            }
            this.logger.LogInformation($"{question} Enter {expectedAnswer} to continue or any other key to cancel:");
            Console.Write($" Enter {expectedAnswer} to continue or any other key to cancel: ");
            return Console.ReadLine().Equals(expectedAnswer);
        }
    }
}