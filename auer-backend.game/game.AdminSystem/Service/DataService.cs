using game.Entity.Area;
using game.Entity.Customer;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.Users;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace game.AdminSystem
{
    public static class DataService
    {
        public static List<ReplyType> GetReplyType()
        {
            return SiteHelper.GetAPIResult<List<ReplyType>>(string.Format("/Customer/GetReplyType?adminID={0}", SiteService.GetAdminUserID()));
        }
        public static List<ReplyContent> GetReplyContent()
        {
            return SiteHelper.GetAPIResult<List<ReplyContent>>(string.Format("/Customer/GetReplyContent?adminID={0}", SiteService.GetAdminUserID()));
        }

        /// <summary>
        /// 现金流通类型分组
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<SelectListItem> MoneySourceGroup(string selected = "", bool all = true)
        {
            var list = new List<SelectListItem>();
            foreach (var item in SiteHelper.GetAPIResult<List<MoneySourceGroup>>("/Money/GetMoneySourceGroup"))
            {
                list.Add(new SelectListItem()
                {
                    Text = GetSpellCode(item.GroupName.Substring(0, 1)) + item.GroupName,
                    Value = item.Id.ToString(),
                    Selected = item.Id.ToString() == selected
                });
            }
            if (all == true) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "所有类型",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }
        /// <summary>
        /// 现金流通类型
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<SelectListItem> MoneySourceType(string selected = "", bool all = true)
        {
            var list = new List<SelectListItem>();

            foreach (var type in SiteHelper.GetAPIResult<List<MoneySourceType>>("/Money/GetMoneySourceType"))
            {
                list.Add(new SelectListItem()
                {
                    Text = GetSpellCode(type.SourceName.Substring(0, 1)) + type.SourceName,
                    Value = type.Id.ToString(),
                    Selected = type.Id.ToString() == selected
                });
            }
            if (all == true) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "所有类型",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }

        /// <summary>
        /// 现金流通类型
        /// </summary>
        /// <returns></returns>
        public static List<MoneySourceType> getMoneySourceType()
        {
            return SiteHelper.GetAPIResult<List<MoneySourceType>>("/Money/GetMoneySourceType");
        }

        /// <summary>
        /// 银行流通类型
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<SelectListItem> BankSourceType(string selected = "", bool all = true)
        {
            var list = new List<SelectListItem>();
            foreach (var type in SiteHelper.GetAPIResult<List<MoneySourceType>>("/Money/GetBankSourceType"))
            {
                list.Add(new SelectListItem()
                {
                    Text = GetSpellCode(type.SourceName.Substring(0, 1)) + type.SourceName,
                    Value = type.Id.ToString(),
                    Selected = type.Id.ToString() == selected
                });
            }
            if (all == true) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "所有类型",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }

        /// <summary>
        /// 游戏
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<SelectListItem> Game(string selected = "", bool all = true)
        {
            var list = new List<SelectListItem>();
            foreach (var game in SiteHelper.GetAPIResult<List<Game>>("/Game/GetGame"))
            {
                if(game.Status == (int)Status.Open)
                {
                    list.Add(new SelectListItem()
                    {
                        Text = game.GameName,
                        Value = game.Id.ToString(),
                        Selected = game.Id.ToString() == selected
                    });
                }
            }
            if (all == true) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "所有游戏",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }

        /// <summary>
        /// 区域
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetAdminAreaList(string selected = "", bool all = true)
        {
            var list = new List<SelectListItem>();
            foreach (var game in SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea"))
            {
                list.Add(new SelectListItem()
                {
                    Text = game.AreaName,
                    Value = game.Id.ToString(),
                    Selected = game.Id.ToString() == selected
                });
            }
            if (all) list.Insert(0, new SelectListItem()
            {
                Value = "0",
                Text = "所有渠道",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }


        public static List<SelectListItem> GetAreaList(string selected = "", bool all = false)
        {
            var list = new List<SelectListItem>();
            foreach (var game in SiteHelper.GetAPIResult<List<AreaAdminUser>>(string.Format("/Area/GetAreaByAdminId?adminID={0}", SiteService.GetAdminUserID())))
            {
                list.Add(new SelectListItem()
                {
                    Text = game.AreaName,
                    Value = game.AreaID.ToString(),
                    Selected = game.AreaID.ToString() == selected
                });
            }
            if (all) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "请选择",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }

        public static List<SelectListItem> GetUserTypeList(string selected = "", bool all = false)
        {
            var list = new List<SelectListItem>();
            foreach (var game in SiteHelper.GetAPIResult<List<UserIdentity>>("/AdminSys/GetUserTypeList"))
            {
                list.Add(new SelectListItem()
                {
                    Text = game.TypeName,
                    Value = game.id.ToString(),
                    Selected = game.id.ToString() == selected
                });
            }
            if (all) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "请选择",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }

        public static List<AreaAdminUser> GetAllArea()
        {
            return SiteHelper.GetAPIResult<List<AreaAdminUser>>(string.Format("/Area/GetAreaByAdminId?adminID={0}", SiteService.GetAdminUserID()));
        }

        public static List<Area> GetArea()
        {
            return SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea");
        }

        public static List<Platform> GetAllPlatform()
        {
            return SiteHelper.GetAPIResult<List<Platform>>("/Area/GetPlatform");
        }

        public static List<Company> GetAllCompany()
        {
            return SiteHelper.GetAPIResult<List<Company>>("/Area/GetCompany");
        }

        public static List<ConvertGateway> GetConvertGateway()
        {
            return SiteHelper.GetAPIResult<List<ConvertGateway>>($"/Pay/GetConvertGateway?operatorID={SiteService.GetAdminUserID()}");
        }

        public static List<PayGateway> GetGateway()
        {
            var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/CDGetPayType?operatorID={SiteService.GetAdminUserID()}");
            var query = (from p in list
                        group p by new { p.GWID, p.GatewayName } into g
                        orderby g.Key.GWID
                        select new PayGateway()
                        {
                            GWID = g.Key.GWID,
                            GatewayName = g.Key.GatewayName
                        }).ToList();
            return query;
        }

        /// <summary>
        /// 游戏房间
        /// </summary>
        /// <returns></returns>
        public static string GameServer()
        {
            var gameServers = SiteHelper.GetAPIResult<List<GameServer>>("/Game/GetGameServer");

            return Newtonsoft.Json.JsonConvert.SerializeObject(
                from server in gameServers
                select new
                {
                    ID = server.Id.ToString(),
                    Name = server.ServerName,
                    GameID = server.GameID.ToString()
                });
        }

        /// <summary> 
        /// 在指定的字符串列表CnStr中检索符合拼音索引字符串 
        /// </summary> 
        /// <param name="CnStr">汉字字符串</param> 
        /// <returns>相对应的汉语拼音首字母串</returns> 
        public static string GetSpellCode(string CnStr)
        {
            string strTemp = "";
            int iLen = CnStr.Length;
            int i = 0;

            for (i = 0; i <= iLen - 1; i++)
            {
                strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
            }

            return strTemp;
        }


        /// <summary> 
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母 
        /// </summary> 
        /// <param name="CnChar">单个汉字</param> 
        /// <returns>单个大写字母</returns> 
        private static string GetCharSpellCode(string CnChar)
        {
            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回 
            if (ZW.Length == 1)
            {
                return CnChar.ToUpper();
            }
            else
            {
                // get the array of byte from the single char 
                int i1 = (short)(ZW[0]);
                int i2 = (short)(ZW[1]);
                iCnChar = i1 * 256 + i2;
            }

            //expresstion 
            //table of the constant list 
            // 'A'; //45217..45252 
            // 'B'; //45253..45760 
            // 'C'; //45761..46317 
            // 'D'; //46318..46825 
            // 'E'; //46826..47009 
            // 'F'; //47010..47296 
            // 'G'; //47297..47613 

            // 'H'; //47614..48118 
            // 'J'; //48119..49061 
            // 'K'; //49062..49323 
            // 'L'; //49324..49895 
            // 'M'; //49896..50370 
            // 'N'; //50371..50613 
            // 'O'; //50614..50621 
            // 'P'; //50622..50905 
            // 'Q'; //50906..51386 

            // 'R'; //51387..51445 
            // 'S'; //51446..52217 
            // 'T'; //52218..52697 
            //没有U,V 
            // 'W'; //52698..52979 
            // 'X'; //52980..53640 
            // 'Y'; //53689..54480 
            // 'Z'; //54481..55289 

            // iCnChar match the constant 
            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {
                return "A";
            }
            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {
                return "B";
            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {
                return "C";
            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {
                return "D";
            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {
                return "E";
            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {
                return "F";
            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {
                return "G";
            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {
                return "H";
            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {
                return "J";
            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {
                return "K";
            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {
                return "L";
            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {
                return "M";
            }

            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {
                return "N";
            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {
                return "O";
            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {
                return "P";
            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {
                return "Q";
            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {
                return "R";
            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {
                return "S";
            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {
                return "T";
            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {
                return "W";
            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {
                return "X";
            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {
                return "Y";
            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {
                return "Z";
            }
            else return ("?");
        }

    }

    /// <summary>
    /// ajax请求结果    
    /// </summary>
    public class AjaxResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}