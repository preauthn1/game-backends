using Hangfire;
using HangfireJob.EFModels;
using HangfireJob.Models;
using HangfireJob.Models.Gash;
using HangfireJob.Services;
using HangfireJob.Services.Gash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace HangfireJob
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IConfiguration _config { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddControllersWithViews();
            services.AddScoped<ISettleService, SettleService>();
            services.AddDbContext<EFCoreContext>(options =>
                 options.UseSqlServer(_config.GetConnectionString("CenterDB"))
            );
            services.AddOptions();
            services.Configure<GashSettleSettings>(_config.GetSection("GashSettleSettings"));
            services.Configure<LDAP>(_config.GetSection("LDAP"));
            services.AddHangfire(x => x.UseSqlServerStorage(_config.GetConnectionString("Hangfire")));
        }

        public void Configure(IApplicationBuilder app,
                            IWebHostEnvironment env,
                            EFCoreContext dbContext,
                            IOptions<GashSettleSettings> gashSettleSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            var hangFireAuth = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangFireAuthorizationFilter(app.ApplicationServices.GetService<IAuthorizationService>(),
                    app.ApplicationServices.GetService<IHttpContextAccessor>())
                },
                AppPath = "/"
            };
            app.UseHangfireDashboard("/hangfire", options: hangFireAuth);
            app.UseHangfireServer();
            var jobId = BackgroundJob.Enqueue(() => StartDoWork());
            RecurringJob.AddOrUpdate<ISettleService>((s) => s.DoWorkAsync(gashSettleSettings.Value.Url), Cron.Daily);

        }

        public void StartDoWork()
        {
            Console.WriteLine("=============================");
            Console.WriteLine("Register Job for re-settle!");
            Console.WriteLine("=============================");
        }
    }
}