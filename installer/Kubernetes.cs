using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using SimpleExec;
using static Bullseye.Targets;
using static SimpleExec.Command;
using static System.Console;
namespace installer
{
    internal static class Kubernetes
    {
        
        public static void InstallResourceIfNotExists(string name, string resourceType, string nameSpace = null, string arguments = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);

            if (!CheckIfResourceExists(resourceType.Split(' ').First(), name, nameSpace))
            {
                WriteLine($"Create {resourceType} {name}");
                Run("kubectl", $"create {resourceType} {name} {arguments} {nameSpaceCommand}");
            }
        }

        public static void WriteHeader(string name)
        {
            WriteLine("####################################");
            WriteLine(name);
            WriteLine("####################################");
        }

        public static void InstallApplicationeIfNotExists(string name, string repo, string nameSpace = null, string arguments = null, params string[] checkRollout)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);

            if (!CheckIfApplicationExists(name, nameSpace))
            {
                WriteLine($"Create {name} {repo}");
                Run("helm", $"install {name} {repo} {arguments} {nameSpaceCommand}");
                checkRollout?.ToList().ForEach(check =>
                    {
                        
                        Run("kubectl",
                                $"rollout status {check} {nameSpaceCommand}");
                });
                
            }
        }

        private static string CreateNameSpaceCommand(string nameSpace)
        {
            var nameSpaceCommand = String.Empty;
            if (!String.IsNullOrEmpty(nameSpace))
            {
                nameSpaceCommand = $"--namespace {nameSpace}";
            }

            return nameSpaceCommand;
        }

        public static bool CheckIfResourceExists(string resourceType, string name, string nameSpace = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);
            var namespaces = Command.Read("kubectl", $"{nameSpaceCommand} get {resourceType}", noEcho: true);
            if(namespaces.Contains(name))
            {
                Console.WriteLine($"{resourceType} {name} exists.");
                return true;
            }
            return false;
        }

        public static bool CheckIfApplicationExists(string name, string nameSpace = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);
            var namespaces = Command.Read("helm", $"list {nameSpaceCommand}", noEcho: true);
            if(namespaces.Contains(name))
            {
                Console.WriteLine($"Application {name} exists.");
                return true;
            }
            return false;
        }
    }
}