# Azure + Gatling - Running Gatling in the Cloud

The projects in the repo contain applications that help to run Gatling in an automated fashion in Azure.

Currently there is:

- `base-image`: The folder containing a Gatling Docker container that can also run .NET Core applications.
- `src/Gatling.Runner`: An API for running Gatling tests on a machine. Combined with the Gatling container above this allows remote running of Gatling tests in the Cloud.
- `src/Gatling.Orchestrator`: A Durable Function for orchestrating running Gatling tests across multiple Azure Container Instances with returns a single combined result.
