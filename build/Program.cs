using Microsoft.Extensions.Logging;

using static Bullseye.Targets;
using static LauPas.BuildExtensions;

namespace Build
{
    static class Build
    {
        static void Main(string[] args)
        {
            Target("default", DependsOn(new string[]
            {
                "init",
                "restore",
                "build", 
                "test", 
                "pack"
            }));

            var buildNumber = string.Empty;

            Target("init", () =>
            {
                if (IsEnvVariableExisting("GITHUB_RUN_NUMBER"))
                {
                    Logger.LogInformation($"BuildAgent run");
                    SetConfigFile("BuildConfig.yml");
                }
                else
                {
                    Logger.LogInformation($"Local run");
                    SetConfigFile(".BuildConfig.yml");
                }
                
                var versionPrefix = GetConfigValue("VERSION_PREFIX", "0.0");
                var runNumber = GetConfigValue("GITHUB_RUN_NUMBER", "0-local");
                buildNumber = $"{versionPrefix}.{runNumber}";
                Logger.LogInformation($"BuildNumber: {buildNumber}");
            });

            Target("restore", DependsOn("init"), () =>
            {
                RunProcess("dotnet", $"restore", workingDirectory: "./..");
            });

            Target("build", DependsOn("init"), () =>
            {
                RunProcess("dotnet", $"build -c Release -p:Version={buildNumber}", "./..");
            });

            Target("test", DependsOn("init"), () =>
            {
                RunProcess("dotnet", $"test -c Release --no-build", "./..");
            });

            Target("pack", DependsOn("init"), () =>
            {
//                var nugetOrgToken = GetConfigValue<string>("NUGET_ORG_API_KEY");
                RunProcess("docker", $"build -t devopsfamily/starterkit.installer:{buildNumber} .", "./..");
            });
            
            RunTargetAndExit(args);
        }
    }
}