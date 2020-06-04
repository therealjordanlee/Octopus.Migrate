# Octopus.Migrate [![Build Status](https://dev.azure.com/jordanleenet/Public/_apis/build/status/therealjordanlee.Octopus.Migrate?branchName=master)](https://dev.azure.com/jordanleenet/Public/_build/latest?definitionId=2&branchName=master)

Octopus.Migrate is a .NET Standard library for extracting Variable Sets from Octopus Deploy and adding them to Azure Devops Variable Groups.

##  Getting Started
Install from NuGet.

```
Install-Package Octopus.Migrate
```

## Sample Usage

```
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

            var octopusDeployClient = new OctopusDeployClient(octopusUrl, octopusApiKey);
            var libraryVariables = octopusDeployClient.GetLibraryVariablesForEnvironment("demo-library", "Prod");

            foreach (var variable in libraryVariables)
            {
                Console.WriteLine($"{variable.Name} : {variable.Value}");
            }
            Console.WriteLine($"Retrieved {libraryVariables.Count()} variables from Octopus");

            // Writing variables into Azure DevOps
            var azureDevopsPat = "[Your Personal Access Token]"; // Replace with your Azure Personal Access Token
            var url = "https://dev.azure.com/contoso"; // Replace with your own Azure DevOps URL
            var projectName = "NewProject"; // Replace with your Azure DevOps project name
            var variableGroupId = 123; //Replace with Id of your variable group

            var azureDevopsClient = new AzureDevopsClient(url, azureDevopsPat, projectName);
            azureDevopsClient.UpdateVariableLibrary(variableGroupId, libraryVariables.ToList()).GetAwaiter().GetResult();
        }
    }
}
```

A sample solution is available in the [Example folder](/Example).

## License
See [LICENSE.md](LICENSE.md).

