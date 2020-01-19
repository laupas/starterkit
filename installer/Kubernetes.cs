using System;
using SimpleExec;

namespace installer
{
    internal static class Kubernetes
    {
        public static bool CheckIfResourceExists(string resourceType, string name, string nameSpace = null)
        {
            var nameSpaceCommand = String.Empty;
            if(!String.IsNullOrEmpty(nameSpace))
            {
                nameSpaceCommand = $"--namespace {nameSpace}";
            }
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
            var nameSpaceCommand = String.Empty;
            if(!String.IsNullOrEmpty(nameSpace))
            {
                nameSpaceCommand = $"--namespace {nameSpace}";
            }
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