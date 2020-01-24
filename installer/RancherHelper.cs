using System;
using System.Net;
using System.Threading;
using installer.Helper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace installer
{
    public class RancherHelper
    {
        private readonly IProcessHelper processHelper;
        private readonly ILogger logger;

        public RancherHelper(ILoggerFactory loggerFactory, IProcessHelper processHelper)
        {
            this.processHelper = processHelper;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        
        public void WaitUntilIsUpAndReady(RestClient client)
        {
            this.logger.LogInformation("Wait until Rancher is up and ready...");
            var pingRequest = new RestRequest("/ping");
            var result = client.Get(pingRequest);

            while (!result.Content.Equals("pong"))
            {
                Console.Write(".");
                result = client.Get(pingRequest);
                Thread.Sleep(500);
            }

            this.logger.LogInformation("Done");
        }

        public string Login(RestClient client)
        {
            this.logger.LogInformation("Login to Rancher...");
            var loginRequest = new RestRequest("/v3-public/localProviders/local?action=login")
            {
                RequestFormat = DataFormat.Json
            };

            loginRequest.AddJsonBody(JObject.FromObject(new
            {
                username = "admin",
                password = "admin"
            }).ToString());

            var loginRequestResult = client.Post(loginRequest);
            string token = JsonConvert.DeserializeObject<dynamic>(loginRequestResult.Content).token;
            if (loginRequestResult.StatusCode != HttpStatusCode.Created)
                throw new Exception(loginRequestResult.ErrorMessage);
            Console.WriteLine("Done");
            return token;
        }

        public void ChangePassword(RestClient client, string token)
        {
            this.logger.LogInformation("Change Password...");
            var changePasswordRequest = new RestRequest("/v3/users?action=changepassword")
            {
                RequestFormat = DataFormat.Json,
            };

            changePasswordRequest.AddJsonBody(JObject.FromObject(new
            {
                currentPassword = "admin",
                newPassword = "admin"
            }).ToString());

            var changePasswordResult = client.Post(changePasswordRequest);
            if (changePasswordResult.StatusCode != HttpStatusCode.OK)
                throw new Exception(changePasswordResult.ErrorMessage);

            this.logger.LogInformation("Done");
        }

        public void SetServerUrl(RestClient client, string rancherUrl)
        {
            // // var rancherUrl = Read("kubectl", "get services -n cattle-system -o=json | jq .items[0].spec.clusterIP -r");
            // var jsonString = Read("kubectl", "get services -n cattle-system -o=json");
            // dynamic json = JsonConvert.DeserializeObject(jsonString);
            // var rancherUrl = json.items[0].spec.clusterIP;

            this.logger.LogInformation("Set Server-Url...");
            var setServerUrlRequest = new RestRequest("/v3/settings/server-url")
            {
                RequestFormat = DataFormat.Json,
            };

            setServerUrlRequest.AddJsonBody(JObject.FromObject(new
            {
                name = "server-url",
                value = $"{rancherUrl}"
            }).ToString());

            var setServerUrlResult = client.Put(setServerUrlRequest);
            if (setServerUrlResult.StatusCode != HttpStatusCode.OK)
                throw new Exception(setServerUrlResult.ErrorMessage);

            this.logger.LogInformation("Done");
        }

        public void WaitUntilIsActive(RestClient client)
        {
            this.logger.LogInformation("Wait until Rancher is active...");
            var pingRequest = new RestRequest("/v3/clusters/local");
            var result = client.Get(pingRequest);
            dynamic json = JsonConvert.DeserializeObject(result.Content);
            while (!json.state.Value.Equals("active"))
            {
                Console.Write(".");
                Thread.Sleep(1000);
                result = client.Get(pingRequest);
                json = JsonConvert.DeserializeObject(result.Content);
            }

            this.logger.LogInformation("Done");
        }

        public void ExecuteUpdatesForDockerDesktop()
        {
            this.logger.LogInformation("Update some pods to work in Docker Desktop");
            var serviceIpJsonString = this.processHelper.Read("kubectl", "get services -n cattle-system -o=json");
            dynamic serviceIpJson = JsonConvert.DeserializeObject(serviceIpJsonString);
            var serviceIp = serviceIpJson.items[0].spec.clusterIP.Value;

            this.processHelper.Run("kubectl", $"set env deployments/cattle-cluster-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system");
            // Run("kubectl", $"set env deployments/cattle-cluster-agent CATTLE_SERVER={serviceIp}  -n cattle-system");

            this.processHelper.Run("kubectl", $"set env daemonset/cattle-node-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system)  -n cattle-system");
            // Run("kubectl", $"set env daemonset/cattle-node-agent CATTLE_SERVER={serviceIp} -n cattle-system)  -n cattle-system");
            this.logger.LogInformation("Done");
        }
    }
}