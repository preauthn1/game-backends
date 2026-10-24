using System.Web.Http;
using WebActivatorEx;
using game.GamePlayAPI;
using Swashbuckle.Application;
using game.Infrastructure;
using target.API.AdminSys;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace game.GamePlayAPI
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            if(!SettingHelper.IsProduction)
            {
                GlobalConfiguration.Configuration
                    .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "game.GamePlayAPI");


                        c.OperationFilter<MySwaggerOperationFilter>();

                    })
                    .EnableSwaggerUi(c =>
                    {
                    });
            }

        }
    }
}
