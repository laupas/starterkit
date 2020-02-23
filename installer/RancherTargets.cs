using System;
using System.Net;
using System.Threading;
using Installer.Helper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Installer
{
    internal class RancherTargets
    {
        private readonly ILogger logger;
        private readonly Options options;
        private readonly IKubernetesHelper kubernetesHelper;
        private string token;

        public RancherTargets(ILoggerFactory loggerFactory, Options options, IKubernetesHelper kubernetesHelper) 
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
            this.options = options;
            this.kubernetesHelper = kubernetesHelper;
        }

        internal void InstallIngress()
        {
            var ssl = "self";
            this.logger.LogInformation("Install Ingress");
            this.kubernetesHelper.InstallResourceIfNotExists("ingress-nginx", "namespace", string.Empty);
            this.kubernetesHelper.InstallResourceIfNotExists(
                "ingress-default-cert",
                "secret tls",
                "ingress-nginx",
                $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");
                    
            this.kubernetesHelper.InstallApplicationeIfNotExists(
                "ingress-nginx",
                "stable/nginx-ingress",
                "ingress-nginx",
                "--set controller.extraArgs.default-ssl-certificate=ingress-nginx/ingress-default-cert",
                "deployment.apps/ingress-nginx-nginx-ingress-controller",
                "deployment.apps/ingress-nginx-nginx-ingress-default-backend");        
        }

        internal void InstallRancher()
        {
            var ssl = "self";
            this.logger.LogInformation("Install Rancher");
            this.kubernetesHelper.InstallResourceIfNotExists("cattle-system", "namespace", string.Empty);
            this.kubernetesHelper.InstallResourceIfNotExists(
                "tls-rancher-ingress",
                "secret tls",
                "cattle-system",
                $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");

            this.kubernetesHelper.InstallResourceIfNotExists(
                "tls-ca",
                "secret generic",
                "cattle-system",
                $"--from-file=./../ssl/{ssl}/cacerts.pem");

            this.kubernetesHelper.InstallApplicationeIfNotExists(
                "rancher",
                "rancher-latest/rancher",
                "cattle-system",
                $"--set hostname={this.options.Dns} " +
                "--set privateCA=true " +
                "--set 'extraEnv[0].name=CATTLE_CA_CHECKSUM' " +
                "--set 'extraEnv[0].value=' " +
                "--set ingress.tls.source=secret",
                "deploy/rancher");

//                Command.Run("kubectl", "apply -f ./../components/rancher/rancher.ingress.yml");        }

            }

        internal void WaitUntilIsUpAndReady()
        {
            var client = new RestClient($"https://{this.options.Dns}");
            this.logger.LogInformation("Wait until Rancher is up and ready...");
            var pingRequest = new RestRequest("/ping");
            var result = client.Get(pingRequest);

            while (!result.Content.Equals("pong"))
            {
                result = client.Get(pingRequest);
                Thread.Sleep(500);
            }
        }

        internal void Login()
        {
            var client = new RestClient($"https://{this.options.Dns}");
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
            this.token = JsonConvert.DeserializeObject<dynamic>(loginRequestResult.Content).token;
            if (loginRequestResult.StatusCode != HttpStatusCode.Created)
                throw new Exception(loginRequestResult.ErrorMessage);
        }

        internal void ChangePassword()
        {
            var client = new RestClient($"https://{this.options.Dns}");
            client.AddDefaultHeader("Authorization", $"Bearer {this.token}");

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
            {
                throw new Exception(changePasswordResult.ErrorMessage);
            }
        }

        internal void SetServerUrl()
        {
            var client = new RestClient($"https://{this.options.Dns}");
            client.AddDefaultHeader("Authorization", $"Bearer {this.token}");
            this.logger.LogInformation("Set Server-Url...");
            var setServerUrlRequest = new RestRequest("/v3/settings/server-url")
            {
                RequestFormat = DataFormat.Json,
            };

            setServerUrlRequest.AddJsonBody(JObject.FromObject(new
            {
                name = "server-url",
                value = $"{this.options.Dns}"
            }).ToString());

            var setServerUrlResult = client.Put(setServerUrlRequest);
            if (setServerUrlResult.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(setServerUrlResult.ErrorMessage);
            }
        }

        internal void WaitUntilRancherIsActive()
        {
            var client = new RestClient($"https://{this.options.Dns}");
            client.AddDefaultHeader("Authorization", $"Bearer {this.token}");
            this.logger.LogInformation("Wait until Rancher is active...");
            var pingRequest = new RestRequest("/v3/clusters/local");
            var result = client.Get(pingRequest);
            dynamic json = JsonConvert.DeserializeObject(result.Content);
            while (!json.state.Value.Equals("active"))
            {
                this.logger.AppendToLastLog(".");
                Thread.Sleep(1000);
                result = client.Get(pingRequest);
                json = JsonConvert.DeserializeObject(result.Content);
            }
        }

        internal void ExecuteUpdatesForDockerDesktop()
        {
            this.logger.LogInformation("Update some pods to work in Docker Desktop");
            var serviceIpJsonString = this.kubernetesHelper.ExecuteKubectlCommand("get services -n cattle-system -o=json");
            dynamic serviceIpJson = JsonConvert.DeserializeObject(serviceIpJsonString);
            var serviceIp = serviceIpJson.items[0].spec.clusterIP.Value;

            this.kubernetesHelper.ExecuteKubectlCommand($"set env deployments/cattle-cluster-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system");

            this.kubernetesHelper.ExecuteKubectlCommand($"set env daemonset/cattle-node-agent CATTLE_CA_CHECKSUM- CATTLE_SERVER={serviceIp} -n cattle-system)  -n cattle-system");
        }
    }
}