using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using Microsoft.ServiceBus;

namespace Eowin.AzureServiceBusRelay.Server
{
    internal class AzureServiceBusOwinServerFactory
    {
        private readonly AzureServiceBusOwinServiceConfiguration _config;
        
        public AzureServiceBusOwinServerFactory(AzureServiceBusOwinServiceConfiguration config)
        {
            _config = config;
        }

        public IDisposable Create(Func<IDictionary<string, object>, Task> app, IDictionary<string, object> properties)
        {
            var host = new WebServiceHost(new DispatcherService(app));
            var ep = host.AddServiceEndpoint(typeof(DispatcherService), GetBinding(), _config.Address);
            ep.Behaviors.Add(_config.GetTransportBehavior());
            host.Open();
            return host;
        }

        private Binding GetBinding()
        {
            var b = new WebHttpRelayBinding(EndToEndWebHttpSecurityMode.None, RelayClientAuthenticationType.None);
            var elems = b.CreateBindingElements();
            var ee = elems.Find<WebMessageEncodingBindingElement>();
            ee.ContentTypeMapper = new RawContentTypeMapper();
            return new CustomBinding(elems);
        }

        internal class RawContentTypeMapper : WebContentTypeMapper
        {
            public override WebContentFormat GetMessageFormatForContentType(string contentType)
            {
                return WebContentFormat.Raw;
            }
        }
    }
}