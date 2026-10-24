using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using target.Models;
using target.Models.Models;
using target.Utility.Helper.HttpContext;
using target.Web.Modules.Filter;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace target.Testing
{
    [TestClass]
    [DoNotParallelize]
    [AIGenerate]
    public class HttpStatusCodeHandleMiddlewareTests : BaseTest
    {
        [TestMethod]
        public async Task Status404_ReturnsNotFoundApiErrorAndMarksErrorLog_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status404NotFound,
                "/api/not-exist",
                HttpMethods.Get);

            string responseText = await ReadResponseText(context.Response);
            JObject response = JObject.Parse(responseText);

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual("application/json", context.Response.ContentType);
            Assert.AreEqual((int)ApiErrorCode.NotFoundPageOrAPiUrl, (int)response["code"]);
            Assert.AreEqual(
                "1",
                context.Request.Headers[SystemConstant.HeaderKey_MustWriteErrorLog].ToString());
        }

        [TestMethod]
        public async Task Status404_LongPath_ReturnsPathLengthInvalidError_Test()
        {
            string longPath = "/api/" + new string('x', 60);

            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status404NotFound,
                longPath,
                HttpMethods.Get);

            JObject response = JObject.Parse(await ReadResponseText(context.Response));

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual(
                (int)ApiErrorCode.NotFoundPageOrAPiUrlPathLengthInvalid,
                (int)response["code"]);
        }

        [TestMethod]
        public async Task Status405_ReturnsMethodNotAllowedError_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status405MethodNotAllowed,
                "/api/mock",
                HttpMethods.Post);

            JObject response = JObject.Parse(await ReadResponseText(context.Response));

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual((int)ApiErrorCode.MethodNotAllow, (int)response["code"]);
        }

        [TestMethod]
        public async Task Status406_ReturnsHttpStatusCodeErrorWithOriginalStatus_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status406NotAcceptable,
                "/api/mock",
                HttpMethods.Get);

            string responseText = await ReadResponseText(context.Response);
            JObject response = JObject.Parse(responseText);

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual((int)ApiErrorCode.HttpStatusCodeError, (int)response["code"]);
            StringAssert.Contains(responseText, "StatusCode:406");
        }

        [TestMethod]
        public async Task Status404_EfgCallback_IsNotConverted_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status404NotFound,
                "/api/EFG/callback",
                HttpMethods.Post);

            Assert.AreEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(context.Response.ContentType));
            Assert.AreEqual(string.Empty, await ReadResponseText(context.Response));
        }

        [TestMethod]
        public async Task Status404_HeadRequest_DoesNotMarkErrorLog_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status404NotFound,
                "/favicon.ico",
                HttpMethods.Head);

            JObject response = JObject.Parse(await ReadResponseText(context.Response));

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual((int)ApiErrorCode.NotFoundPageOrAPiUrl, (int)response["code"]);
            Assert.IsFalse(
                context.Request.Headers.ContainsKey(SystemConstant.HeaderKey_MustWriteErrorLog));
        }

        [TestMethod]
        public async Task Status200_PreservesDownstreamResponse_Test()
        {
            DefaultHttpContext context = await ExecuteMiddleware(
                StatusCodes.Status200OK,
                "/api/health",
                HttpMethods.Get,
                "ok");

            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.AreEqual("ok", await ReadResponseText(context.Response));
        }

        private static async Task<DefaultHttpContext> ExecuteMiddleware(
            int downstreamStatusCode,
            string path,
            string method,
            string downstreamResponseText = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = path;
            context.Request.QueryString = new QueryString("?source=mstest");
            context.Response.Body = new MemoryStream();

            MyHttpContext.Configure(new HttpContextAccessor
            {
                HttpContext = context
            });

            RequestDelegate next = async httpContext =>
            {
                httpContext.Response.StatusCode = downstreamStatusCode;

                if (!string.IsNullOrEmpty(downstreamResponseText))
                {
                    await httpContext.Response.WriteAsync(downstreamResponseText);
                }
            };

            var middleware = new HttpStatusCodeHandleMiddleware(next);
            await middleware.Invoke(context);

            return context;
        }

        private static async Task<string> ReadResponseText(HttpResponse response)
        {
            response.Body.Position = 0;

            using (var reader = new StreamReader(
                response.Body,
                Encoding.UTF8,
                false,
                1024,
                true))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
