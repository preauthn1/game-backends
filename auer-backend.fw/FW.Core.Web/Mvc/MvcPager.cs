using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using FW.Core.Engine;
using FW.Core.Helper;

namespace FW.Core.Web.Mvc
{
    public static class MvcPager
    {
        public static Language.LanguageResource Lang;

        /// <summary>
        /// 首页
        /// </summary>
        public const string PAGERLANGKEY_FIRST = "First";
        /// <summary>
        /// 上一页
        /// </summary>
        public const string PAGERLANGKEY_PREVIOUS = "Previous";
        /// <summary>
        /// 下一页
        /// </summary>
        public const string PAGERLANGKEY_NEXT = "Next";
        /// <summary>
        /// 末页
        /// </summary>
        public const string PAGERLANGKEY_TAIL = "Tail";

        private const string ZH_CN = "zh-cn";
        private const string ZH_TW = "zh-tw";
        private const string EN_US = "en-us";

        /// <summary>
        /// cctor
        /// </summary>
        static MvcPager()
        {
            Lang = new Language.LanguageResource() ;

            var cn = new Language.LanguagePackage();
            cn.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_FIRST, Value = "首页" });
            cn.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_NEXT, Value = "下一页" });
            cn.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_PREVIOUS, Value = "上一页" });
            cn.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_TAIL, Value = "末页" });

            Lang.Add(ZH_CN,  cn);
            Lang.Add(ZH_TW, cn);


            var en = new Language.LanguagePackage();
            en.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_FIRST, Value = "First" });
            en.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_NEXT, Value = "Next" });
            en.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_PREVIOUS, Value = "Previous" });
            en.Add(new Language.LanguageItem() { Key = PAGERLANGKEY_TAIL, Value = "Last" });

            Lang.Add(EN_US, cn);

        }

        public static MvcHtmlString Pager(this HtmlHelper html, int pageSize, int totalCount, string pageStr = "p")
        {
            string culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            var pagerLang =  Lang[culture];

            var queryString = html.ViewContext.HttpContext.Request.QueryString;
            int currentPageIndex = 1;

            if (queryString[pageStr] != null)
            {
                int.TryParse(queryString[pageStr], out currentPageIndex);
            }
            else if (html.ViewContext.RequestContext.HttpContext.Request.Form[pageStr] != null)
            {
                int.TryParse(html.ViewContext.RequestContext.HttpContext.Request.Form[pageStr], out currentPageIndex);
            }

            if (currentPageIndex == 0)
            {
                currentPageIndex = 1;
            }

            //计算总页数
            int totalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(totalCount) / pageSize));
            var dct = new RouteValueDictionary(html.ViewContext.RouteData.Values);
            var output = new StringBuilder();
            foreach (string key in queryString.Keys)
            {
                if (!String.IsNullOrWhiteSpace(key))
                {
                    dct[key] = queryString[key];
                }
            }

            if (totalPage >= 1)
            {
                output.Append("<span>");
                output.Append("共" + totalCount + "条数据");
                output.Append("</span>");

                if (currentPageIndex != 1)
                {
                    dct[pageStr] = 1;
                    output.Append("<span>");
                    output.Append(html.RouteLink(pagerLang[PAGERLANGKEY_FIRST], dct));
                    output.Append("</span>");
                }
                else
                {
                    output.Append("<span>");
                    output.Append(pagerLang[PAGERLANGKEY_FIRST]);
                    output.Append("</span>");
                }
                output.Append(" ");

                if (currentPageIndex > 1)
                {
                    dct[pageStr] = currentPageIndex - 1;
                    output.Append("<span>");
                    output.Append(html.RouteLink(pagerLang[PAGERLANGKEY_PREVIOUS], dct));
                    output.Append("</span>");
                }
                else
                {
                    output.Append("<span>");
                    output.Append(pagerLang[PAGERLANGKEY_PREVIOUS]);
                    output.Append("</span>");
                }

                output.Append(" ");

                output.Append("<ul>");
                int pagerNum = 3;
                for (int i = 0; i <= pagerNum * 2; i++)
                {
                    if ((currentPageIndex + i - pagerNum) >= 1 && (currentPageIndex + i - pagerNum) <= totalPage)
                    {
                        if (currentPageIndex + i - pagerNum == currentPageIndex)
                        {
                            output.Append("<li class='current'>");
                            output.Append((currentPageIndex + i - pagerNum).ToString());
                            output.Append("</li>");
                        }
                        else
                        {
                            int pageNumber = currentPageIndex + i - pagerNum;
                            dct[pageStr] = pageNumber;

                            output.Append("<li>");
                            output.Append(html.RouteLink(pageNumber.ToString(), dct));
                            output.Append("</li>");
                        }

                        output.Append(" ");
                    }
                }
                output.Append("</ul>");

                if (currentPageIndex < totalPage)
                {
                    dct[pageStr] = currentPageIndex + 1;

                    output.Append("<span>");
                    output.Append(html.RouteLink(pagerLang[PAGERLANGKEY_NEXT], dct));
                    output.Append("</span>");
                }
                else
                {
                    output.Append("<span>");
                    output.Append(pagerLang[PAGERLANGKEY_NEXT]);
                    output.Append("</span>");
                }

                output.Append(" ");

                if (currentPageIndex != totalPage)
                {
                    dct[pageStr] = totalPage;
                    output.Append("<span>");
                    output.Append(html.RouteLink(pagerLang[PAGERLANGKEY_TAIL], dct));
                    output.Append("</span>");
                }
                else
                {
                    output.Append("<span>");
                    output.Append(pagerLang[PAGERLANGKEY_TAIL]);
                    output.Append("</span>");
                }

                output.Append(" ");

                output.Append("<span>");
                output.AppendFormat("{0}/{1}", currentPageIndex, totalPage);
                output.Append("</span>");

            }

            return MvcHtmlString.Create(output.ToString());

        }

        public static MvcHtmlString Pager<TObject>(this HtmlHelper html, PagedList<TObject> data, string pageStr = "p")
        {
            return Pager(html, data.PageSize, data.TotalCount, pageStr);
        }

    }
}

