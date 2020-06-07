using Microsoft.TeamFoundation.Common;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Migrate.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Migrate.Clients
{
    public interface IOctopusDeployClient
    {
        IEnumerable<VariableModel> GetLibraryVariablesForEnvironment(string libraryName, string environment);

        IEnumerable<string> GetAllProjects();

        IEnumerable<VariableModel> GetProjectVariables(string projectName);

        IEnumerable<string> GetAllEnvironments();

        IEnumerable<VariableModel> GetProjectVariablesForEnvironment(string projectName, string environment);
    }

    public class OctopusDeployClient : IOctopusDeployClient
    {
        private OctopusServerEndpoint _octopusServerEndpoint;
        private OctopusRepository _octopusRepository;

        public OctopusDeployClient(string octopusUrl, string octopusApiKey)
        {
            _octopusServerEndpoint = new OctopusServerEndpoint(octopusUrl, octopusApiKey);
            _octopusRepository = new OctopusRepository(_octopusServerEndpoint);
        }

        /// <summary>
        /// Gets all variables in a Library Variable Set which apply to an environment. This includes unscoped variables (which apply to all environments).
        /// </summary>
        /// <param name="libraryName">The name of the Library Variable Set</param>
        /// <param name="environment">The environment (scope) name (e.g. "Prod")</param>
        public IEnumerable<VariableModel> GetLibraryVariablesForEnvironment(string libraryName, string environment)
        {
            var librarySet = _octopusRepository.LibraryVariableSets.FindByName(libraryName);
            if (librarySet == null)
            {
                throw new Exception("Library set does not exist");
            }
            // Variables are scoped to environments, so we need to first find the environment to get its Id
            var environmentResource = _octopusRepository.Environments.FindByName(environment);
            if (environmentResource == null)
            {
                throw new Exception("Environment does not exist");
            }

            var libraryVariableSetResource = _octopusRepository.LibraryVariableSets.FindByName(libraryName);
            var variablesSetResource = _octopusRepository.VariableSets.Get(libraryVariableSetResource.VariableSetId);

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

        /// <summary>
        /// Returns a list of all Octopus project names, sorted by project name
        /// </summary>
        public IEnumerable<string> GetAllProjects()
        {
            var allProjects = _octopusRepository.Projects.GetAll();
            var projectNames = allProjects.Select(x => x.Name)
                .ToList();
            projectNames.Sort(StringComparer.OrdinalIgnoreCase);
            return projectNames;
        }

        /// <summary>
        /// Returns a list of variables for an Octopus project, sorted by variable name
        /// </summary>
        /// <param name="projectName">The name of the Octopus project</param>
        public IEnumerable<VariableModel> GetProjectVariables(string projectName)
        {
            var project = _octopusRepository.Projects.FindByName(projectName);
            var variableSetResource = _octopusRepository.VariableSets.Get(project.VariableSetId);
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

        /// <summary>
        /// Returns a list of all Environments, sorted by name
        /// </summary>
        public IEnumerable<string> GetAllEnvironments()
        {
            var environmentResources = _octopusRepository.Environments.GetAll();
            var environments = environmentResources.Select(x => x.Name).ToList();
            environments.Sort(StringComparer.OrdinalIgnoreCase);
            return environments;
        }

        /// <summary>
        /// Returns a list of project variables applied to a particular environment. This includes unscoped variables (which apply to all environments).
        /// </summary>
        /// <param name="projectName">The name of the Octopus project</param>
        /// <param name="environment">The environment (scope) name (e.g. "Prod")</param>
        public IEnumerable<VariableModel> GetProjectVariablesForEnvironment(string projectName, string environment)
        {
            var project = _octopusRepository.Projects.FindByName(projectName);
            if (project == null)
            {
                throw new Exception($"Project with name of '{projectName}' not found");
            }
            var environmentResource = _octopusRepository.Environments.FindByName(environment);
            if (environmentResource == null)
            {
                throw new Exception("Environment does not exist");
            }

            var variableSetResource = _octopusRepository.VariableSets.Get(project.VariableSetId);

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
}