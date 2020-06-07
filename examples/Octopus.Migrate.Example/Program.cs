using Octopus.Migrate.Clients;
using System;
using System.Linq;

namespace Octopus.Migrate.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create OctopusDeployClient
            var octopusUrl = "https://octopus.contoso.local/"; //Replace with your own Octopus URL
            var octopusApiKey = "API-XXXXXXXXXXXXXXXXXXXXXXXXXX"; // Replace with your Octopus API key
            var client = new OctopusDeployClient(octopusUrl, octopusApiKey);

            // Get all library variables for "Prod" environment
            var libraryVariablesForProd = client.GetLibraryVariablesForEnvironment("demogroup", "Prod");
            foreach (var libraryVariable in libraryVariablesForProd)
            {
                Console.WriteLine($"{libraryVariable.Name}  :  {libraryVariable.Value}");
            }

            // Get list of all projects in Octopus
            var allProjects = client.GetAllProjects();
            foreach (var project in allProjects)
            {
                Console.WriteLine($"PROJECT: {project}");
            }

            // Get all project variables for "DemoProject"
            var projectVariables = client.GetProjectVariables("DemoProject");
            foreach (var x in projectVariables)
            {
                Console.WriteLine($"PROJECTVAR: {x.Name}  :  {x.Value}");
            }

            // Get all Octopus environments
            var environments = client.GetAllEnvironments();
            foreach (var x in environments)
            {
                Console.WriteLine($"ENVIRONMENT: {x}");
            }

            // Get all project variables for "DemoProject" which apply to "Prod" environment
            var projectVariablesForProd = client.GetProjectVariablesForEnvironment("DemoProject", "Prod");
            foreach (var projectVariable in projectVariablesForProd)
            {
                Console.WriteLine($"PROJECTVAR FOR ENV: {projectVariable.Name} : {projectVariable.Value}");
            }

            // Create AzureDevopsClient
            var azureDevopsPat = "[Your Personal Access Token]"; // Replace with your Azure Personal Access Token
            var url = "https://dev.azure.com/contoso"; // Replace with your own Azure DevOps URL
            var projectName = "NewProject"; // Replace with your Azure DevOps project name
            var variableGroupId = 123; //Replace with Id of your variable group
            var azureDevopsClient = new AzureDevopsAsyncClient(url, azureDevopsPat, projectName);

            // Update AzureDevopsLibrary with Octopus project variables
            azureDevopsClient.UpdateVariableLibrary(variableGroupId, projectVariablesForProd.ToList()).GetAwaiter().GetResult();
        }
    }
}