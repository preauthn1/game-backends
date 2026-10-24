using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
namespace game.AdminSystem
{
    public static class ConfigData
    {
        /// <summary>
        /// 上传目录
        /// </summary>
        public static string UploadFolder
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["uploadFolder"].ToString();
            }
        }

        /// <summary>
        /// 上传文件基地址
        /// </summary>
        public static string UploadFileBaseUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["uploadFileBaseUrl"].ToString();
            }
        }


        /// <summary>
        /// 网域域名
        /// </summary>
        public static string WebSiteHost
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["webSiteHost"].ToString();
            }
        }

        /// <summary>
        /// 金币显示比例
        /// </summary>
        public static Decimal CoinShowRate
        {
            get
            {
                return Convert.ToDecimal(System.Configuration.ConfigurationManager.AppSettings["CoinShowRate"]);
            }
        }

        /// <summary>
        /// 可操作房间桌子的游戏ID
        /// </summary>
        public static int[] OperateServerGameIDs
        {
            get
            {
                var _strResult = System.Configuration.ConfigurationManager.AppSettings["OperateServerGameID"].Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                return _strResult.Split(',').Select(m => Convert.ToInt32(m.Split('@')[0])).ToArray();
            }
        }

        public static Dictionary<int, string> DicOperateServerGameIDs
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["OperateServerGameID"].Split(',').ToDictionary(m => Convert.ToInt32(m.Split('@')[0]), m => m.Split('@')[1]);
            }
        }

        public static string ProtoBufMD5Key
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["ProtoBufMD5Key"];
            }
        }
    }
}