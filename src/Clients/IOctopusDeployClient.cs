using Octopus.Migrate.Models;
using System.Collections.Generic;

namespace Octopus.Migrate.Clients
{
    public interface IOctopusDeployClient
    {
        IEnumerable<VariableModel> GetLibraryVariablesForEnvironment(string libraryName, string environment);
        IEnumerable<string> GetAllProjects();
        IEnumerable<VariableModel> GetProjectVariables(string projectName);
        IEnumerable<string> GetAllEnvironments();
    }
}
