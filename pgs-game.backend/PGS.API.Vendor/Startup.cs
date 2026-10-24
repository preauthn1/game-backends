using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using target.API.AdminSys;
using target.BL.Services;
using target.Models;
using target.Utility;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using target.Web.Modules.Filter;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using example.invalid;
using System.Reflection;

namespace target.API.Vendor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }


        private List<string> SwaggerSpecialDocumentGroups = SystemConstant.SwaggerSpecialDocumentGroups;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                   .AddNewtonsoftJson(options =>
                   {
                       //如果沒有呼叫newtonsoft版的，int欄位一定要用數字型別 不得使用string傳入
                       options.SerializerSettings.DateFormatString = DateTimeHelper.ISO_8601_Format;
                   });

            //自訂的httpContext
            services.AddMyHttpContextAccessor();
            services.AddSwaggerGen(c =>
            {
                c.SchemaFilter<PGSSwaggerTemporarySkipOutputColumnsFilter>();
                c.ParameterFilter<PGSSwaggerQueryParameterChangeParameterNameFilter>();
                c.DocumentFilter<PGSSwaggerQueryParameterIgnoreFilter>();
                c.OperationFilter<PGSOperationFilter>();

                c.SwaggerDoc("tfMode", new OpenApiInfo { Title = SettingHelper.WebSiteName, Version = "tfMode" });
                c.SwaggerDoc("swMode", new OpenApiInfo { Title = SettingHelper.WebSiteName + "(Seamless)", Version = "swMode" });

                foreach (var groupName in SwaggerSpecialDocumentGroups)
                {
                    c.SwaggerDoc(groupName, new OpenApiInfo 
                    { 
                        Title = SettingHelper.WebSiteName + $" ({groupName})", 
                        Version = groupName
                    });
                }

                LoadXmlDocument(c, "target.API.Vendor");
            });

            //自定的錯誤處理、
            services.AddMvc(config =>
            {
                config.Filters.Add(new ApiResponseFormatterFilter());
                config.Filters.Add(new GlobalApiHeaderTemporaryInfoFilter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    return ModalValidCustomizedResponse.ResponseCustomizedErrorResponse(context);
                };
            });

            //註冊幾種HttpClient
            RuntimeHelper.RegisterHttpClient(services);

            //呼叫自訂的autofac初始化
            InitializeIOC();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //需要放前面一點 避免被其他middleWare覆蓋
            app.UseStaticFiles();

            //swagger相關
            app.UseSwagger();

            if (!SettingHelper.IsProduction)
            {               
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/tfMode/swagger.json", "target.API.TransferMode");
                    c.SwaggerEndpoint("/swagger/swMode/swagger.json", "target.API.SeamlessMode");

                    foreach(var groupName in SwaggerSpecialDocumentGroups)
                    {
                        c.SwaggerEndpoint($"/swagger/{groupName}/swagger.json", groupName);
                    }

                    c.InjectStylesheet("/css/swagger_customized.css");
                });
            }

            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                return next();
            });

            //自訂的extension (middleWare、或擴充)
            app.UseMiddleware<GlobalRequestLogMiddleware>();
            app.UseMiddleware<HttpStatusCodeHandleMiddleware>();
            app.UseGlobalExceptionHandler();  //exception攔截必須在比較後面
            app.UseStaticHttpContext();


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //全域暫存Service註冊器
            RuntimeHelper.SetGlobalServiceProvider(app.ApplicationServices);

            ApiConfigDefineHelper.AnalyzeAllApiConfigDefines(Assembly.GetExecutingAssembly());
            WarmUp();
        }

        private void LoadXmlDocument(SwaggerGenOptions c, string fileName)
        {
            try
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, $"AppData/{fileName}.xml");
                c.IncludeXmlComments(filePath);
            }
            catch
            {

            }
        }


        private void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "target.BL"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
        }


        /// <summary>
        /// 暖機
        /// </summary>
        private void WarmUp()
        {
            var service = new WarmUpService();
            _ = service.WarmUp();
        }



    }
}
