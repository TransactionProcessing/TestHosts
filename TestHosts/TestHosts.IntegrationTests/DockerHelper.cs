using Shared.IntegrationTesting;
using Shared.Serialisation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Shared.IntegrationTesting.TestContainers;

namespace TestHosts.IntegrationTests
{
    public class DockerHelper : Shared.IntegrationTesting.TestContainers.DockerHelper
    {
        public IAgencyBankingClient AgencyBankingClient;

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        public async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices)
        {
            await base.StartContainersForScenarioRun(scenarioName, dockerServices);
                                   
            Func<String, String> agencyBankingBaseAddressResolver = api => $"http://localhost:{this.TestHostServicePort}";
            HttpClient httpClient = new HttpClient();
            this.AgencyBankingClient = new AgencyBankingClient(agencyBankingBaseAddressResolver,httpClient, this.Serialise, this.Deserialise);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }

        
        
        public override async Task CreateSubscriptions(){
            // No subscriptions needed
        }

        public override ContainerBuilder SetupTestHostContainer()
        {
            this.Trace("About to Start Test Hosts Container");

            Dictionary<String, String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add("ConnectionStrings:TestBankReadModel", this.SetConnectionString("TestBankReadModel", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:PataPawaReadModel", this.SetConnectionString("PataPawaReadModel", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:AgencyBankingReadModel", this.SetConnectionString("AgencyBankingReadModel", this.UseSecureSqlServerDatabase));
            //environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            Dictionary<String, String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.TestHost);

            foreach (KeyValuePair<String, String> additionalEnvironmentVariable in additionalEnvironmentVariables)
            {
                environmentVariables.Add(additionalEnvironmentVariable.Key, additionalEnvironmentVariable.Value);
            }

            (String imageName, Boolean useLatest) imageDetails = this.GetImageDetails(ContainerType.TestHost).Data;

            ContainerBuilder testHostContainer = new ContainerBuilder()
                .WithName(this.TestHostContainerName)  // similar to WithName()
                .WithImage(imageDetails.imageName)
                .WithEnvironment(environmentVariables)
                .MountHostFolder(this.DockerPlatform, this.HostTraceFolder)
                .WithPortBinding(DockerPorts.TestHostPort, true);

            return testHostContainer;
        }
    }
}
