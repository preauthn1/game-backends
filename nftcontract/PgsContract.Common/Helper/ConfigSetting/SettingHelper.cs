using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using target.Models;

namespace target.Utility
{
    public static class SettingHelper
    {


        public static bool IsDev
        {
            get
            {
                return Env == SystemConstant.Env_Name_Development || Env == SystemConstant.Env_Name_Dev;
            }
        }


        public static bool IsCreateRoundJobEnable
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("IsCreateRoundJobEnable", false);
            }
        }




        public static string Contract_ProviderUrl
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("Contract_ProviderUrl");
            }
        }

        public static int Contract_ProviderChainID
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<int>("Contract_ProviderChainID");
            }
        }



        public static string ContractAddress_SlotGame
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("ContractAddress_SlotGame");
            }
        }


        public static string ContractAddress_DragonTiger
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("ContractAddress_DragonTiger");
            }
        }


        public static string ContractAddress_PgsToken
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("ContractAddress_PgsToken");
            }
        }


        public static string ContractAddress_NFT
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("ContractAddress_NFT");
            }
        }


        public static string ContractAddress_NFTDragonTiger
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrException<string>("ContractAddress_NFTDragonTiger");
            }
        }


        /// <summary>
        /// 會強轉小寫
        /// </summary>
        public static string Env
        {
            get
            {
                return (BaseSettingHelper.GetSettingValue("Env") ?? "").ToLower();
            }
        }


        public static bool IsProduction
        {
            get
            {
                return Env == SystemConstant.Env_Name_Production;
            }
        }


        public static string WebHostListenPort
        {
            get
            {
                return BaseSettingHelper.GetSettingValue("WebHostListenPort");
            }
        }


        public static SlackNotifySetting SlackNotify => new SlackNotifySetting();

        public static ThirdPartySolutionSetting ThirdParty => new ThirdPartySolutionSetting();

    }



    public class ThirdPartySolutionSetting
    {

        private static string Prefix = "ThirdParty:";


        public string RedisConn
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "RedisConn", "");
            }
        }

        /// <summary>
        /// 某些環境會共用redis，然後會用 DB編號區別開來
        /// </summary>
        public int RedisDBNumber
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>(Prefix + "RedisDBNumber", 0);
            }
        }

    }


    public class SlackNotifySetting
    {
        private static string Prefix = "SlackNotify:";


        /// <summary>
        /// 當跳出一般C# exception的時候發通知
        /// </summary>
        public bool IsNotifyEnable_When_SystemException
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>(Prefix + "IsNotifyEnable_When_SystemException", true);
            }
        }


        public string NotifyChannel_When_SystemException
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "NotifyChannel_When_SystemException", "n_error_game_api");
            }
        }

    }





}
