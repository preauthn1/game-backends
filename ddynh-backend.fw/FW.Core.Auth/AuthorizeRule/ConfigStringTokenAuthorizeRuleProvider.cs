using System;
using System.Configuration;
using System.Xml;


namespace FW.Core.Auth.AuthorizeRule
{
    public class ConfigStringTokenAuthorizeRuleProvider : IAuthorizeRuleProvider, IConfigurationSectionHandler
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
                    rule.RightToken = [REDACTED] Token.StringRightToken(node.Attributes["token"].Value);

                    if (node.Attributes["allowRoles"] != null)
                    {
                        string allowRoles = node.Attributes["allowRoles"].Value;
                        string[] allowRolesArr = allowRoles.Split(new char[]{','});
                        Array.ForEach(allowRolesArr, r => {
                            rule.AllowRoles.Add(r);
                        });

                    }

                    if (node.Attributes["denyUsers"] != null)
                    {
                        string denyUsers = node.Attributes["denyUsers"].Value;
                        string[] denyUsersArr = denyUsers.Split(new char[]{','});
                        Array.ForEach(denyUsersArr, du => {
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
