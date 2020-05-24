namespace Installer.Helper
{
    public interface IConsoleService
    {
        public bool AskFormConfirmation(string question, string expectedAnswer);
    }
}