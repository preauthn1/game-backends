using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using game.Infrastructure.Extensions;

namespace FW.Core.AdminSys.WebUI.Themes.Bootstrap
{
    public static class CustomHtmlHelper
    {
        public static bool CheckFunctionPermissionExist(this HtmlHelper html, string controller, string actionName, string method = "GET")
        {
            return CheckFunctionPermissionExist(controller , actionName , method);
        }

        public static bool CheckFunctionPermissionExist(string controller, string actionName, string method = "GET")
        {
            string token = [REDACTED]"", controller, actionName, method);

            //底層有約30秒cache 不過終究還是會頻繁queryDB
            var isExist = BootstrapHtmlExtensions.CheckRightToken(token);
            return isExist;
        }

        public static string DisplayEnumText<T>(this HtmlHelper html, int value) where T : IComparable, IFormattable, IConvertible
        {
            T convert = (T)((object)value);
            var text = convert.GetEnumDescriptionForGeneric();
            return text;
        }

        /// <summary>
        /// 判斷是不是預設的空字串日期
        /// </summary>
        public static string DisplayTimeOrEmptyString(this HtmlHelper html, DateTime date , string format = "yyyy-MM-dd")
        {
            if (date == new DateTime())
                return string.Empty;

            if (date.Year == 1900)
                return string.Empty;

            return date.ToString(format);
        }
               
        public static MvcHtmlString RenderDropdownByEnum<T>(this HtmlHelper html, string elementId, int? defaultValue = null, bool isIncludePleaseSelect = false , string cssClass = "form-control input-small") where T : IComparable, IFormattable, IConvertible
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<T>();

            string myHtml = $"<select id='{elementId}' name='{elementId}' class='{cssClass}' >";

            if (isIncludePleaseSelect)
            {
                myHtml += $"<option value=''>--请选择--</option>";
            }

            foreach (dynamic enumValue in allEnums)
            {
                string text = ExtensionOfDependModel.GetEnumDescription(enumValue);
                string defaultSelectHtml = string.Empty;

                if (defaultValue.HasValue &&
                   ((int)enumValue) == defaultValue.Value)
                {
                    defaultSelectHtml = " selected='selected' ";
                }

                myHtml += $"<option value='{(int)enumValue}'  {defaultSelectHtml} >{text}</option>";
            }

            myHtml += " </select>";
            return new MvcHtmlString(myHtml);
        }


        public static List<SelectListItem> GetSelectListItemByEnum<T>(this HtmlHelper html , bool isIncludePleaseSelect = false , string defaultSelectedValue = "") where T : IComparable, IFormattable, IConvertible
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<T>();

            var listItems = new List<SelectListItem>();

            if (isIncludePleaseSelect)
            {
                listItems.Add(new SelectListItem
                {
                    Text = "--请选择--",
                    Value = "",
                    Selected = true
                });
            }

            foreach (dynamic enumValue in allEnums)
            {
                string text = ExtensionOfDependModel.GetEnumDescription(enumValue);
                string defaultSelectHtml = string.Empty;

                listItems.Add(new SelectListItem
                {
                    Text = text,
                    Value = ((int)enumValue).ToString()
                });
            }


            foreach(var item in listItems)
            {
                if(item.Value == defaultSelectedValue)
                {
                    item.Selected = true;
                }
            }


            return listItems;
        }



    }
}