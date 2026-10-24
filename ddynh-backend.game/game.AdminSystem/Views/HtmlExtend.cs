using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FW.Core.AdminSys.WebUI.Themes.Bootstrap;

namespace game.AdminSystem
{
    public static class HtmlExtend
    {
        public static MvcHtmlString IsChoose(this HtmlHelper helper, bool isChoose, string tag = "")
        {
            if (isChoose)
            {
                if (tag == "select")
                {
                    return new MvcHtmlString(" selected=\"selected\"");
                }
                else
                {
                    return new MvcHtmlString(" checked=\"checked\"");
                }
            }
            return null;
        }

        public static MvcHtmlString ShowFormatAmount(this HtmlHelper helper, string money)
        {
            return new MvcHtmlString((Convert.ToInt64(money)/10).ToString("N").Replace(".00", ""));
        }

        public static MvcHtmlString FormatAmount(this HtmlHelper helper,string money)
        {
            return new MvcHtmlString(Convert.ToInt64(money).ToString("N").Replace(".00", ""));           
        }
    }
}