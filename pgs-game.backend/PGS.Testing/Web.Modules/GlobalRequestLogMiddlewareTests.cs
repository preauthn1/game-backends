using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.Models.Table;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using target.Web.Modules.Filter;
using System;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace target.Testing
{
    /// <summary>
    /// 這段是 AI幫寫的
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class GlobalRequestLogMiddlewareTests : BaseTest
    {
        private IContainer _originalContainer;
        private IContainer _testContainer;
        private IApiRequestLogService _apiRequestLogService;

        [TestInitialize]
        public void Initialize()
        {
            // 用 mock log service 隔離 middleware 寫 log 行為，測試不應連線或寫入真實資料庫。
            _originalContainer = IOCHelper.Container;
            _apiRequestLogService = Substitute.For<IApiRequestLogService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(_apiRequestLogService).As<IApiRequestLogService>();
            _testContainer = containerBuilder.Build();
            IOCHelper.Container = _testContainer;
        }


        [TestCleanup]
        public void Cleanup()
        {
            // 還原全域 container，避免影響後續測試。
            IOCHelper.Container = _originalContainer;
            _testContainer?.Dispose();
        }


        [TestMethod]
        public async Task GlobalRequestLogMiddleware_SuccessResponse_LogsBodiesAndReturnsResponseToClient()
        {
            // 情境：一般 POST API。middleware 緩衝 response 供 log 使用後，
            // 下游仍必須讀得到 request body，且 client 必須收到完整 response。
            const string requestText = "{\"PlayerID\":123}";
            const string responseText = "{\"result\":\"ok\"}";
            var context = CreateHttpContext("POST", "/api/middleware-test", requestText);
            ApiRequestLogEntity writtenLog = null;

            _apiRequestLogService
                .When(service => service.CreateTryCatch(Arg.Any<string>(), Arg.Any<ApiRequestLogEntity>()))
                .Do(callInfo => writtenLog = callInfo.Arg<ApiRequestLogEntity>());

            RequestDelegate next = async httpContext =>
            {
                httpContext.Request.Body.Position = 0;
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                string bodyReceivedByNext = await reader.ReadToEndAsync();
                Assert.AreEqual(requestText, bodyReceivedByNext);

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                await httpContext.Response.WriteAsync(responseText);
            };

            await new GlobalRequestLogMiddleware(next).Invoke(context);

            Assert.AreSame(context.Items["OriginalResponseBody"], context.Response.Body);
            Assert.AreEqual(responseText, await ReadResponseTextAsync(context.Response.Body));
            Assert.IsNotNull(writtenLog);
            Assert.AreEqual(requestText, writtenLog.RequestBody);
            Assert.AreEqual(responseText, writtenLog.ResponseText);
            Assert.AreEqual(StatusCodes.Status200OK, writtenLog.ResponseStatusCode);
        }

        [TestMethod]
        public async Task GlobalRequestLogMiddleware_HeadRequest_DoesNotReplaceResponseStreamOrWriteLog()
        {
            // 情境：HEAD 屬於不需要記錄的 HTTP method；應直接往下執行，
            // 不建立 response buffer，也不呼叫 log service。
            var context = CreateHttpContext("HEAD", "/api/middleware-test", string.Empty);
            var originalResponseBody = context.Response.Body;

            RequestDelegate next = async httpContext =>
            {
                Assert.AreSame(originalResponseBody, httpContext.Response.Body);
                await httpContext.Response.WriteAsync("head-response");
            };

            await new GlobalRequestLogMiddleware(next).Invoke(context);

            Assert.AreEqual("head-response", await ReadResponseTextAsync(context.Response.Body));
            _apiRequestLogService.DidNotReceiveWithAnyArgs().CreateTryCatch(default, default);
            _apiRequestLogService.DidNotReceiveWithAnyArgs().WriteToQueueBuffering(default);
        }


        [TestMethod]
        public async Task GlobalRequestLogMiddleware_WhenNextThrows_RestoresOriginalResponseStream()
        {
            // 情境：後續 middleware/controller 發生未處理例外；
            // finally 必須還原原始 response stream，讓外層 exception handler 還能寫入錯誤 response。
            var context = CreateHttpContext("POST", "/api/middleware-test", "{}");
            var originalResponseBody = context.Response.Body;
            RequestDelegate next = _ => throw new InvalidOperationException("test exception");

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => new GlobalRequestLogMiddleware(next).Invoke(context));

            Assert.AreSame(originalResponseBody, context.Response.Body);
        }

        private static DefaultHttpContext CreateHttpContext(string method, string path, string requestText)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestText));
            // 模擬 Startup 內的 EnableBuffering，確保 body 被讀取後能回到開頭再供後續流程讀取。
            context.Request.EnableBuffering();

            var originalResponseBody = new MemoryStream();
            context.Response.Body = originalResponseBody;
            context.Items["OriginalResponseBody"] = originalResponseBody;

            MyHttpContext.Configure(new HttpContextAccessor { HttpContext = context });
            return context;
        }

        private static async Task<string> ReadResponseTextAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
    }
}
