using System.Web.Mvc;

namespace Agent.Areas.agentclub
{
    public class agentclubAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "agentclub";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "agentclub_default",
                "agentclub/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}