using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;

namespace HangfireJob.Services
{
    public sealed class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HangFireAuthorizationFilter(IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool Authorize([NotNull] DashboardContext context)
        {
            try
            {
                var httpContext = context.GetHttpContext();
                var user = new Byte[20];
                httpContext.Session.TryGetValue("User", out user);
                if (String.IsNullOrEmpty(System.Text.Encoding.UTF8.GetString(user)))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
