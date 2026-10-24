using FW.Core.AdminSys.Provider.RightFinder.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Mvc;

namespace FW.Core.AdminSys.Provider.RightFinder
{
    public class MvcReflectedRightFinder :IRightFinder
    {
        public List<RightInfo> GetRightInfo()
        {
            var rights = new List<RightInfo>();

            var assems = BuildManager.GetReferencedAssemblies();


            foreach (Assembly assem in assems)
            {
               var types = assem.GetTypes();
               foreach (var type in types)
               {
                   RightInfo right = null;

                   string areaName = "";
                   string controllerName = "";
                   //区域
                   if (!type.IsAbstract && typeof(AreaRegistration).IsAssignableFrom(type))
                   {
                       var attr = type.GetCustomAttributes(true).OfType<RightInfoAttribute>().FirstOrDefault();
                       if (attr != null)
                       {
                           areaName = (Activator.CreateInstance(type) as AreaRegistration).AreaName;
                           right = new RightInfo();
                           right.PageUrl = string.Empty;
                           right.RightName = attr.RightName;
                           right.RightToken = [REDACTED];
                           right.RightType = attr.RightType;
                           right.Sort = attr.Sort;
                           right.PageUrl = attr.PageUrl;

                           if (String.IsNullOrWhiteSpace(right.RightToken))
                           {
                               right.RightToken = [REDACTED]"{0}_{1}_{2}_{3}", areaName, "", "" , "");
                           }
                           
                       }
                   }

                   //controller
                   if (!type.IsAbstract && typeof(IController).IsAssignableFrom(type))
                   {
                       //controller标签
                       var attr = type.GetCustomAttributes(true).OfType<RightInfoAttribute>().FirstOrDefault();

                       if (attr != null)
                       {
                           RightInfo cRight = null;
                           if (right == null)
                           {
                               right = new RightInfo();
                               cRight = right;
                           }
                           else
                           {
                               right.ChildRights.Add(cRight);
                           }

                           controllerName = type.Name.ToLower().Replace("controller", "") ;
                           cRight.PageUrl = attr.PageUrl;
                           cRight.RightName = attr.RightName;
                           cRight.RightToken = [REDACTED];
                           cRight.RightType = attr.RightType;
                           cRight.Sort = attr.Sort;
                           if (String.IsNullOrWhiteSpace(cRight.RightToken))
                           {
                               cRight.RightToken = [REDACTED]"{0}_{1}_{2}_{3}",areaName, controllerName,  "", "");
                           }
                       }

                       if(right == null)
                       {
                           continue;
                       }

                       //action标签
                       var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                       if (methods != null && methods.Length > 0)
                       {
                           foreach (MethodInfo mi in methods)
                           {
                               if (mi.GetCustomAttributes().OfType<NonActionAttribute>().FirstOrDefault() != null)
                               {
                                   continue;
                               }

                               string methodName = mi.Name;
                               string methodType = "*";

                               var mright = new RightInfo();

                               mright.PageUrl = attr.PageUrl;
                               mright.RightName = attr.RightName;
                               mright.RightToken = [REDACTED];
                               mright.RightType = attr.RightType;
                               mright.Sort = attr.Sort;

                               if (mi.GetCustomAttributes().OfType<HttpPatchAttribute>().FirstOrDefault() != null)
                               {
                                   methodType = "PATCH";
                               }
                               if(mi.GetCustomAttributes().OfType<HttpGetAttribute>().FirstOrDefault() != null)
                               {
                                   methodType = "GET";
                               }
                               if (mi.GetCustomAttributes().OfType<HttpPostAttribute>().FirstOrDefault() != null)
                               {
                                   methodType = "POST";
                               }
                               if (mi.GetCustomAttributes().OfType<HttpPutAttribute>().FirstOrDefault() != null)
                               {
                                   methodType = "PUT";
                               }
                               if (mi.GetCustomAttributes().OfType<HttpDeleteAttribute>().FirstOrDefault() != null)
                               {
                                   methodType = "DELETE";
                               }

                               if (String.IsNullOrWhiteSpace(mright.RightToken))
                               {
                                   mright.RightToken = [REDACTED]"{0}_{1}_{2}_{3}", areaName, controllerName, methodName, methodType);
                               }
                               if (String.IsNullOrWhiteSpace(mright.PageUrl) && mright.RightType == Entity.RightType.PageRight)
                               {
                                   if (!String.IsNullOrWhiteSpace(areaName))
                                   {
                                       mright.PageUrl = String.Format("/{0}/{1}/{2}/", areaName, controllerName, methodName);
                                   }
                                   else
                                   {
                                       mright.PageUrl = String.Format("/{1}/{2}/", controllerName, methodName);
                                   }
                               }
                           }
                       }

                   }

                   if (right != null)
                   {
                       rights.Add(right);
                   }
               }
            }

            return rights;
        }
    }
}
