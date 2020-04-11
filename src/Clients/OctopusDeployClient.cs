using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.Services.Common;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Migrate.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Migrate
{
    public class OctopusDeployClient
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
        public IEnumerable<VariableEntity> GetLibraryVariables(string libraryName, string environment)
        {
            var librarySet = _octopusRepository.LibraryVariableSets.FindByName(libraryName);
            if (librarySet == null)
            {
                throw new Exception("Library set does not exist");
            }
            var environmentResource = _octopusRepository.Environments.FindByName(environment);
            if (environmentResource == null)
            {
                throw new Exception("Environment does not exist");
            }

            ScopeValue prodScope = new ScopeValue(environmentResource.Id);

            var variables = _octopusRepository.VariableSets.Get(librarySet.VariableSetId);

            // Get unscoped variables
            var unscopedVariables = variables.Variables.Where(x => x.Scope.IsNullOrEmpty());

            // Convert octopus VariableResource into VariableEntity
            var results = new List<VariableEntity>();
            unscopedVariables.ForEach(x => results.Add(new VariableEntity { Name = x.Name, Value = x.Value }));

            // get variables scoped to the requested environment
            if (!environment.IsNullOrEmpty())
            {
                var scopedVariables = variables.Variables
                .Where(x => x.Scope.ContainsKey(ScopeField.Environment))
                .Where(x => x.Scope[ScopeField.Environment].Contains(environmentResource.Id))
                .ToList()
                .OrderBy(x => x.Name);

                scopedVariables.ForEach(x => results.Add(new VariableEntity { Name = x.Name, Value = x.Value }));
            }

            results.OrderBy(x => x.Name);
            return results;
        }
    }
}