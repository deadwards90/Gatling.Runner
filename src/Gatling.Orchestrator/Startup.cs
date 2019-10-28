using System;
using Gatling.Orchestrator;
using Gatling.Orchestrator.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Gatling.Orchestrator
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            var appId = Environment.GetEnvironmentVariable("ApplicationId");
            var appSecret = Environment.GetEnvironmentVariable("ApplicationSecret");
            var tenantId = Environment.GetEnvironmentVariable("TenantId");
            var subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("TestsStorageAccount");

            var environment = AzureEnvironment.AzureGlobalCloud;

            var credentials = new AzureCredentialsFactory()
                .FromServicePrincipal(appId, appSecret, tenantId, environment);

            var azure = Azure
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddSingleton<IContainerInstanceService, ContainerInstanceService>();
            builder.Services.AddSingleton<IGatlingService, GatlingService>();
            builder.Services.AddSingleton(_ => azure);
            builder.Services.AddSingleton(_ =>
            {
                var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                return storageAccount.CreateCloudBlobClient();
            });
        }
    }
}
