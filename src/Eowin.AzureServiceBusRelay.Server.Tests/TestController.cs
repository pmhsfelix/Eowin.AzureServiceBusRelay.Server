using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Eowin.AzureServiceBusRelay.Server.Tests
{
    public class TestController : ApiController
    {
        static readonly RequestExecutor<HttpRequestMessage, HttpResponseMessage> _getExecutor = new RequestExecutor<HttpRequestMessage, HttpResponseMessage>();
        static readonly RequestExecutor<HttpRequestMessage, HttpResponseMessage> _postExecutor = new RequestExecutor<HttpRequestMessage, HttpResponseMessage>();

        public static void OnGet(Func<HttpRequestMessage, HttpResponseMessage> func)
        {
            _getExecutor.RunSync(func);
        }

        public static void OnPost(Func<HttpRequestMessage, HttpResponseMessage> func)
        {
            _postExecutor.RunSync(func);
        }

        public Task<HttpResponseMessage> Get()
        {
            return _getExecutor.Post(Request);
        }

        public Task<HttpResponseMessage> Post()
        {
            return _postExecutor.Post(Request);
        }
    }
}