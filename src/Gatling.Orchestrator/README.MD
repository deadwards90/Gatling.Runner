# Gatling Orchestrator

Gatling Orchestrator is an Azure Durable Function which will run create a set of Gatling Containers in Azure Container Instances and run a test, before returning a merged simulation result. This can be run in many regions, with as many ACI containers as you would like for a particular test.

Remember that in your script that whatever you have specified as things like requests per second will be multipled by the number of containers and regions you have specified!

## Prerequisites

You will need

- Azure Function CLI (This will be installed by Visual Studio if you are using it)
- dotnet SDK 2.2+
- Azure Subscription
  - [An Azure Application in Active Directory](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal). I would recommend it having the `Contributor` role when assigned to a subscription as it will need to manage resources. When created make a note of
    - ApplicationId
    - ApplicationSecret
    - TenantId
    - SubscriptionId
- A storage account
  - Make a note of the connection string
- Function App on consumption, everything else will not be a good use of money!

## Running

I would recommend running this through Visual Studio as I find for Azure Functions this it is a much better development experience and it makes it very easy to quickly deploy it into your own subscription.

### Locally

If you are running locally, you will need to create a `local.settings.json` file in the root folder (alongside `host.json`)

It has the following format:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ApplicationId": "<azure ad application id>",
    "ApplicationSecret": "<azure ad application secret>",
    "TenantId": "<azure ad tenant id>",
    "SubscriptionId": "<your subscription id>",
    "TestsStorageAccount": "<storage account connection string>"
  }
}
```

### In Azure

If you decide to deploy the application into an Azure Function App, then `ApplicationId`, `ApplicationSecret`, `TenantId`, `SubscriptionId`, and `TestsStorageAccount` will need to be part of the configuration instead.

### Starting a test

Firstly, create your test in Scala with the following layout

```
|   run.json
|
+---resources
|   +---feeders
|   |       my_feeder_file.csv
|   |
|   \---requestBodies
|           my_request_body.json
|
\---simulations
        MySimulation.scala
```

Where `run.json` has the the name of the simulation you wish to run

```json
{
  "SimulationClassName": "mysimulationsw.MySimulation"
}
```

Your simulation should be able to be run by Gatling in that format. If it works locally, it should work with this solution too.

Once made, zip up the solution.

With that made, decide where you would like to run your test. You can find the supported regions [here](https://docs.microsoft.com/bs-latn-ba/azure/container-instances/container-instances-region-availability) (Linux).

Once you know what regions you want, you will need to find what their "actual" name is as the ones in that list are the diplay names. Usually this is just the display name without spaces in lower case. If you have the Azure CLI installed you can check by running `az account list-locations`.

With your regions selected, you can finally start your test!

Do a form post to:

`<root-url>/api/starttest`

with

```
test: <attached file zip>
regions: <region>-<count>, <region>-<count
testid: <uuid>
```

e.g.

```
test: <attached file zip>
regions: uksouth-1, northeurope-1
testId: b44988a6-5b63-46f4-939c-ee70f3fe2a06
```

I use Insomnia for testing locally and it looks like this

![Example](https://i.imgur.com/BuTWwdT.png)

The run will then happen in the background. I've only tended to run this locally so at the moment there is no endpoint for pulling the result out but what should happen is that in your storage account in blobs under the container `results`.

Ideally there would be a way of querying this but I have not written this yet, although it should not be that difficult to do.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
