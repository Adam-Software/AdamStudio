using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AdamController.WebApi.Client.v1
{
    internal class ApiClient
    {
        #region Const

        private const string cDefaultApiPath = "api/";

        #endregion

        #region Var

        private readonly HttpClient mClient;
        private readonly MediaTypeWithQualityHeaderValue mMediaTypeHeader = new("application/json");

        #endregion

        #region ~

        internal ApiClient(Uri uri, string login, string password)
        {
            // .NET 9+ SocketsHttpHandler performs HTTP version detection and may
            // attempt HTTP/2 negotiation that some legacy servers (Python Flask,
            // aiohttp, old ASP.NET) cannot handle — they reset the connection.
            // Pin the client to HTTP/1.1 explicitly to match the Adam-Servers
            // WebApi contract.
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30)
            };

            mClient = new HttpClient(handler, disposeHandler: true)
            {
                DefaultRequestVersion = HttpVersion.Version11,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                Timeout = TimeSpan.FromSeconds(15)
            };

            mClient.DefaultRequestHeaders.Accept.Clear();
            mClient.DefaultRequestHeaders.Add("X-Version", "1");
            mClient.DefaultRequestHeaders.Accept.Add(mMediaTypeHeader);

            AuthenticationHeaderValue defaultAutentificationHeader = new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}")));

            mClient.BaseAddress = uri;
            mClient.DefaultRequestHeaders.Authorization = defaultAutentificationHeader;
        }

        #endregion

        #region Methods

        internal Task<HttpResponseMessage> Put(string path)
        {
            var responseMessage = mClient.PutAsync($"{cDefaultApiPath}{path}", null);
            return responseMessage;
        }

        internal Task<HttpResponseMessage> Get(string path)
        {
            var responseMessage = mClient.GetAsync($"{cDefaultApiPath}{path}");
            return responseMessage;
        }

        internal Task<HttpResponseMessage> Post(string path, RequestModel.PythonCommandModel command)
        {
            var result = mClient.PostAsJsonAsync($"{cDefaultApiPath}{path}", command);
            return result;
        }

        internal void Dispose()
        {
            mClient.Dispose();
        }

        #endregion
    }
}
