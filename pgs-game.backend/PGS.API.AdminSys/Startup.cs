using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using target.Web.Modules.Filter;
using Swashbuckle.AspNetCore.SwaggerGen;
using target.Utility.Extensions;
using example.invalid;
using Microsoft.AspNetCore.Routing;
using target.Utility.Extensions.Web;
using Microsoft.AspNetCore.Http;

namespace target.API.AdminSys
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson(options =>
                    {
                        //如果沒有呼叫newtonsoft版的，int欄位一定要用數字型別 不得使用string傳入
                        options.SerializerSettings.DateFormatString = DateTimeHelper.ISO_8601_Format;

                        //如果本地斷點測試，遇到時區被轉成+8 本地時間了。可能是你傳入的time時間格式是 ISO 8601 ? 例如: 2024-01-01T00:00:00Z 請想辦法改成傳入 2024-01-01 00:00:00 ...為何這樣我也無解
                        //註:下面這個方式無用處
                        //options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                    });
                        
            if (SettingHelper.IsDev)
            {
                services.AddControllersWithViews();
                services.AddRazorPages().AddRazorRuntimeCompilation();
            }

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .WithExposedHeaders("X-target-File-Name");
                });
            });

            //自訂的httpContext
            services.AddMyHttpContextAccessor();
            services.AddSwaggerGen(c =>
            {
                c.SchemaFilter<PGSSwaggerTemporarySkipOutputColumnsFilter>();
                c.ParameterFilter<PGSSwaggerQueryParameterChangeParameterNameFilter>();
                c.DocumentFilter<PGSSwaggerQueryParameterIgnoreFilter>();
                c.OperationFilter<PGSOperationFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = SettingHelper.WebSiteName, Version = "v1" });
                LoadXmlDocument(c, "target.API.AdminSys");
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
            return;
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {             
            //需要放前面一點 避免被其他middleWare覆蓋
            app.UseStaticFiles();

            //swagger相關
            //app.UseSwagger();

            if (!SettingHelper.IsProduction)
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", SettingHelper.WebSiteName);
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
            app.UseCors();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //全域暫存Service註冊器
            RuntimeHelper.SetGlobalServiceProvider(app.ApplicationServices);

            ApiConfigDefineHelper.AnalyzeAllApiConfigDefines(Assembly.GetExecutingAssembly() , isGenerateTypeScriptText: true);
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
    }


}
