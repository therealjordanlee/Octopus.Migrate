using Octopus.Migrate.Clients;
using System;
using System.Linq;

namespace Octopus.Migrate.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Extracting variables from Octopus Deploy
            var octopusUrl = "https://octopus.contoso.local"; //Replace with your own Octopus URL
            var octopusApiKey = "[Your API Key]"; // Replace with your Octopus API key

            var octopusService = new OctopusDeployClient(octopusUrl, octopusApiKey);
            var octopusVariables = octopusService.GetLibraryVariables("demo-library", "Prod");

            foreach (var variable in octopusVariables)
            {
                Console.WriteLine($"{variable.Name} : {variable.Value}");
            }
            Console.WriteLine($"Retrieved {octopusVariables.Count()} variables from Octopus");

            // Writing variables into Azure DevOps
            var azureDevopsPat = "[Your Personal Access Token]"; // Replace with your Azure Personal Access Token
            var url = "https://dev.azure.com/contoso"; // Replace with your own Azure DevOps URL
            var projectName = "NewProject"; // Replace with your Azure DevOps project name
            var variableGroupId = 123; //Replace with Id of your variable group

            var azureDevopsClient = new AzureDevopsClient(url, azureDevopsPat, projectName);
            azureDevopsClient.UpdateVariableLibrary(variableGroupId, octopusVariables.ToList()).GetAwaiter().GetResult();
        }
    }
}