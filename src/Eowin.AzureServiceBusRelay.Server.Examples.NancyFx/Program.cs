using System;
using Eowin.AzureServiceBusRelay.Server.Tests;
using Nancy;
using Owin;

namespace Eowin.AzureServiceBusRelay.Server.Examples.NancyFx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string address = "https://webapibook.servicebus.windows.net/nancy/";
            var sbConfig = new AzureServiceBusOwinServiceConfiguration(
                issuerName: "owner",
                issuerSecret: SecretCredentials.Secret,
                address: address);
            var server = AzureServiceBusOwinServer.Create(sbConfig, app =>
            {
                app.UseNancy();
            });
            Console.WriteLine("Server is listening at {0}", address);
            Console.ReadKey();
        }
    }

    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/nancy"] = x => "hello from Nancy in the cloud";
        }
    }
}
