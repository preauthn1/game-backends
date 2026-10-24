using System.Web.Mvc;

namespace Agent.Areas.agenttotal
{
    public class agenttotalAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "agenttotal";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "agenttotal_default",
                "agenttotal/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}