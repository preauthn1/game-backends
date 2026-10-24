using System;
using System.Configuration;
using System.Xml;

namespace FW.Core.Auth.AuthorizeRule.Mvc
{
    public class ConfigMvcTokenAuthorizeRuleProvider : IAuthorizeRuleProvider, IConfigurationSectionHandler
    {
        public AuthorizeRuleColl LoadRules()
        {
            return System.Configuration.ConfigurationManager.GetSection("authConfig") as AuthorizeRuleColl;  
        }

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            AuthorizeRuleColl rules = new AuthorizeRuleColl();

            if (section.Attributes["logonUrl"] != null)
            {
                rules.LogonUrl = section.Attributes["logonUrl"].Value;
            }

            XmlNodeList nodes = section.SelectNodes("//rule");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    var rule = new AuthorizeRule();
                    string area = "*";
                    string controller = "*";
                    string action = "*";
                    string method = "*";

                    if (node.Attributes["area"] != null)
                    {
                        area = node.Attributes["area"].Value;
                    }
                    if (node.Attributes["controller"] != null)
                    {
                        controller = node.Attributes["controller"].Value;
                    }
                    if (node.Attributes["action"] != null)
                    {
                        action = node.Attributes["action"].Value;
                    }
                    if (node.Attributes["method"] != null)
                    {
                        method = node.Attributes["method"].Value;
                    }
                    string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);
                    rule.RightToken = [REDACTED] Token.MvcSegmentRightToken(token);

                    if (node.Attributes["allowRoles"] != null)
                    {
                        string allowRoles = node.Attributes["allowRoles"].Value;
                        string[] allowRolesArr = allowRoles.Split(new char[] { ',' });
                        Array.ForEach(allowRolesArr, r =>
                        {
                            rule.AllowRoles.Add(r);
                        });

                    }

                    if (node.Attributes["denyUsers"] != null)
                    {
                        string denyUsers = node.Attributes["denyUsers"].Value;
                        string[] denyUsersArr = denyUsers.Split(new char[] { ',' });
                        Array.ForEach(denyUsersArr, du =>
                        {
                            rule.DenyUsers.Add(du);
                        });
                    }


                    rules.Add(rule);

                }

            }

            return rules;
        }
    }
}
