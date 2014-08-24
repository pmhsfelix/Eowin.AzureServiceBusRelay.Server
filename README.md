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


## Build and tests

To run the tests located in `Eowin.AzureServiceBusRelay.Server.Tests`, the following environment variables must be defined

* `ServiceBusAddress` - containing the relay listening address
* `ServiceBusSecret` - containing the owner's secret

