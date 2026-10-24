using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Linq;

namespace FW.Core.Auth.UserRight
{
    public class UserRightConfigHandler : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            UserRightCollection urs = new UserRightCollection();

            var userNodes = section.SelectNodes("//user");

            if (userNodes != null && userNodes.Count > 0)
            {
                foreach (XmlNode node in userNodes)
                {
                    var ur = new UserRight();
                    urs.Add(ur);

                    ur.UserName = node.Attributes["name"].Value;

                    if (urs.Where(r => r.UserName == ur.UserName).Count() > 1)
                    {
                        throw new AuthException(String.Format("不止一个用户{0}的权限配置信息", ur.UserName));
                    }

                    //用户角色
                    var roleNode = node.SelectSingleNode("userRole");
                    if (roleNode != null)
                    {
                        string userroles = roleNode.InnerText;
                        if (!String.IsNullOrWhiteSpace(userroles))
                        {
                            var urArr =  userroles.Split(new char[]{','});
                            Array.ForEach(urArr, r => {
                                if (!String.IsNullOrWhiteSpace(r))
                                {
                                    ur.UserRoles.Add(r);
                                }
                            });
                        }
                    }

                    //用户权限
                    var rightNodes = node.SelectNodes("//right");
                    if (rightNodes != null && rightNodes.Count > 0)
                    {
                        foreach (XmlNode rightNode in rightNodes)
                        {
                            string rightStr = rightNode.InnerText;
                            if (!String.IsNullOrWhiteSpace(rightStr))
                            {
                                ur.UserRights.Add(rightStr);
                            }
                        }
                    }
                }
            }

            return urs;
        }

      
    }
}
