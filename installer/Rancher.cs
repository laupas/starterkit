using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using static SimpleExec.Command;

namespace installer
{
    internal static class Rancher
    {
        public static void WaitUntilIsUpAndReady(RestClient client)
        {
            Console.Write("Wait until Rancher is up and ready...");
            var pingRequest = new RestRequest("/ping");
            var result = client.Get(pingRequest);

            while (!result.Content.Equals("pong"))
            {
                Console.Write(".");
                result = client.Get(pingRequest);
                Thread.Sleep(500);
            }

            Console.WriteLine("Done");
        }

        public static string Login(RestClient client)
        {
            Console.Write("Login to Rancher...");
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

        public static void ChangePassword(RestClient client, string token)
        {
            Console.Write("Change Password...");
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

            Console.WriteLine("Done");
        }

        public static void SetServerUrl(RestClient client, string rancherUrl)
        {
            // // var rancherUrl = Read("kubectl", "get services -n cattle-system -o=json | jq .items[0].spec.clusterIP -r");
            // var jsonString = Read("kubectl", "get services -n cattle-system -o=json");
            // dynamic json = JsonConvert.DeserializeObject(jsonString);
            // var rancherUrl = json.items[0].spec.clusterIP;

            Console.Write("Set Server-Url...");
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

            Console.WriteLine("Done");
        }

        public static void WaitUntilIsActive(RestClient client)
        {
            Console.Write("Wait until Rancher is active...");
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

            Console.WriteLine("Done");
        }

        public static void ExecuteUpdatesForDockerDesktop()
        {
            Console.WriteLine("Update some pods to work in Docker Desktop");
            var serviceIpJsonString = Read("kubectl", "get services -n cattle-system -o=json");
            dynamic serviceIpJson = JsonConvert.DeserializeObject(serviceIpJsonString);
            var serviceIp = serviceIpJson.items[0].spec.clusterIP.Value;

            Run("kubectl", $"set env deployments/cattle-cluster-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system");
            // Run("kubectl", $"set env deployments/cattle-cluster-agent CATTLE_SERVER={serviceIp}  -n cattle-system");

            Run("kubectl", $"set env daemonset/cattle-node-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system)  -n cattle-system");
            // Run("kubectl", $"set env daemonset/cattle-node-agent CATTLE_SERVER={serviceIp} -n cattle-system)  -n cattle-system");
        }

    }
}