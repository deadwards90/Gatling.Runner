using System;
using System.Threading.Tasks;
using Gatling.Orchestrator.Models;
using Gatling.Orchestrator.Orchestrators;
using Gatling.Orchestrator.Services;
using Microsoft.Azure.WebJobs;

namespace Gatling.Orchestrator.Activities
{
    public class AciActivities
    {
        private readonly IContainerInstanceService _containerInstanceService;

        public AciActivities( 
            IContainerInstanceService containerInstanceService)
        {
            _containerInstanceService = containerInstanceService;
        }

        [FunctionName(nameof(CreateAciInRegion))]
        public async Task<string> CreateAciInRegion([ActivityTrigger]DurableActivityContextBase context)
        {
            var region = context.GetInput<string>();
            var regionId = Guid.NewGuid().ToString();

            return await _containerInstanceService.CreateContainerGroup(
                region, regionId);
        }

        [FunctionName(nameof(DeleteAciInRegion))]
        public async Task DeleteAciInRegion([ActivityTrigger]DurableActivityContextBase context)
        {
            var containerName = context.GetInput<string>();
            await _containerInstanceService.DeleteContainerGroup(containerName);
        }
    }
}
