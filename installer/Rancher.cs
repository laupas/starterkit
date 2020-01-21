using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace installer
{
    internal static class Rancher
    {
        public static void SetServerUrl(RestClient client, string rancherUrl)
        {
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

            client.AddDefaultHeader("Authorization", $"Bearer {token}");

            var changePasswordResult = client.Post(changePasswordRequest);
            if (changePasswordResult.StatusCode != HttpStatusCode.OK)
                throw new Exception(changePasswordResult.ErrorMessage);

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
    }
}