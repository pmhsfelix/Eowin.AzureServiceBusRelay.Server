using System;

namespace Eowin.AzureServiceBusRelay.Server.Tests
{
    public class SecretCredentials
    {
        public static string ServiceBusAddress
        {
            get
            {
                var addr = Environment.GetEnvironmentVariable("ServiceBusAddress");
                if(string.IsNullOrWhiteSpace(addr)) throw new InvalidOperationException("ServiceBusAddress is not defined");
                return addr;
            }
        }

        public static string Secret
        {
            get
            {
                var secret = Environment.GetEnvironmentVariable("ServiceBusSecret");
                if (string.IsNullOrWhiteSpace(secret)) throw new InvalidOperationException("ServiceBusSecret is not defined");
                return secret;
            }
        }
    }
}