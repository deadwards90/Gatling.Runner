using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace Gatling.Orchestrator.Services
{
    public interface IContainerInstanceService
    {
        Task<string> CreateContainerGroup(string regionName,
            string containerGroupName);

        Task DeleteContainerGroup(
            string containerGroupName);
    }

    public class ContainerInstanceService : IContainerInstanceService
    {
        private readonly IAzure _azure;

        public ContainerInstanceService(IAzure azure)
        {
            _azure = azure;
        }

        public async Task<string> CreateContainerGroup(
            string regionName,
            string containerGroupName)
        {
            var resourceGroupName = $"{containerGroupName}-rg";
            IResourceGroup resGroup;

            if (await _azure.ResourceGroups.ContainAsync(resourceGroupName))
            {
                resGroup = _azure.ResourceGroups.GetByName(resourceGroupName);
            }
            else
            {
                resGroup = await _azure.ResourceGroups.Define(resourceGroupName)
                    .WithRegion(regionName)
                    .CreateAsync();
            }

            var azureRegion = resGroup.Region;

            var containerGroup = _azure.ContainerGroups.Define(containerGroupName)
                .WithRegion(azureRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance("gatling")
                .WithImage("dantheman999/gatling.runner:master")
                    .WithExternalTcpPort(80)
                    .WithExternalTcpPort(443)
                    .WithCpuCoreCount(1.0)
                    .WithMemorySizeInGB(1)
                .Attach()
                .WithDnsPrefix(containerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();

            return containerGroup.Fqdn;
        }

        public async Task DeleteContainerGroup(
            string containerGroupName)
        {
            var resourceGroupName = $"{containerGroupName}-rg";
            await _azure.ContainerGroups.DeleteByResourceGroupAsync(resourceGroupName, containerGroupName);
            await _azure.ResourceGroups.DeleteByNameAsync(resourceGroupName);
        }
    }
}
