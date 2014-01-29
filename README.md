Eowin.AzureServiceBusRelay.Server
=================================

A OWIN server for the Azure Service Bus Relay

## Introduction

This project aims to provide a OWIN server based on the Azure Service Bus relaying functionality.

Usage example:

```csharp
    var sbConfig = new AzureServiceBusOwinServiceConfiguration(
        issuerName: "owner",
        issuerSecret: SecretCredentials.Secret,
        address: SecretCredentials.ServiceBusAddress);
    _server = AzureServiceBusOwinServer.Create(sbConfig, app =>
    {
        var config = new HttpConfiguration();
        config.Routes.MapHttpRoute("ApiDefault", "webapi/{controller}/{id}", new { id = RouteParameter.Optional });

        app.Use((ctx, next) =>
        {
            Trace.TraceInformation(ctx.Request.Uri.ToString());
            return next();
        });
        app.UseWebApi(config);
    });
```


## Build

1. The `Eowin.AzureServiceBusRelay.Server.Tests` project is missing a `SecretCredentials.cs` file.
This file contains the Service Bus credentials, with the following structure, and is never committed into the repository.

```csharp
namespace Eowin.AzureServiceBusRelay.Server.Tests
{
    public class ServiceBusCredentials
    {
        public static readonly string ServiceBusAddress = "https://???????.servicebus.windows.net/webapi/";
        public static readonly string Secret = "????";
    }
}
``` 

Please create this files locally, using an adequate service bus address and secret.

