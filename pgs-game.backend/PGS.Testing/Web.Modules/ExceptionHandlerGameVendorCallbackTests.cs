using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.Models;
using target.Models.Models;
using target.Models.Table;
using target.Web.Modules.Filter;
using System;
using System.Diagnostics;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace target.Testing
{
    [TestClass]
    [DoNotParallelize]
    [AIGenerate]
    public class ExceptionHandlerGameVendorCallbackTests : BaseTest
    {
        [TestMethod]
        public async Task TerproException_ReturnsPgsErrorResponseAndWritesMockLog_Test()
        {
            var exception = new TerproException(ApiErrorCode.NotFoundData);
            var logService = Substitute.For<IApiRequestLogService>();

            var context = await ExecuteMiddleware(exception, logService);
            string responseText = await ReadResponseText(context.Response);

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.AreEqual("application/json", context.Response.ContentType);
            StringAssert.Contains(
                responseText,
                $"\"code\":{(int)ApiErrorCode.NotFoundData}");

            logService.Received(1).CreateErrorLogTryCatch(
                Arg.Any<string>(),
                Arg.Is<ApiRequestLogEntity>(log =>
                    log.ResponseStatusCode == StatusCodes.Status400BadRequest &&
                    log.ResponseText == responseText));
        }

        [TestMethod]
        public async Task BasicAuthorizeError_DoesNotWriteStackTrace_Test()
        {
            var exception = new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);
            var logService = Substitute.For<IApiRequestLogService>();
            ApiRequestLogEntity capturedLog = null;

            logService
                .When(service => service.CreateErrorLogTryCatch(
                    Arg.Any<string>(),
                    Arg.Any<ApiRequestLogEntity>()))
                .Do(call => capturedLog = call.ArgAt<ApiRequestLogEntity>(1));

            var context = await ExecuteMiddleware(exception, logService);
            string responseText = await ReadResponseText(context.Response);

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            StringAssert.Contains(
                responseText,
                $"\"code\":{(int)ApiErrorCode.Authorize_AccessToken_NotValid}");
            Assert.IsNotNull(capturedLog);
            Assert.AreEqual(string.Empty, capturedLog.StackTrace);
        }

        [TestMethod]
        public async Task NativeException_ReturnsInternalServerErrorWithoutExternalNotify_Test()
        {
            var exception = new InvalidOperationException("Unexpected end of request content.");
            var logService = Substitute.For<IApiRequestLogService>();

            var context = await ExecuteMiddleware(exception, logService);
            string responseText = await ReadResponseText(context.Response);

            Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            StringAssert.Contains(
                responseText,
                $"\"code\":{(int)ApiErrorCode.InternalServerError}");

            logService.Received(1).CreateErrorLogTryCatch(
                Arg.Any<string>(),
                Arg.Any<ApiRequestLogEntity>());
        }

        private static async Task<DefaultHttpContext> ExecuteMiddleware(
            Exception exception,
            IApiRequestLogService logService)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMetrics();
            services.AddOptions();
            services.AddSingleton(new DiagnosticListener("ExceptionHandlerGameVendorCallbackTests"));
            services.AddSingleton<DiagnosticSource>(serviceProvider =>
                serviceProvider.GetRequiredService<DiagnosticListener>());

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                var applicationBuilder = new ApplicationBuilder(serviceProvider);
                applicationBuilder.UseGameVendorCallbackExceptionHandler(() => logService);
                applicationBuilder.Run(_ => Task.FromException(exception));

                var context = new DefaultHttpContext
                {
                    RequestServices = serviceProvider
                };
                context.Request.Method = HttpMethods.Post;
                context.Request.Path = "/api/external-vendor/mock-callback";
                context.Request.Scheme = "https";
                context.Request.Host = new HostString("localhost");
                context.Response.Body = new MemoryStream();

                RequestDelegate pipeline = applicationBuilder.Build();
                await pipeline(context);

                return context;
            }
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
