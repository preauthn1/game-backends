using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.Dependency;
using target.Utility.Utility.ThirdParty;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace target.Utility.Extensions.Web
{
    /// <summary>
    /// 懶得用interface去定義的一些 遊戲商差異性
    /// </summary>
    public class GameVendorDefinitionHelper
    {


        private const string GameLogRawDB = "GameLogRawDB";


        private static Dictionary<int, GameVendorDefinitionConfigModel> Dic = new Dictionary<int, GameVendorDefinitionConfigModel>();


        /// <summary>
        /// 如果有新增遊戲商 這邊需要增加定義
        /// </summary>
        static GameVendorDefinitionHelper()
        {
            AddGameVendorConfig(GameVendorTypeEnum.target, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[dbo].[BetRecord]",
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "en-us"),
                    AddSupportLang(LanguageEnum.zh_cn, "zh-CN"),
                },
                IsSupportIframePageBackButton = true,
                IsSupportExternalLinkScreenShot = true,
            });

            AddGameVendorConfig(GameVendorTypeEnum.Funky, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordFunky]",
                VendorCodePrefix = "fun",
                IsSupportExternalLinkScreenShot = true,
                IsSupportCompareStatDBReport = true,
                IsSupportZeroBetAmount = true,
                IsSupportPartitionTable = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "EN_US"),
                    AddSupportLang(LanguageEnum.zh_cn, "ZH_CN"),
                    AddSupportLang(LanguageEnum.ms_my, "MS_MY"),
                },
                DBConnType = GameLogRawDB,
                ServerIPs = new List<string>
                {
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0"
                },
                NotSupportCurrency = new List<string>
                {
                    "SGD"
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.AGG, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordAGG]",
                VendorCodePrefix = "agg",
                IsSupportExternalLinkScreenShot = true,
                IsSupportRTPConfig = true,
                IsSupportPartitionTable = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = true,
                IsSupportCompareStatDBReport = true,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                IMChannelName = "target x AGG Slots (維運問題／線上問題回報)",
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "en-us"),
                    AddSupportLang(LanguageEnum.zh_cn, "zh-cn"),
                    AddSupportLang(LanguageEnum.ms_my, "ms-my"),
                    AddSupportLang(LanguageEnum.th_th, "th-th"),
                    AddSupportLang(LanguageEnum.vi_vn, "vi-vn"),
                },
                ServerIPsText = @"
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0
                    0.0.0.0"                
            });

            AddGameVendorConfig(GameVendorTypeEnum.AGGLive, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordAGGLive]",
                HasTaxAmount = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportExternalLinkScreenShot = true,
                IsSupportCompareStatDBReport = true,
                IsWriteAmountAfterSettle = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "en"),
                    AddSupportLang(LanguageEnum.zh_cn, "zh-cn"),
                },
            });

            AddGameVendorConfig(GameVendorTypeEnum.AGGFish, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordAGGFish]",
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsAutoCreateDataWhenSettleProcess = true,
                IsSupportZeroBetAmount = true,
                IsSupportFishGameDetailRecord = true,
                IsSupportCompareStatDBReport = true,
                IsSuuportSkipModeSingleWalletOnly = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "en-us"),
                    AddSupportLang(LanguageEnum.zh_cn, "zh-cn"),
                },
            });

            AddGameVendorConfig(GameVendorTypeEnum.CrazyFishing, new GameVendorDefinitionConfigModel
            {
                SettlementReportTimezone = 8,
                GameVendorTableName = "[ThirdParty].[BetRecordFish]",
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us, "en-us"),
                    AddSupportLang(LanguageEnum.zh_cn, "zh-cn"),
                },
            });

            AddGameVendorConfig(GameVendorTypeEnum.Jili, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordJili]",
                VendorCodePrefix = "jili",
                IMChannelName = "TH051 Jili 吉利電子(正式環境) [TitanHub/target]",
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsRaiseProcessGameExist = true,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en-US"),
                    AddSupportLang(LanguageEnum.id_id , "id-ID"),
                    AddSupportLang(LanguageEnum.vi_vn , "vi-VN"),
                    AddSupportLang(LanguageEnum.th_th , "th-TH"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-CN"),
                    AddSupportLang(LanguageEnum.zh_tw , "zh-CN"),
                },
                ServerIPs = new List<string>
                {
                    //Staging 
                    "0.0.0.0" ,
                    "0.0.0.0" ,
                    "0.0.0.0",

                    //Production
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0" ,
                    "0.0.0.0" ,
                    "0.0.0.0" ,
                    "0.0.0.0" ,
                    "0.0.0.0",
                    "0.0.0.0",
                },
                NotSupportCurrency = new List<string>
                {
                    "BRL",
                    "AUD",
                    "KES",
                    "EUR",
                    "MYR100",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.GPI, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordGPI]",
                VendorCodePrefix = "gpi",
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = false,
                IsSupportPartitionTable = true,
                IsRaiseProcessGameExist = true,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en-us"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-cn"),
                },
            });

            AddGameVendorConfig(GameVendorTypeEnum.Mario, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordMario]",
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.id_id , "idn"),
                    AddSupportLang(LanguageEnum.vi_vn , "vi"),
                    AddSupportLang(LanguageEnum.th_th , "th"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh"),
                    AddSupportLang(LanguageEnum.zh_tw , "zh"),
                },
                ServerIPs = new List<string>
                {
                    //測試環境
                    "0.0.0.0", 

                    //正式環境
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.YesGame, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordYesGame]",
                HasTaxAmount = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = true,
                IsSupportRTPConfig = true,
                IsSupportExternalLinkScreenShot = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-cn"),
                },
            });


            AddGameVendorConfig(GameVendorTypeEnum.AT, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordAT]",
                DBConnType = GameLogRawDB,
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.th_th , "thai"),
                    AddSupportLang(LanguageEnum.vi_vn , "vn"),
                    AddSupportLang(LanguageEnum.zh_cn , "chs"),
                    AddSupportLang(LanguageEnum.zh_tw , "cht"),
                },
                ServerIPs = new List<string>
                {
                    //測試
                    "0.0.0.0",

                    //正式
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.ATGames, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordATGames]",
                DBConnType = GameLogRawDB,
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = true,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.th_th , "thai"),
                    AddSupportLang(LanguageEnum.vi_vn , "vn"),
                    AddSupportLang(LanguageEnum.zh_cn , "chs"),
                    AddSupportLang(LanguageEnum.zh_tw , "cht"),
                },
                ServerIPs = new List<string>
                {
                    //測試
                    "0.0.0.0",

                    //正式
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.Galaxy, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordGalaxy]",
                VendorCodePrefix = "galaxy",
                IsSupportZeroBetAmount = false,
                IsSupportExternalLinkScreenShot = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = true,
                HasBetAndSettleBundleProcess = false,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "gb"),
                    AddSupportLang(LanguageEnum.vi_vn , "vn"),
                    AddSupportLang(LanguageEnum.th_th , "th"),
                    AddSupportLang(LanguageEnum.ms_my , "my"),
                    AddSupportLang(LanguageEnum.zh_cn , "cn"),
                    AddSupportLang(LanguageEnum.zh_tw , "cn"),
                },
                ServerIPs = new List<string>
                {
                    //測試
                    //"0.0.0.0",

                    ////正式
                    //"0.0.0.0",
                    //"0.0.0.0",
                    //"0.0.0.0",

                    //新品牌
                    "0.0.0.0",
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.AGin, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordAGin]",
                IsSupportZeroBetAmount = false,
                IsSupportExternalLinkScreenShot = false,
                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = false,
                SettlementReportTimezone = -4,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "3"),
                    AddSupportLang(LanguageEnum.id_id , "11"),
                    AddSupportLang(LanguageEnum.th_th , "6"),
                    AddSupportLang(LanguageEnum.vi_vn , "8"),
                    AddSupportLang(LanguageEnum.zh_cn , "1"),
                    AddSupportLang(LanguageEnum.zh_tw , "1"),
                },
                ServerIPs = new List<string>
                {
                    //"0.0.0.0",
                }
            });


            AddGameVendorConfig(GameVendorTypeEnum.ClotPlay, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordClotPlay]",
                IsSupportZeroBetAmount = true,
                IsSupportZeroBetAmountAndStillCallbackPlaceBet = true,
                IsSupportExternalLinkScreenShot = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en-us"),
                    AddSupportLang(LanguageEnum.th_th , "th"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-cn"),
                    AddSupportLang(LanguageEnum.zh_tw , "zh-cn"),
                },
                ServerIPs = new List<string>
                {
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.RG, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordRG]",
                VendorCodePrefix = "rg",
                IsSupportZeroBetAmount = false,
                IsSupportExternalLinkScreenShot = true,
                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = true,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en-us"),
                    AddSupportLang(LanguageEnum.id_id , "id-in"),
                    AddSupportLang(LanguageEnum.vi_vn , "vi-vn"),
                    AddSupportLang(LanguageEnum.th_th , "th-TH"),
                    AddSupportLang(LanguageEnum.ms_my , "bm-my"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-cn"),
                    AddSupportLang(LanguageEnum.zh_tw , "zh-tw"),
                },
                ServerIPs = new List<string>
                {
                    "0.0.0.0",
                    "0.0.0.0"
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.SlotMania, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordSM]",
                VendorCodePrefix = "sm",
                IsSupportZeroBetAmount = false,
                IsSupportExternalLinkScreenShot = true,
                IMChannelName = "VPLUS - target (BWG) (維運問題／線上問題回報)",
                WagerIdHint = "gameRoundId",
                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = false,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.id_id , "ida"),
                    AddSupportLang(LanguageEnum.ms_my , "my"),
                    AddSupportLang(LanguageEnum.vi_vn , "vi"),
                    AddSupportLang(LanguageEnum.th_th , "th"),
                    AddSupportLang(LanguageEnum.zh_cn , "cn"),
                },
                ServerIPs = new List<string>
                {
                    "0.0.0.0"
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.Fuma, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordFuma]",
                VendorCodePrefix = "fuma",
                IMChannelName = "FUMA X BWG (維運問題／線上問題回報)",
                WagerIdHint = "roundId",
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,

                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = false,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.zh_tw , "zhft"),
                    AddSupportLang(LanguageEnum.zh_cn , "zhft"),
                    AddSupportLang(LanguageEnum.vi_vn , "vi"),
                    AddSupportLang(LanguageEnum.th_th , "th"),
                    AddSupportLang(LanguageEnum.id_id , "id"),
                },
                ServerIPs = new List<string>
                {
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                }
            });

            AddGameVendorConfig(GameVendorTypeEnum.EFG, new GameVendorDefinitionConfigModel
            {
                GameVendorTableName = "[ThirdParty].[BetRecordEFG]",
                VendorCodePrefix = "efg",
                IsSupportZeroBetAmount = true,
                IsSupportExternalLinkScreenShot = true,

                IsCheckBetRecordNoByCacheWhenPlaceBet = false,
                IsAllowCrossGameWhenRestrictTokenMode = true,
                IsSupportCompareStatDBReport = true,
                IsSupportPartitionTable = true,
                IsWriteAmountAfterSettle = false,
                HasBetAndSettleBundleProcess = false,
                SettlementReportTimezone = 8,
                DBConnType = GameLogRawDB,
                SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>
                {
                    AddSupportLang(LanguageEnum.en_us , "en"),
                    AddSupportLang(LanguageEnum.zh_tw , "zh-tw"),
                    AddSupportLang(LanguageEnum.zh_cn , "zh-tw"),
                },
                ServerIPs = new List<string>
                {
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                    "0.0.0.0",
                }
            });

        }

        private static GameVendorSupportLanguageModel AddSupportLang(LanguageEnum lang, string langCode)
        {
            return new GameVendorSupportLanguageModel(lang, langCode);
        }

        private static void AddGameVendorConfig(GameVendorTypeEnum gameVendor, GameVendorDefinitionConfigModel config)
        {
            if(config.ServerIPs.IsEmptyOrNull())
            {
                config.ServerIPs = AppHelper.AnalyzeTextFromRawTextBySpaceChar(config.ServerIPsText);
            }

            Dic.Add(gameVendor.ToInteger(), config);
        }


        public static string GetDbConnType(GameVendorTypeEnum vendor)
        {
            var match = GetMatchGameVendorDefineSetting(vendor.ToInteger());

            if (string.IsNullOrWhiteSpace(match.DBConnType))
                return "GameLogDB";

            return match.DBConnType;
        }



        public static bool IsSupportRTPConfig(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.IsSupportRTPConfig;
        }


        public static bool IsSupportRTPConfig(GameVendorTypeEnum vendorID)
        {
            return IsSupportRTPConfig(vendorID.ToInteger());
        }

        public static bool IsSupportIframePageBackButton(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.IsSupportIframePageBackButton;
        }

        /// <summary>
        /// 是否支援 快照/回撥 連結
        /// </summary>
        public static bool IsSupportExternalLinkUrl(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.IsSupportExternalLinkScreenShot;
        }


        public static bool IsAllowCrossGameWhenRestrictTokenMode(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.IsAllowCrossGameWhenRestrictTokenMode;
        }


        public static bool IsSupportPartitionTable(GameVendorTypeEnum vendorID)
        {
            return IsSupportPartitionTable(vendorID.ToInteger());
        }


        public static bool IsSupportPartitionTable(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.IsSupportPartitionTable;
        }


        public static bool IsSupportExternalLinkUrl(GameVendorTypeEnum vendorID)
        {
            return IsSupportExternalLinkUrl(vendorID.ToInteger());
        }

        public static bool HasTaxAmount(GameVendorTypeEnum vendorID)
        {
            return HasTaxAmount(vendorID.ToInteger());
        }

        public static bool HasTaxAmount(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);
            return match.HasTaxAmount;
        }

        /// <summary>
        /// 是否在投注流程中 跳過檢查注單號重複 (效能考量)
        /// </summary>
        public static bool IsCheckBetRecordNoByCacheWhenPlaceBet(GameVendorTypeEnum vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID.ToInteger());
            return match.IsCheckBetRecordNoByCacheWhenPlaceBet;
        }

        public static string GetVendorCodePrefix(GameVendorTypeEnum vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID, isThrowException: true);

            var text = match.VendorCodePrefix;

            if (string.IsNullOrWhiteSpace(text))
                return (vendorID).ToString().ToCamelCase();

            return text;
        }


        public static string GetBetRecordTableName(GameVendorTypeEnum vendorID)
        {
            return GetBetRecordTableName(vendorID.ToInteger());
        }

        public static string GetBetRecordTableName(int vendorID)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID, isThrowException: true);
            return match.GameVendorTableName;
        }

        public static string ConvertToGameVendorSupportLanguageCode(int vendorID, LanguageEnum myLang)
        {
            var match = GetMatchGameVendorDefineSetting(vendorID);

            var gameSuuportLangs = match.SupportLanguagesV2;

            //優先1: 完全符合的語言代碼
            var matchLang = gameSuuportLangs.FirstOrDefault(x => x.Language == myLang);

            if (matchLang != null)
            {
                return matchLang.LanguageCode;
            }

            //如果只支持簡中或繁中其中一種
            if (myLang == LanguageEnum.zh_cn || myLang == LanguageEnum.zh_tw)
            {
                var matchCN = gameSuuportLangs.FirstOrDefault(x => x.Language == LanguageEnum.zh_cn || x.Language == LanguageEnum.zh_tw);

                if (matchCN != null)
                {
                    return matchCN.LanguageCode;
                }
            }

            var matchEN = gameSuuportLangs.FirstOrDefault(x => x.Language == LanguageEnum.en_us);

            if (matchEN != null)
            {
                return matchEN.LanguageCode;
            }

            if (gameSuuportLangs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("gameSuuportLangs"));
            }

            return gameSuuportLangs.FirstOrDefault().LanguageCode;
        }



        public static int ConverSlot88_To_PGS(int vendorID)
        {
            if (vendorID == GameVendorTypeEnum.Slot88.ToInteger())
                return GameVendorTypeEnum.target.ToInteger();

            return vendorID;
        }



        public static GameVendorDefinitionConfigModel GetMatchGameVendorDefineSetting(GameVendorTypeEnum vendorID, bool isThrowException = false)
        {
            return GetMatchGameVendorDefineSetting(vendorID.ToInteger(), isThrowException);
        }

        public static GameVendorDefinitionConfigModel GetMatchGameVendorDefineSetting(int vendorID, bool isThrowException = false)
        {
            vendorID = ConverSlot88_To_PGS(vendorID);

            if (Dic.ContainsKey(vendorID))
                return Dic[vendorID];

            //如果有指定要拋錯誤
            if (isThrowException)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue, new ErrorDetailModel($"{vendorID} GameVendorDefinitionHelper 尚未定義"));
            }

            //返回預設
            return new GameVendorDefinitionConfigModel
            {

            };
        }

    }


    /// <summary>
    /// 定義 每款遊戲商 不同的檢查機制差異
    /// </summary>
    public class GameVendorDefinitionConfigModel
    {
        public GameVendorDefinitionConfigModel()
        {
            this.SupportLanguagesV2 = new List<GameVendorSupportLanguageModel>();
        }

        public string GameVendorTableName { get; set; }


        /// <summary>
        /// 投注API 會採用短時間的cache來處理 (不透過查詢DB)
        /// 對效能應該會有很大幫助、建議熱門的老虎機遊戲要設為true (AGG以外的也支援)
        /// </summary>
        public bool IsCheckBetRecordNoByCacheWhenPlaceBet { get; set; }


        /// <summary>
        /// 是否支援快照/回播 紀錄 API
        /// </summary>
        public bool IsSupportExternalLinkScreenShot { get; set; }


        /// <summary>
        /// 是否支援調整RTP
        /// </summary>
        public bool IsSupportRTPConfig { get; set; }


        public bool IsSupportZeroBetAmount { get; set; }


        /// <summary>
        /// 即使0元注單。也要callback placeBet
        /// </summary>
        public bool IsSupportZeroBetAmountAndStillCallbackPlaceBet { get; set; }


        public bool IsSupportFishGameDetailRecord { get; set; }


        public bool IsSupportPartitionTable { get; set; }


        public bool IsSupportCompareStatDBReport { get; set; }


        public bool IsSuuportSkipModeSingleWalletOnly { get; set; }

        /// <summary>
        /// 帳單統計以哪個時區為基準 (會影響三方帳務比對這頁的 聚合方式)
        /// </summary>
        public double SettlementReportTimezone { get; set; }


        /// <summary>
        /// 是否有加注流程(同一筆注單號)的遊戲, 請確認這是做什麼在設為true
        /// </summary>
        public bool IsRaiseProcessGameExist { get; set; }


        /// <summary>
        /// 是否有 投注、結算二合一的流程
        /// </summary>
        public bool HasBetAndSettleBundleProcess { get; set; }


        /// <summary>
        /// 是否紀錄結算後的金幣 (ps. update會多少影響效能)
        /// </summary>
        public bool IsWriteAmountAfterSettle { get; set; }

        /// <summary>
        /// 如果結算查無注單 必須想辦法補單
        /// </summary>
        public bool IsAutoCreateDataWhenSettleProcess { get; set; }

        /// <summary>
        /// 僅對內部組織的人 開放某些實驗性功能
        /// </summary>
        public bool IsSupportExperimentFeatureForInternalUsers { get; set; }


        /// <summary>
        /// 嚴格token模式下，允許同一個遊戲商的遊戲自由進出 (如果該遊戲商有自訂大廳 會需要使用這種)
        /// </summary>
        public bool IsAllowCrossGameWhenRestrictTokenMode { get; set; }

        /// <summary>
        /// 在iframe模式下，使用自製返回大廳按鈕
        /// </summary>
        public bool IsSupportIframePageBackButton { get; set; }


        public bool HasTaxAmount { get; set; }


        /// <summary>
        /// 參考 DBConnType
        /// </summary>
        public string DBConnType { get; set; }


        /// <summary>
        /// 影響Gamelist寫入的 遊戲代碼
        /// </summary>
        public string VendorCodePrefix { get; set; }


        public List<string> ServerIPs { get; set; }

        public string ServerIPsText { get; set; }

        public string IMChannelName { get; set; }

        public string WagerIdHint { get; set; }

        public List<GameVendorSupportLanguageModel> SupportLanguagesV2 { get; set; }

        //public List<LanguageEnum> SupportLanguages { get; set; }

        public List<string> NotSupportCurrency { get; set; }


    }



    public class GameVendorSupportLanguageModel
    {
        public GameVendorSupportLanguageModel()
        {

        }

        public GameVendorSupportLanguageModel(LanguageEnum lang, string langCode)
        {
            this.Language = lang;
            this.LanguageCode = langCode;
        }

        public LanguageEnum Language { get; set; }

        public string LanguageCode { get; set; }

    }









}
