using Microsoft.TeamFoundation.Common;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Migrate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Octopus.Migrate.Clients
{
    public interface IOctopusDeployAsyncClient
    {
        Task<IEnumerable<VariableModel>> GetLibraryVariablesForEnvironment(string libraryName, string environment);

        Task<IEnumerable<string>> GetAllProjects();

        Task<IEnumerable<VariableModel>> GetProjectVariables(string projectName);

        Task<IEnumerable<string>> GetAllEnvironments();

        Task<IEnumerable<VariableModel>> GetProjectVariablesForEnvironment(string projectName, string environment);
    }

    public class OctopusDeployAsyncClient : IOctopusDeployAsyncClient
    {
        private OctopusServerEndpoint _octopusServerEndpoint;

        public OctopusDeployAsyncClient(string octopusUrl, string octopusApiKey)
        {
            _octopusServerEndpoint = new OctopusServerEndpoint(octopusUrl, octopusApiKey);
        }

        /// <summary>
        /// Gets all variables in a Library Variable Set which apply to an environment. This includes unscoped variables (which apply to all environments).
        /// </summary>
        /// <param name="libraryName">The name of the Library Variable Set</param>
        /// <param name="environment">The environment (scope) name (e.g. "Prod")</param>
        public async Task<IEnumerable<VariableModel>> GetLibraryVariablesForEnvironment(string libraryName, string environment)
        {
            using (var client = await OctopusAsyncClient.Create(_octopusServerEndpoint))
            {
                var repository = client.CreateRepository();
                var libraryVariableSetResource = await GetOctopusLibrarySet(libraryName, repository);
                var environmentResource = await GetEnvironmentResource(environment, repository);
                var variablesSetResource = await repository.VariableSets.Get(libraryVariableSetResource.VariableSetId);
                var variables = variablesSetResource.Variables
                .Where(x => x.Scope.IsNullOrEmpty() ||
                 (x.Scope.ContainsKey(ScopeField.Environment) && x.Scope[ScopeField.Environment].Contains(environmentResource.Id)))
                .Select(x =>
                {
                    if (x.IsSensitive)
                    {
                        return new VariableModel
                        {
                            Name = x.Name,
                            Value = "[SECRET]"
                        };
                    }
                    return new VariableModel
                    {
                        Name = x.Name,
                        Value = x.Value
                    };
                });
                return variables.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns a list of all Octopus project names, sorted by project name
        /// </summary>
        public async Task<IEnumerable<string>> GetAllProjects()
        {
            using (var client = await OctopusAsyncClient.Create(_octopusServerEndpoint))
            {
                var repository = client.CreateRepository();
                var allProjects = await repository.Projects.GetAll();
                var projectNames = allProjects.Select(x => x.Name)
                    .ToList();
                projectNames.Sort(StringComparer.OrdinalIgnoreCase);
                return projectNames;
            }
        }

        /// <summary>
        /// Returns a list of variables for an Octopus project, sorted by variable name
        /// </summary>
        /// <param name="projectName">The name of the Octopus project</param>
        public async Task<IEnumerable<VariableModel>> GetProjectVariables(string projectName)
        {
            using (var client = await OctopusAsyncClient.Create(_octopusServerEndpoint))
            {
                var repository = client.CreateRepository();
                var project = await GetProjectResource(projectName, repository);
                var variableSetResource = await repository.VariableSets.Get(project.VariableSetId);
                var variables = variableSetResource.Variables.Select(x =>
                {
                    if (x.IsSensitive)
                    {
                        return new VariableModel
                        {
                            Name = x.Name,
                            Value = "[SECRET]"
                        };
                    }
                    return new VariableModel
                    {
                        Name = x.Name,
                        Value = x.Value
                    };
                })
                    .ToList();

                return variables.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns a list of all Environments, sorted by name
        /// </summary>
        public async Task<IEnumerable<string>> GetAllEnvironments()
        {
            using (var client = await OctopusAsyncClient.Create(_octopusServerEndpoint))
            {
                var repository = client.CreateRepository();
                var environmentResources = await repository.Environments.GetAll();
                var environments = environmentResources.Select(x => x.Name).ToList();
                environments.Sort(StringComparer.OrdinalIgnoreCase);
                return environments;
            }
        }

        /// <summary>
        /// Returns a list of project variables applied to a particular environment. This includes unscoped variables (which apply to all environments).
        /// </summary>
        /// <param name="projectName">The name of the Octopus project</param>
        /// <param name="environment">The environment (scope) name (e.g. "Prod")</param>
        public async Task<IEnumerable<VariableModel>> GetProjectVariablesForEnvironment(string projectName, string environment)
        {
            using (var client = await OctopusAsyncClient.Create(_octopusServerEndpoint))
            {
                var repository = client.CreateRepository();
                var project = await GetProjectResource(projectName, repository);
                var environmentResource = await GetEnvironmentResource(environment, repository);

                var variableSetResource = await repository.VariableSets.Get(project.VariableSetId);

                var variables = variableSetResource.Variables
                    .Where(x => x.Scope.Values.IsNullOrEmpty() ||
                        (x.Scope.ContainsKey(ScopeField.Environment) && x.Scope[ScopeField.Environment].Contains(environmentResource.Id))
                    )
                    .Select(x =>
                    {
                        if (x.IsSensitive)
                        {
                            return new VariableModel
                            {
                                Name = x.Name,
                                Value = "[SECRET]"
                            };
                        }
                        return new VariableModel
                        {
                            Name = x.Name,
                            Value = x.Value
                        };
                    })
                    .ToList();

                return variables.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<LibraryVariableSetResource> GetOctopusLibrarySet(string libraryName, IOctopusAsyncRepository repository)
        {
            var librarySet = await repository.LibraryVariableSets.FindByName(libraryName);
            if (librarySet == null)
            {
                throw new Exception("Library set does not exist");
            }
            return librarySet;
        }

        private async Task<EnvironmentResource> GetEnvironmentResource(string environment, IOctopusAsyncRepository repository)
        {
            var environmentResource = await repository.Environments.FindByName(environment);
            if (environmentResource == null)
            {
                throw new Exception("Environment does not exist");
            }
            return environmentResource;
        }

        private async Task<ProjectResource> GetProjectResource(string projectName, IOctopusAsyncRepository repository)
        {
            var project = await repository.Projects.FindByName(projectName);
            if (project == null)
            {
                throw new Exception($"Project with name of '{projectName}' not found");
            }
            return project;
        }
    }
}