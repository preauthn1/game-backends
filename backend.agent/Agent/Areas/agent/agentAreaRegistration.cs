using System.Web.Mvc;

namespace Agent.Areas.agent
{
    public class agentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "agent";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "agent_default",
                "agent/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}