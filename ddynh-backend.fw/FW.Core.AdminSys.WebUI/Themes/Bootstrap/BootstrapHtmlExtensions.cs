using FW.Core.AdminSys.Provider;
using FW.Core.Auth;
using FW.Core.Auth.Token;
using FW.Core.Engine;
using FW.Core.Helper;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace FW.Core.AdminSys.WebUI.Themes.Bootstrap
{
    /// <summary>
    /// Bootstrap UI扩展
    /// </summary>
    public static class BootstrapHtmlExtensions
    {
        /// <summary>
        /// 获取权限标识
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetRightToken(string area, string controller, string action, string method)
        {
            return string.Format("{0}_{1}_{2}_{3}", area,controller, action, method);
        }

        /// <summary>
        /// 检查权限标识
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckRightToken(string token)
        {
            AuthManager authManager = EngineContext.Current.Resolve<AuthManager>();
            IAdminSysProvider provider = EngineContext.Current.Resolve<IAdminSysProvider>();

            if (provider.CheckUserRight(Convert.ToInt32(authManager.CurrentUser.UserID), token))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据地址解析路由信息
        /// </summary>
        /// <param name="href"></param>
        /// <returns></returns>
        public static Pair<string, string, string> GetRouteTokenFromUrl(string href)
        {
            if (String.IsNullOrWhiteSpace(href))
            {
                throw new ArgumentNullException("href");
            }

            string area = "";
            string controller = "";
            string action = "";

            string urlFormat = "http://{0}{1}";
            if (!href.StartsWith("/"))
            {
                urlFormat = "http://{0}/{1}";
            }

            string routeHref = String.Format(urlFormat, HttpContext.Current.Request.Url.Authority, href);
            HttpRequest request = new HttpRequest("", routeHref, null);
            HttpResponse response = new HttpResponse(new StringWriter());
            HttpContext context = new HttpContext(request, response);
            HttpContextBase contextWrapper = new HttpContextWrapper(context);

            RouteData routeData = RouteTable.Routes.GetRouteData(contextWrapper);

            if (routeData.Values.ContainsKey("area"))
            {
                area = routeData.Values["area"].ToString();
            }
            if (routeData.Values.ContainsKey("controller"))
            {
                controller = routeData.Values["controller"].ToString();
            }
            if (routeData.Values.ContainsKey("action"))
            {
                action = routeData.Values["action"].ToString();
            }

            return new Pair<string, string, string>() { Item1 = area, Item2 = controller, Item3 = action };
        }

        #region RawWithRight
        public static MvcHtmlString RawWithRight(this HtmlHelper html, string data, string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return new MvcHtmlString("");
            }

            if(CheckRightToken(token))
            {
                return new MvcHtmlString(data);
            }

            return new MvcHtmlString("");
        }

        public static MvcHtmlString RawWithRight(this HtmlHelper html, string data, string area, string controller, string action, string method)
        {
            string token = [REDACTED], controller, action, method);

            return html.RawWithRight(data,token);
        }
        #endregion

        #region Right Button
        public static MvcHtmlString ButtonWithRight(this HtmlHelper html, string id = "", string value = "", string area = "", string controller = "", string action = "", string method = "", string cssName = "", string iconCls = "", string href = "", string style="", string onclick = "", string type = "button")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id =  Guid.NewGuid().ToString().Replace("-", "").Substring(1, 6);
                id = String.Format("btn_{0}", id);
            }

            if (!String.IsNullOrWhiteSpace(href) && href != "#" && !String.IsNullOrWhiteSpace(onclick))
            {
                onclick = String.Format("window.location.href='{0}'", href);
            }

            if (string.IsNullOrWhiteSpace(area) && string.IsNullOrWhiteSpace(controller) && string.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("area"))
                {
                    area = html.ViewContext.RouteData.Values["area"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("controller"))
                {
                    controller = html.ViewContext.RouteData.Values["controller"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("action"))
                {
                    action = html.ViewContext.RouteData.Values["action"].ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                method = "Get";
            }

            string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);
            var r = EngineContext.Current.Resolve<IAdminSysProvider>().GetRightByToken(token);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (r != null)
                {
                    value = r.RightName;
                }
            }
            if (String.IsNullOrWhiteSpace(iconCls) && r != null)
            {
                iconCls = r.IconCls;
            }

            string iconHtml = String.Empty;
            if (!String.IsNullOrWhiteSpace(iconCls))
            {
                iconHtml = String.Format("<i class=\"fa {0}\"></i> ", iconCls);
            }

            string data = String.Format("<button id=\"{0}\" type=\"{6}\" class=\"btn {3}\"  style=\"{4}\" onclick=\"{5}\" >{1}{2}</button>", id, iconHtml, value, cssName, style, onclick, type);
            return html.RawWithRight(data,token);
        }

        public static MvcHtmlString ButtonWithRightAction(this HtmlHelper html, string action, string id = "", string value = "", string area = "", string controller = "", string method = "", string cssName = "", string iconCls = "", string style="", string onclick = "", string type = "button")
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException("action");
            }

            if (string.IsNullOrWhiteSpace(area) && string.IsNullOrWhiteSpace(controller) && string.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("area"))
                {
                    area = html.ViewContext.RouteData.Values["area"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("controller"))
                {
                    controller = html.ViewContext.RouteData.Values["controller"].ToString();
                }
            }

            string href = new UrlHelper(html.ViewContext.RequestContext).Action(action, controller, new { area = area});

            return html.ButtonWithRight(id, value, area, controller, action, method, cssName, iconCls, href, style, onclick, style);
        }

        public static MvcHtmlString ButtonWithRightHref(this HtmlHelper html, string href, string id = "", string value = "", string method = "", string cssName = "", string iconCls = "", string style="", string onclick = "", string type = "button")
        {
            var rdata = GetRouteTokenFromUrl(href);
            string area = rdata.Item1;
            string controller = rdata.Item2;
            string action = rdata.Item3;

            return html.ButtonWithRight(id, value, area, controller, action, method, cssName, iconCls, href, style, onclick, type);
        }
        public static MvcHtmlString PartialViewOrEmptyDefault(this HtmlHelper html, string partialViewName)
        {
            var controllerContext = html.ViewContext.Controller.ControllerContext;
            var viewEngineResult = ViewEngines.Engines.FindPartialView(controllerContext, partialViewName);
            var isExist = (viewEngineResult.View != null);

            if (isExist)
                return html.Partial(partialViewName);
            else
                return new MvcHtmlString("");
        }

        public static MvcHtmlString OkWithRight(this HtmlHelper html, string id = "btnOk", string value = "确定", string iconCls = "fa-file-o", string area = "", string controller = "", string action = "", string method = "POST", string cssName = "dark", string href = "", string style="", string onclick = "")
        {
            return html.ButtonWithRight(id: id, value: value, area: area, controller: controller, action: action, method: method, cssName: cssName, iconCls: iconCls, href: href, style: style, onclick: onclick, type: "submit");
        }

        public static MvcHtmlString AddWithRightAction(this HtmlHelper html, string id = "btnAdd", string value = "新增", string iconCls = "fa-plus", string area = "", string controller = "", string action = "Add", string method = "GET", string cssName = "dark", string style = "", string onclick = "")
        {
            return html.ButtonWithRightAction(action, id, value, area, controller, method, cssName, iconCls, style, onclick, "Button");
        }

        public static MvcHtmlString AddWithRightHref(this HtmlHelper html, string id = "btnAdd", string value = "新增", string iconCls = "fa-plus",string method = "GET", string cssName = "btn-primary", string href = "", string style="", string onclick = "")
        {
            return html.ButtonWithRightHref(href, id, value, method, cssName, iconCls, style, onclick, "button");
        }

        public static MvcHtmlString ModifyWithRightAction(this HtmlHelper html, string id = "btnModify", string value = "修改", string iconCls = "fa-edit", string area = "", string controller = "", string action = "Modify", string method = "GET", string cssName = "btn-info",  string style="", string onclick = "")
        {
            return html.ButtonWithRightAction(action, id, value, area, controller, method, cssName, iconCls, style, onclick, "button");
        }

        public static MvcHtmlString ModifyWithRightHref(this HtmlHelper html, string id = "btnModify", string value = "修改", string iconCls = "fa-edit",  string method = "GET", string cssName = "btn-info", string href = "", string style="", string onclick = "")
        {
            return html.ButtonWithRightHref(href, id, value, method, cssName, iconCls, style, onclick, "button");
        }

        public static MvcHtmlString DeleteWithRightAction(this HtmlHelper html, string id = "btnDel", string value = "删除", string iconCls = "fa-times", string area = "", string controller = "", string action = "Del", string method = "GET", string cssName = "btn-warning",  string style="", string onclick = "")
        {
            return html.ButtonWithRightAction(action, id, value, area, controller, method, cssName, iconCls, style, onclick, "button");
        }

        public static MvcHtmlString DeleteWithRightHref(this HtmlHelper html, string id = "btnDel", string value = "删除", string iconCls = "fa-times",  string method = "GET", string cssName = "btn-warning", string href = "", string style="", string onclick = "")
        {
            return html.ButtonWithRightHref(href, id, value, method, cssName, iconCls, style, onclick, "button");
        }


        #endregion

        #region  Link
        public static MvcHtmlString Link(this HtmlHelper html, string id = "", string value = "", string cssName = "", string href = "", string style="", string onclick = "")
        {
            string data = String.Format("<a id=\"{0}\" class=\"{1}\"  style=\"{2}\" onclick=\"{3}\" href=\"{4}\">{5}</a> ", id, cssName, style, onclick, href, value);
            return new MvcHtmlString(data);
        }

        public static MvcHtmlString LinkWithRight(this HtmlHelper html, string id = "", string value = "", string area = "", string controller = "", string action = "", string method = "", string cssName = "btn btn-sm dark", string href = "", string style = "", string onclick = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString().Replace("-", "").Substring(1, 6);
                id = string.Format("btn_{0}", id);
            }
            
            if (string.IsNullOrWhiteSpace(area) && string.IsNullOrWhiteSpace(controller) && string.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("area"))
                {
                    area = html.ViewContext.RouteData.Values["area"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("controller"))
                {
                    controller = html.ViewContext.RouteData.Values["controller"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("action"))
                {
                    action = html.ViewContext.RouteData.Values["action"].ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                method = "Get";
            }

            string token = [REDACTED], controller, action, method);
           
            if (string.IsNullOrWhiteSpace(value))
            {
                var r = EngineContext.Current.Resolve<IAdminSysProvider>().GetRightByToken(token);
                if (r != null)
                {
                    value = r.RightName;
                }
            }

            string data = String.Format("<a id=\"{0}\" class=\"{1}\"  style=\"{2}\" onclick=\"{3}\" href=\"{4}\">{5}</a> ", id, cssName, style, onclick, href, value);
            return html.RawWithRight(data, token);
        }

        public static MvcHtmlString LinkWithRightHref(this HtmlHelper html, string href, string id = "", string value = "", string method = "", string cssName = "btn btn-sm dark", string style = "", string onclick = "")
        {
            var rdata = GetRouteTokenFromUrl(href);
            string area = rdata.Item1;
            string controller = rdata.Item2;
            string action = rdata.Item3;

            return html.LinkWithRight(id, value, area, controller, action, method, cssName, href, style, onclick);
        }

        public static MvcHtmlString RouteLinkWithRightHref(this HtmlHelper html, string href, string id = "", string value = "", string method = "", string cssName = "", string style="", string onclick = "")
        {
            var rdata = GetRouteTokenFromUrl(href);
            string area = rdata.Item1;
            string controller = rdata.Item2;
            string action = rdata.Item3;

            if (href.IndexOf("?") == -1)
            {
                href += "?";
            }
            else
            {
                href += "&";
            }

            var rightToken = [REDACTED];
            var r = EngineContext.Current.Resolve<IAdminSysProvider>().GetRightByToken(rightToken);
            href += "fromright=" + r.RightID.ToString();

            return html.LinkWithRight(id, value, area, controller, action, method, cssName, href, style, onclick);
        }

        public static MvcHtmlString LinkWithRightAction(this HtmlHelper html, string action, string id = "", string value = "", string area = "", string controller = "", string method = "", string cssName = "", string style="", string onclick = "")
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException("action");
            }

            if (string.IsNullOrWhiteSpace(area) && string.IsNullOrWhiteSpace(controller) && string.IsNullOrWhiteSpace(action))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("area"))
                {
                    area = html.ViewContext.RouteData.Values["area"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("controller"))
                {
                    controller = html.ViewContext.RouteData.Values["controller"].ToString();
                }
            }
            string href = new UrlHelper(html.ViewContext.RequestContext).Action(action, controller, new { area = area });

            return html.LinkWithRight(id, value, area, controller, action, method, cssName, href, style, onclick);
        }

        public static MvcHtmlString OkLinkWithRightAction(this HtmlHelper html, string id = "btnOk",string value = "确定",  string area = "", string controller = "", string action = "", string method = "POST", string cssName = "", string style="", string onclick = "document.getElementsByTagName('form')[0].submit();")
        {
            return html.LinkWithRightAction(action, id, value, area, controller, method, cssName, style, onclick);
        }

        public static MvcHtmlString OkLinkWithRightHref(this HtmlHelper html, string id = "btnOk", string value = "确定",  string method = "POST", string cssName = "", string href = "", string style="", string onclick = "document.getElementsByTagName('form')[0].submit();")
        {
            return html.LinkWithRightHref(href, id, value, method, cssName, style, onclick);
        }

        public static MvcHtmlString AddLinkWithRightAction(this HtmlHelper html, string id = "btnAdd", string value = "新增",string area = "", string controller = "", string action = "", string method = "GET", string cssName = "", string style="", string onclick = "")
        {
            return html.LinkWithRightHref(action, id, value, method, cssName, style, onclick);
        }

        public static MvcHtmlString AddLinkWithRightHref(this HtmlHelper html, string id = "btnAdd", string value = "新增", string area = "", string controller = "", string action = "", string method = "GET", string cssName = "", string href = "Add", string style="", string onclick = "")
        {
            return html.LinkWithRightHref(href, id, value, method, cssName, style, onclick);
        }

        public static MvcHtmlString ModifyLinkWithRightAction(this HtmlHelper html, string id = "btnModify", string value = "修改", string area = "", string controller = "", string action = "", string method = "GET", string cssName = "",string style="", string onclick = "")
        {
            return html.LinkWithRightAction(action, id, value, area, controller, method, cssName, style, onclick);
        }

        public static MvcHtmlString ModifyLinkWithRightHref(this HtmlHelper html, string id = "btnModify", string value = "修改", string method = "GET", string cssName = "btn btn-sm dark", string href = "Modify", string style = "", string onclick = "")
        {
            return html.LinkWithRightHref(href, id, value, method, cssName, style, onclick);
        }

        public static MvcHtmlString DeleteLinkWithRightAction(this HtmlHelper html, string id = "btnDel", string value="删除", string area = "", string controller = "", string action = "", string method = "GET", string cssName = "",  string style="", string onclick = "")
        {
            return html.LinkWithRightAction(action, id, value, area, controller, method, cssName, style, onclick);
        }

        public static MvcHtmlString DeleteLinkWithRightHref(this HtmlHelper html, string id = "btnDel", string value = "删除", string area = "", string controller = "", string action = "", string method = "GET", string cssName = "", string href = "Del", string style="", string onclick = "")
        {
            return html.LinkWithRightHref(href, id, value, method, cssName, style, onclick);
        }
        #endregion

        #region Button
        public static MvcHtmlString Button(this HtmlHelper html, string id = "btnOk", string value = "", string cssName = "", string iconCls = "", string href = "", string style="", string onclick = "", string type = "button")
        {
            string iconHtml = String.Empty;
            if (!String.IsNullOrWhiteSpace(iconCls))
            {
                iconHtml = String.Format("<i class=\"fa {0}\"></i> ", iconCls);
            }
            if (!String.IsNullOrWhiteSpace(href))
            {
                onclick = String.Format("window.location.href='{0}'", href);
            }

            string data = String.Format("<button id=\"{0}\" type=\"{6}\" class=\"btn {3}\"  style=\"{4}\" onclick=\"{5}\" >{1}{2}</button>", id, iconHtml, value, cssName, style, onclick, type);
            return new MvcHtmlString(data);
        }

        public static MvcHtmlString Back(this HtmlHelper html, string url = "")
        {
            string onclick = "";
            if (String.IsNullOrWhiteSpace(url))
            {
                onclick = "javascript:window.history.back();return false;";
            }
            else
            {
                onclick = String.Format("window.location.href='{0}'", url);
            }

            return html.Button(value: "返回", id: "btnBack", iconCls: " fa-mail-reply-all", onclick: onclick);
        }

        public static MvcHtmlString Ok(this HtmlHelper html, string id = "btnOk", string href = "", string style="", string onclick = "document.getElementsByTagName('form')[0].submit();")
        {
            return html.Button(value: "确定", id: id, iconCls: "fa-file-o", href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString Cancel(this HtmlHelper html, string id = "btnCancel", string href = "", string style="", string onclick = "")
        {
            return html.Button(value: "取消", id: id, iconCls: "fa-minus-circle", href: href, cssName: "btn-default", style: style, onclick: onclick, type: "reset");
        }
        #endregion

        #region UI Components
        /// <summary>
        /// 操作提示
        /// </summary>
        /// <param name="html"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static MvcHtmlString OperationTips(this HtmlHelper html, params string[] content)
        {
            string htmlformat = @"  <div class='note note-success'>
						                <h4 class='block'>操作提示:</h4>
                                        {0}						            
					                </div>
                                    ";
            if (content.Length <= 0)
            {
                return new MvcHtmlString("");
            }
            else
            {
                string plist = "";
                foreach (string c in content)
                {
                    plist += "<p>" + c + "</p>";
                }
                return new MvcHtmlString(htmlformat.Replace("{0}", plist));
            }
        }

        #endregion

        #region Nav Tab
        /// <summary>
        /// nav tab
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onclick"></param>
        /// <returns></returns>
        public static MvcHtmlString Tab(this HtmlHelper html,string name, string onclick)
        {
            return new MvcHtmlString(String.Format("<label class=\"btn dark\" onclick=\"{0}\"><input type=\"radio\" class=\"toggle\">{1}</label>", onclick, name));
        }

        /// <summary>
        /// nav right tab
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onclick"></param>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MvcHtmlString TabWithRight(this HtmlHelper html,string name="", string onclick = "", string area = "", string controller = "", string action = "", string method = "GET")
        {
            if (String.IsNullOrWhiteSpace(controller))
            {
                if (html.ViewContext.RouteData.Values.ContainsKey("controller"))
                {
                    controller = html.ViewContext.RouteData.Values["controller"].ToString();
                }
            }

            if (String.IsNullOrWhiteSpace(onclick) && !String.IsNullOrWhiteSpace(action))
            {
                 string href = new UrlHelper(html.ViewContext.RequestContext).Action(action, controller, new { area = area});
                 onclick = String.Format("window.location.href='{0}'", href);
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);
                var r = EngineContext.Current.Resolve<IAdminSysProvider>().GetRightByToken(token);
                name = r.RightName;
            }

            string data = String.Format("<label class=\"btn dark\" onclick=\"{0}\"><input type=\"radio\" class=\"toggle\">{1}</label>", onclick, name);
            return html.RawWithRight(data, area, controller, action, method);
        }

        public static MvcHtmlString TabWithRight(this HtmlHelper html, string name = "", string href="")
        {
            var rdata = GetRouteTokenFromUrl(href);
            string area = rdata.Item1;
            string controller = rdata.Item2;
            string action = rdata.Item3;

            string onclick = String.Format("window.location.href='{0}'", href);

            return html.TabWithRight(name, onclick, area, controller, action, "get");
        }
        #endregion


    }
}