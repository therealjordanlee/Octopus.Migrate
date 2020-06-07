using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Octopus.Migrate.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octopus.Migrate.Clients
{
    public class AzureDevopsAsyncClient
    {
        private VssConnection _connection;
        private string _projectName;

        public AzureDevopsAsyncClient(string azureDevopsUrl, string personalAccessToken, string projectName)
        {
            VssCredentials creds = new VssBasicCredential(string.Empty, personalAccessToken);
            _connection = new VssConnection(new Uri(azureDevopsUrl), creds);
            _projectName = projectName;
        }

        /// <summary>
        /// Updates a variable group using a list of <VariableEntity>
        /// </summary>
        /// <param name="variableGroupId">The Id of the Variable Group in Azure DevOps</param>
        /// <param name="variables">A list of key-value pairs to add or update in the Variable Group</param>
        /// <returns></returns>
        public async Task UpdateVariableLibrary(int variableGroupId, List<VariableModel> variables)
        {
            var taskClient = _connection.GetClient<TaskAgentHttpClient>();
            var varGroup = await taskClient.GetVariableGroupAsync(_projectName, variableGroupId);

            variables.ForEach(x =>
            {
                if (!varGroup.Variables.ContainsKey(x.Name))
                    varGroup.Variables.Add(x.Name, x.Value);
                else
                    varGroup.Variables[x.Name] = x.Value;
            });

            VariableGroupParameters varGroupParam = new VariableGroupParameters();
            varGroupParam.Variables = varGroup.Variables;
            varGroupParam.Name = varGroup.Name;

            await taskClient.UpdateVariableGroupAsync(_projectName, variableGroupId, varGroupParam);
        }
    }
}