using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Reflection;


namespace FW.Core.Auth.AuthorizeRule.Mvc
{
    public class ReflectionRuleProvider : IAuthorizeRuleProvider
    {
        public AuthorizeRuleColl LoadRules()
        {
            //从所有程序集中查找所有Controller
            //ps：实现接口IController并且类名以Controller结尾
            var types = new List<Type>();
            FW.Core.Helper.SystemEnvironment.Assemblies
                .ToList()
                .ForEach(assem =>
                assem.GetTypes().ToList().ForEach(t => {
                    if (typeof(IController).IsAssignableFrom(t) && t.Name.EndsWith("Controller"))
                    {
                        types.Add(t);
                    }
                }
                ));

            var rules = new AuthorizeRuleColl();
            types.ForEach(controllerType => {
                
                //解析Area
                string area = "*";
                string nsName = controllerType.Namespace;
                Type areaType = controllerType.Assembly.GetTypes().ToList()
                    .Where(at => typeof(AreaRegistration).IsAssignableFrom(at)
                    && at.Namespace == nsName
                    ).FirstOrDefault();

                if (areaType != null)
                {
                    area = (Activator.CreateInstance(areaType) as AreaRegistration).AreaName;
                }
                if (String.IsNullOrWhiteSpace(area))
                {
                    area = "*";
                }

                //控制器
                string controller = controllerType.Name.Replace("Controller", "");

                #region action
                //查找所有打了标签的方法
                var mis = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                mis.ToList().ForEach(mi => {
                    if (mi.CustomAttributes.OfType<NonActionAttribute>().FirstOrDefault() != null)
                    {
                        return;
                    }

                    //为了减少授权规则的体积，默认不映射未打标签的方法
                    if (mi.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault() == null
                        && mi.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().FirstOrDefault() == null
                        && mi.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().FirstOrDefault() == null)
                    {
                        return;
                    }

                    var rule = new AuthorizeRule();
                    //获取HttpMethod
                    string httpMethod = getHttpMethod(mi);
                    var token = [REDACTED] Token.MvcSegmentRightToken();
                    token.Area = area;
                    token.Controller = controller;
                    token.Action = mi.Name;
                    token.Method = httpMethod;

                    rule.RightToken = [REDACTED];

                    //开放访问
                    var noAuthAttr = mi.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault();
                    if (noAuthAttr != null)
                    {
                        rule.AllowRoles.Add(AuthorizeRule.ALLROLES);
                        rules.Add(rule);

                        return;
                    }
                    
                    //允许的角色
                    var roleAttrs = mi.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().ToList();
                    if (roleAttrs != null && roleAttrs.Count > 0)
                    {
                        roleAttrs.ForEach(roleAttr => {
                            rule.AllowRoles.AddRange(roleAttr.AllowRoles);
                        });
                    }

                    //拒绝的用户
                    var userAttrs = mi.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().ToList();
                    if (userAttrs != null && userAttrs.Count > 0)
                    {
                        userAttrs.ForEach(userAttr => {
                            rule.DenyUsers.AddRange(userAttr.DenyUsers);
                        });
                    }

                    rules.Add(rule);
                });
                #endregion

                #region Controller
                //检查controller级别标签
                //为了减少授权规则的体积，默认不映射未打标签的控制器
                if (controllerType.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault() != null
                        || controllerType.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().FirstOrDefault() != null
                        || controllerType.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().FirstOrDefault() != null)
                {
                    var rule = new AuthorizeRule();
                    var token = [REDACTED] Token.MvcSegmentRightToken();
                    token.Area = area;
                    token.Controller = controller;
                    token.Action = "*";
                    token.Method = "*";

                    rule.RightToken = [REDACTED];

                    //开放访问
                    var noAuthAttr = controllerType.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault();
                    if (noAuthAttr != null)
                    {
                        rule.AllowRoles.Add("*");
                        rules.Add(rule);
                        return;
                    }

                    //允许的角色
                    var roleAttrs = controllerType.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().ToList();
                    if (roleAttrs != null && roleAttrs.Count > 0)
                    {
                        roleAttrs.ForEach(roleAttr =>
                        {
                            rule.AllowRoles.AddRange(roleAttr.AllowRoles);
                        });
                    }

                    //拒绝的用户
                    var userAttrs = controllerType.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().ToList();
                    if (userAttrs != null && userAttrs.Count > 0)
                    {
                        userAttrs.ForEach(userAttr =>
                        {
                            rule.DenyUsers.AddRange(userAttr.DenyUsers);
                        });
                    }

                    rules.Add(rule);
                    

                    rules.Add(rule);

                }
                #endregion

                #region area
                //检查area级别标签
                //为了减少授权规则的体积，默认不映射未打标签的Area
                if (areaType == null)
                {
                    return;
                }

                if (areaType.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault() != null
                        || areaType.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().FirstOrDefault() != null
                        || areaType.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().FirstOrDefault() != null)
                {
                    var rule = new AuthorizeRule();
                    var token = [REDACTED] Token.MvcSegmentRightToken();
                    token.Area = area;
                    token.Controller = "*";
                    token.Action = "*";
                    token.Method = "*";

                    rule.RightToken = [REDACTED];

                    //开放访问
                    var noAuthAttr = areaType.GetCustomAttributes().OfType<Attribute.NonAuthorizeAttribute>().FirstOrDefault();
                    if (noAuthAttr != null)
                    {
                        rule.AllowRoles.Add("*");
                        rules.Add(rule);
                        return;
                    }

                    //允许的角色
                    var roleAttrs = areaType.GetCustomAttributes().OfType<Attribute.AllowRolesAttribute>().ToList();
                    if (roleAttrs != null && roleAttrs.Count > 0)
                    {
                        roleAttrs.ForEach(roleAttr =>
                        {
                            rule.AllowRoles.AddRange(roleAttr.AllowRoles);
                        });
                    }

                    //拒绝的用户
                    var userAttrs = areaType.GetCustomAttributes().OfType<Attribute.DenyUsersAttribute>().ToList();
                    if (userAttrs != null && userAttrs.Count > 0)
                    {
                        userAttrs.ForEach(userAttr =>
                        {
                            rule.DenyUsers.AddRange(userAttr.DenyUsers);
                        });
                    }

                    rules.Add(rule);
                }
                #endregion

            });

            return rules;
        }

        /// <summary>
        /// 获取Action的Method标签
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        private string getHttpMethod(MethodInfo mi)
        {
            string method = "*";
            if (mi.CustomAttributes.OfType<HttpGetAttribute>().FirstOrDefault() != null)
            {
                method = "GET";
            }
            else if (mi.CustomAttributes.OfType<HttpPostAttribute>().FirstOrDefault() != null)
            {
                method = "post";
            }
            else if (mi.CustomAttributes.OfType<HttpPutAttribute>().FirstOrDefault() != null)
            {
                method = "put";
            }
            else if (mi.CustomAttributes.OfType<HttpOptionsAttribute>().FirstOrDefault() != null)
            {
                method = "options";
            }
            else if (mi.CustomAttributes.OfType<HttpDeleteAttribute>().FirstOrDefault() != null)
            {
                method = "delete";
            }

            return method;
        }


    }
}
