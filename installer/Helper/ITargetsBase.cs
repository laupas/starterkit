namespace Installer.Helper
{
    public interface ITargetsBase
    {
        /// <summary>
        /// Define which targets in which order should be executed.
        /// </summary>
        /// <returns>Dictionary with Targets to be executed. As lower the number, as earlier the Action will be executed.</returns>
        void DefineTargetsToExecute(InstallerProcess installerProcess);
    }
}