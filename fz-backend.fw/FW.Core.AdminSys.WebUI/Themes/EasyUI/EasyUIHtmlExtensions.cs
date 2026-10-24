using KCKJ.Core.AdminSys.Provider;
using KCKJ.Core.Auth;
using KCKJ.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KCKJ.Core.AdminSys.WebUI.Themes.EasyUI
{
    /// <summary>
    /// EasyUI Html扩展
    /// </summary>
    public static class EasyUIHtmlExtensions
    {
        #region Right Button
        public static MvcHtmlString RawWithRight(this HtmlHelper html,string data, string area, string controller, string action, string method)
        {
            string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);

            if (String.IsNullOrWhiteSpace(token))
            {
                return new MvcHtmlString("");
            }

            AuthManager authManager = EngineContext.Current.Resolve<AuthManager>();
            IAdminSysProvider provider = EngineContext.Current.Resolve<IAdminSysProvider>();

            if(provider.CheckUserRight(Convert.ToInt32(authManager.CurrentUser.UserID), token))
            {
                return new MvcHtmlString(data);
            }

            return new MvcHtmlString("");
        }

        public static MvcHtmlString ButtonWithRight(this HtmlHelper html, string id = "", string value = "", string area = "", string controller = "", string action = "", string method = "*", string cssName = "", string iconCls = "", string href = "#", string style = "width:80px", string onclick = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString().Replace("-", "").Substring(1, 6);
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = "";
            }

            if (String.IsNullOrWhiteSpace(area))
            {
                area = "";
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                controller = html.ViewContext.RouteData.GetRequiredString("controller");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                action = html.ViewContext.RouteData.GetRequiredString("action");
            }

            if (String.IsNullOrWhiteSpace(method))
            {
                method = "";
            }
                

            string data = String.Format("<a id=\"{0}\" class=\"easyui-linkbutton {3}\" data-options=\"iconCls:'{1}'\" style=\"{4}\" onclick=\"{5}\" href=\"{6}\">{2}</a> ", id,  iconCls, value, cssName, style, onclick, href);
            return html.RawWithRight(data,area, controller, action, method);
        }

        public static MvcHtmlString OkWithRight(this HtmlHelper html, string id = "btnOk", string area = "", string controller = "", string action = "", string method = "POST", string cssName = "", string href = "#", string style = "width:80px", string onclick = "if ($('form').form('validate')) {$('form').submit();}")
        {
            return html.ButtonWithRight(id: id, value: "确定", area: area, controller: controller, action: action, method: method, cssName: cssName, iconCls: "icon-ok", href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString AddWithRight(this HtmlHelper html, string id = "btnAdd", string area = "", string controller = "", string action = "Add", string method = "GET", string cssName = "", string href = "Add", string style = "width:80px", string onclick = "")
        {
            return html.ButtonWithRight(id: id, value: "新增", area: area, controller: controller, action: action, method: method, cssName: cssName, iconCls: "icon-add", href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString ModifyWithRight(this HtmlHelper html, string id = "btnModify", string area = "", string controller = "", string action = "Modify", string method = "GET", string cssName = "", string href = "Modify", string style = "width:80px", string onclick = "")
        {
            return html.ButtonWithRight(id: id, value: "修改", area: area, controller: controller, action: action, method: method, cssName: cssName, iconCls: "icon-edit", href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString DeleteWithRight(this HtmlHelper html, string id = "btnDel", string area = "", string controller = "", string action = "Del", string method = "GET", string cssName = "", string href = "Del", string style = "width:80px", string onclick = "")
        {
            return html.ButtonWithRight(id: id, value: "删除", area: area, controller: controller, action: action, method: method, cssName: cssName, iconCls: "icon-remove", href: href, style: style, onclick: onclick);
        }
        #endregion

        #region Right Link
        public static MvcHtmlString LinkWithRight(this HtmlHelper html, string id = "", string value = "", string area = "", string controller = "", string action = "", string method = "*", string cssName = "", string href = "#", string style = "width:80px", string onclick = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString().Replace("-", "").Substring(1, 6);
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = "";
            }

            if (String.IsNullOrWhiteSpace(area))
            {
                area = "";
            }

            if (String.IsNullOrWhiteSpace(controller))
            {
                controller = html.ViewContext.RouteData.GetRequiredString("controller");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                action = html.ViewContext.RouteData.GetRequiredString("action");
            }

            if (String.IsNullOrWhiteSpace(method))
            {
                method = "";
            }


            string data = String.Format("<a id=\"{0}\" class=\"{1}\"  style=\"{2}\" onclick=\"{3}\" href=\"{4}\">{5}</a> ", id, cssName, style, onclick, href, value);
            return html.RawWithRight(data, area, controller, action, method);
        }

        public static MvcHtmlString OkLinkWithRight(this HtmlHelper html, string id = "btnOk", string area = "", string controller = "", string action = "", string method = "POST", string cssName = "", string href = "#", string style = "width:80px", string onclick = "if ($('form').form('validate')) {$('form').submit();}")
        {
            return html.LinkWithRight(id: id, value: "确定", area: area, controller: controller, action: action, method: method, cssName: cssName, href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString AddLinkWithRight(this HtmlHelper html, string id = "btnAdd", string area = "", string controller = "", string action = "Add", string method = "GET", string cssName = "", string href = "Add", string style = "width:80px", string onclick = "")
        {
            return html.LinkWithRight(id: id, value: "新增", area: area, controller: controller, action: action, method: method, cssName: cssName, href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString ModifyLinkWithRight(this HtmlHelper html, string id = "btnModify", string area = "", string controller = "", string action = "Modify", string method = "GET", string cssName = "", string href = "Modify", string style = "width:80px", string onclick = "")
        {
            return html.LinkWithRight(id: id, value: "修改", area: area, controller: controller, action: action, method: method, cssName: cssName, href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString DeleteLinkWithRight(this HtmlHelper html, string id = "btnDel", string area = "", string controller = "", string action = "Del", string method = "GET", string cssName = "", string href = "Del", string style = "width:80px", string onclick = "")
        {
            return html.LinkWithRight(id: id, value: "删除", area: area, controller: controller, action: action, method: method, cssName: cssName, href: href, style: style, onclick: onclick);
        }
        #endregion

        #region Button
        public static MvcHtmlString Button(this HtmlHelper html, string id = "btnOk", string value = "", string cssName = "", string iconCls = "", string href = "#", string style = "width:80px", string onclick = "")
        {
            string data = String.Format("<a id=\"{0}\" class=\"easyui-linkbutton {3}\" data-options=\"iconCls:'{1}'\" style=\"{4}\" onclick=\"{5}\" href=\"{6}\">{2}</a> ", id, iconCls, value, cssName, style, onclick, href);
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

            return html.Button(value: "返回", id: "btnBack", iconCls: "icon-back", onclick: onclick);
        }

        public static MvcHtmlString Ok(this HtmlHelper html, string id = "btnOk", string href = "#", string style = "width:80px", string onclick = "if ($('form').form('validate')) {$('form').submit();}")
        {
            return html.Button(value: "确定", id: id, iconCls : "icon-ok", href: href, style: style, onclick: onclick);
        }

        public static MvcHtmlString Cancel(this HtmlHelper html, string id = "btnCancel", string href = "#", string style = "width:80px", string onclick = "")
        {
            return html.Button(value: "取消", id: id, iconCls: "icon-cancel", href: href, style: style, onclick: onclick);
        }
        #endregion

    }
}