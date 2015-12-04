using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Xunit;
using Owin;

namespace Eowin.AzureServiceBusRelay.Server.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly string _baseAddress = SecretCredentials.ServiceBusAddress;
        
        [Fact]
        public async Task When_GET_response_content_is_received()
        {
            const string contentString = "eureka";
            var client = new HttpClient();
            var rt = client.GetAsync(_baseAddress + "test");
            TestController.OnGet(req => new HttpResponseMessage()
            {
                Content = new StringContent(contentString)
            }
                );
            var response = await rt;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(contentString, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task When_GET_request_headers_are_preserved()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _baseAddress + "Test");
            var acceptHeaders = new MediaTypeWithQualityHeaderValue[]
            {
                new MediaTypeWithQualityHeaderValue("text/plain"),
                new MediaTypeWithQualityHeaderValue("application/xml", 0.13)
            };
            foreach (var h in acceptHeaders) { request.Headers.Accept.Add(h); }
            request.Headers.Add("X-CustomHeader", "value1");
            request.Headers.Add("X-CustomHeader", "value2");
            var rt = client.SendAsync(request);
            TestController.OnGet(req =>
            {
                foreach (var h in acceptHeaders)
                {
                    Assert.True(req.Headers.Accept.Contains(h));
                }
                var customHeader = req.Headers.First(kvp => kvp.Key == "X-CustomHeader");
                Assert.NotNull(customHeader);
                Assert.True(customHeader.Value.Any(s => s.Contains("value1")));
                Assert.True(customHeader.Value.Any(s => s.Contains("value2")));
                return new HttpResponseMessage();
            });
            var response = await rt;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task When_POST_request_headers_are_preserved()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _baseAddress + "Test");
            var acceptHeaders = new MediaTypeWithQualityHeaderValue[]
            {
                new MediaTypeWithQualityHeaderValue("text/plain"),
                new MediaTypeWithQualityHeaderValue("application/xml", 0.13)
            };
            foreach (var h in acceptHeaders) { request.Headers.Accept.Add(h); }
            request.Headers.Add("X-CustomHeader", "value1");
            request.Headers.Add("X-CustomHeader", "value2");
            request.Content = new StringContent("some content");
            request.Content.Headers.ContentLanguage.Add("en-gb");
            request.Content.Headers.ContentLanguage.Add("en-us");
            var rt = client.SendAsync(request);
            TestController.OnPost(req =>
            {
                foreach (var h in acceptHeaders)
                {
                    Assert.True(req.Headers.Accept.Contains(h));
                }
                var customHeader = req.Headers.First(kvp => kvp.Key == "X-CustomHeader");
                Assert.NotNull(customHeader);
                Assert.True(customHeader.Value.Any(s => s.Contains("value1")));
                Assert.True(customHeader.Value.Any(s => s.Contains("value2")));
                Assert.True(req.Content.Headers.ContentLanguage.Contains("en-gb"));
                Assert.True(req.Content.Headers.ContentLanguage.Contains("en-us"));

                return new HttpResponseMessage();
            });
            var response = await rt;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void When_POST_request_content_is_preserved()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _baseAddress + "Test");
            var values = new Dictionary<string, string>()
            {
                {"key1","value1"},
                {"key2","value2"},
                {"key3","value3"},
                {"key4","value4"},

            };
            request.Content = new FormUrlEncodedContent(values);

            var rt = client.SendAsync(request);
            TestController.OnPost(req =>
            {
                var cont = req.Content.ReadAsAsync<JObject>().Result;
                foreach (var p in values)
                {
                    Assert.Equal(p.Value, cont[p.Key].Value<string>());
                }
                return new HttpResponseMessage();
            }
                );
            var response = rt.Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        public class SomeModel
        {
            public int AnInteger { get; set; }
            public string AString { get; set; }
        }

        [Fact]
        public void When_POST_typed_XML_content_can_be_read()
        {
            var reqModel = new SomeModel
            {
                AnInteger = 2,
                AString = "hello"
            };
            var client = new HttpClient();
            client.PostAsXmlAsync(_baseAddress + "Test", reqModel);
            TestController.OnPost(req =>
            {
                var recModel = req.Content.ReadAsAsync<SomeModel>().Result;
                Assert.Equal(reqModel.AnInteger, recModel.AnInteger);
                Assert.Equal(reqModel.AString, recModel.AString);
                return new HttpResponseMessage();
            });
        }

        [Fact]
        public void When_POST_typed_JSON_content_can_be_read()
        {
            var reqModel = new SomeModel
            {
                AnInteger = 2,
                AString = "hello"
            };
            var client = new HttpClient();
            client.PostAsJsonAsync(_baseAddress + "Test", reqModel);
            TestController.OnPost(req =>
            {
                var recModel = req.Content.ReadAsAsync<SomeModel>().Result;
                Assert.Equal(reqModel.AnInteger, recModel.AnInteger);
                Assert.Equal(reqModel.AString, recModel.AString);
                return new HttpResponseMessage();
            });
        }

        [Fact]
        public void When_GET_response_headers_are_preserved()
        {
            const string contentString = "eureka";
            var client = new HttpClient();
            var rt = client.GetAsync(_baseAddress + "test");
            TestController.OnGet(req =>
            {
                var resp = new HttpResponseMessage()
                {
                    Content = new StringContent(contentString)
                };

                resp.Content.Headers.ContentLanguage.Add("pt");
                resp.Content.Headers.ContentLanguage.Add("gr");

                resp.Headers.Add("X-CustomHeader", "value1");
                resp.Headers.Add("X-CustomHeader", "value2");
                resp.Headers.ETag = new EntityTagHeaderValue("\"12345678\"");

                return resp;
            });
            var response = rt.Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(response.Content.Headers.ContentLanguage.Contains("pt"));
            Assert.True(response.Content.Headers.ContentLanguage.Contains("gr"));

            Assert.NotNull(response.Headers.ETag);
            var customHeader = response.Headers.First(kvp => kvp.Key == "X-CustomHeader");
            Assert.NotNull(customHeader);
            Assert.True(customHeader.Value.Contains("value1,value2"));
        }

        [Fact]
        public void When_POST_request_and_response_content_is_preserved()
        {
            const string contentString = "eureka";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _baseAddress + "test")
            {
                Content = new StringContent(contentString)
            };
            var rt = client.SendAsync(request);
            TestController.OnPost(req =>
            {
                var s = req.Content.ReadAsStringAsync().Result;
                Assert.Equal(contentString, s);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(s.ToUpper())
                };
            });
            var response = rt.Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(contentString.ToUpper(), response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Can_handle_responses_with_no_content()
        {
            var client = new HttpClient();
            var t = client.GetAsync(_baseAddress + "test");
            TestController.OnGet(req => new HttpResponseMessage(HttpStatusCode.NoContent));
            var resp = t.Result;
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        [Fact]
        public void Handles_querystring_correctly()
        {
            var client = new HttpClient();
            var t = client.GetAsync(_baseAddress + "test?a=1&b=xyz");
            TestController.OnGet(req =>
            {
                var qs = req.RequestUri.Query;
                Assert.Equal("?a=1&b=xyz", qs);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });            
            var resp = t.Result;
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        private readonly AzureServiceBusOwinServer _server;

        public IntegrationTests()
        {
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
        }

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}