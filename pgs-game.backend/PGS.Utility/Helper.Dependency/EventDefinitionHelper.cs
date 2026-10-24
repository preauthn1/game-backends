using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using System.Reflection;
using target.Web.Modules.Filter;
using target.Utility.Utility.ThirdParty;
using target.Models.Enums;
using target.Models.ViewModels;

namespace target.Utility.Extensions.Web
{

    public class EventDefinitionHelper
    {

        private static List<EventPrizeTypeEnum> VendorAPIVisible = new List<EventPrizeTypeEnum>();

        private static List<EventPrizeTypeEnum> VendorAPIInvisible = new List<EventPrizeTypeEnum>();


        static EventDefinitionHelper()
        {
            //※　Step 1. 請務必確認新增的 EventPrizeTypeEnum 有在下方定義過
            //※　Step 2. 部分type需要定義在 PrizeTypes_AdminSys_MemoVisiblePrizeTypes
            //※　Step 3. 部分type需要定義在 PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes

            SetVisibleOrNot(false, EventPrizeTypeEnum.Default);
            SetVisibleOrNot(false, EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent);
            SetVisibleOrNot(false, EventPrizeTypeEnum.Ticket_RedEnvelope);
            SetVisibleOrNot(false, EventPrizeTypeEnum.GameVendor_EventBonus);
            SetVisibleOrNot(false, EventPrizeTypeEnum.UpdatePlayerRTP);
            SetVisibleOrNot(false, EventPrizeTypeEnum.ResetPlayerAllGameRTP);
            SetVisibleOrNot(false, EventPrizeTypeEnum.WriteEventCounterAllInOne);
            SetVisibleOrNot(false, EventPrizeTypeEnum.WriteEventCounterByGameID);
            SetVisibleOrNot(false, EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter);
            SetVisibleOrNot(false, EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter);
            SetVisibleOrNot(false, EventPrizeTypeEnum.ExchangeTicketByPoint);
            SetVisibleOrNot(false, EventPrizeTypeEnum.UpdateEventPlayerTag_LuckyDrawRateAdjustment);
            SetVisibleOrNot(false, EventPrizeTypeEnum.EventFinish);

            SetVisibleOrNot(true, EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount);
            SetVisibleOrNot(true, EventPrizeTypeEnum.FishGame_GameBonus);
            SetVisibleOrNot(true, EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting);
            SetVisibleOrNot(true, EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting);

            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_ItemCard);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_CashCard);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_GiftCode);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_BonusMission);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_Ranking);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_Raffle);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_VIP);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_Roulette);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_OtherBonus);
            SetVisibleOrNot(true, EventPrizeTypeEnum.AGG_Freespin);

            //*若沒定義MSTest會檢查報錯
        }


        /// <summary>
        /// 不需要選擇GameIDs的活動模板
        /// </summary>
        public static List<int> EventTemplateTypes_GameIdUnnecessary()
        {
            return new List<int>
                {
                    EventTemplateTypeEnum.LuckyWheelWihtoutPointCounter.ToInteger()
                };
        }


        /// <summary>
        /// PublicAPI(大廳、遊戲頁)...可以獲取的活動
        /// </summary>
        public static List<int> EventTypes_PublicApiVisible()
        {
            return new List<int>
                {
                    EventTypeEnum.BetAmountDrawRedEnvelope.ToInteger()
                };
        }

        /// <summary>
        /// 進遊戲房、需檢查邏輯的活動
        /// </summary>
        public static List<int> EventTypes_WhenEnterGames()
        {
            return new List<int>
                {
                    EventTypeEnum.RTPEvent.ToInteger()
                };
        }

        /// <summary>
        /// 結算階段、需檢查邏輯的活動
        /// </summary>
        public static List<int> EventTypes_WhenSettle()
        {
            return AllEventTypes();
        }


        public static List<int> EventTypes_Quest()
        {
            var targetEnums = ExtensionOfEnum.GetEnumAllValue<EventTypeEnum>()
                .Where(x => x.ToString().Contains("QuestEvent"))
                .Select(x => x.ToInteger())
                .ToList();

            return targetEnums;
        }

        public static List<int> AllEventTypes()
        {
            return ExtensionOfEnum.GetEnumAllValue<EventTypeEnum>()
              .Select(z => z.ToInteger())
              .ToList();
        }

        /// <summary>
        /// 使用到randomDraw邏輯的都算
        /// </summary>
        public static List<int> RandomDrawPrizeTypes()
        {
            return new List<int>
            {
                EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting.ToInteger(),
                EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting.ToInteger()
            };
        }


        /// <summary>
        /// 後台會顯示memo的獎勵類型 (大部分不顯示memo)
        /// </summary>
        public static List<int> PrizeTypes_AGGReport_Page()
        {
            return new List<int>
            {
                (int)EventPrizeTypeEnum.AGG_ItemCard,
                (int)EventPrizeTypeEnum.AGG_Freespin,
                (int)EventPrizeTypeEnum.AGG_GiftCode,
                (int)EventPrizeTypeEnum.AGG_BonusMission,
                (int)EventPrizeTypeEnum.AGG_Ranking,
                (int)EventPrizeTypeEnum.AGG_Raffle,
                (int)EventPrizeTypeEnum.AGG_VIP,
                (int)EventPrizeTypeEnum.AGG_OtherBonus,
            };
        }

        /// <summary>
        /// 後台會顯示memo的獎勵類型 (大部分不顯示memo)
        /// </summary>
        public static List<int> PrizeTypes_AdminSys_MemoVisiblePrizeTypes()
        {
            return new List<int>
            {
                (int)EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent,
                (int)EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting,
                (int)EventPrizeTypeEnum.FishGame_GameBonus,
            };
        }


        /// <summary>
        /// 一般商戶 透過VendorAPI能看到的獎項類型
        /// vendorAPI 原則上是以有amount的為主
        /// </summary>
        public static List<int> PrizeTypes_VendorAPI_VisiblePrizeTypes()
        {
            return VendorAPIVisible.Select(x => x.ToInteger()).ToList();
        }


        /// <summary>
        /// 一般商戶/代理/一般用戶  能看到的獎項類型
        /// </summary>
        public static List<int> PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes()
        {
            var prizeTypes = PrizeTypes_VendorAPI_VisiblePrizeTypes();
            prizeTypes.Add((int)EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent);

            return prizeTypes;
        }

        /// <summary>
        /// 屬於 任務型活動、且是用來識別數值條件的rules
        /// </summary>
        public static List<int> RuleTypes_ValueCompareForQuestEvent()
        {
            return new List<int>
            {
                (int)EventRuleTypeEnum.ValueCompare_TotalPoint_AllInOne_MustLargeThan,
                (int)EventRuleTypeEnum.ValueCompare_TotalPoint_SingleGame_MustLargeThan,
                (int)EventRuleTypeEnum.ValueCompare_TotalGameRound_SingleGame_MustLargeThan,
                (int)EventRuleTypeEnum.ValueCompare_TotalGameRound_AllInOne_MustLargeThan,
            };
        }



        /// <summary>
        /// 根據多重獎項不同 有少數的客製化要調整
        /// </summary>
        public static void SetUIControl_ByMultiplePrizeType(EventMultiplePrizeTypeEnum multipleType, EventDrawPrizeDetailViewModel viewModel)
        {
            //編輯頁面的客製化
            switch (multipleType)
            {
                case EventMultiplePrizeTypeEnum.SendBalanceByBetAmountRate:
                    viewModel.PageControl.IsMaxReferenceValueVisible = true;
                    break;

                case EventMultiplePrizeTypeEnum.SendBalanceWithCash:
                    viewModel.PageControl.AmountColumnDisplayText = "發放現金";
                    break;

                case EventMultiplePrizeTypeEnum.SendBalanceByRank:
                    viewModel.PageControl.AmountColumnDisplayText = "發放金額";
                    viewModel.PageControl.ExtraDescription = "會根據排行順序，依序從排序最前面的獎項開始發放。直到發完";
                    viewModel.PageControl.IsProbabilityWeightsDisable = true;
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 部分類型的prizeType 會依賴多重獎項設定
        /// </summary>
        public static int? GetPrizeTypeMappingMultiplePrizeType(int prizeType)
        {
            var parseEnum = (EventPrizeTypeEnum)prizeType;
            int? mappingType = null;

            switch (parseEnum)
            {
                case EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting:
                    mappingType = EventMultiplePrizeTypeEnum.SendBalanceByBetAmountRate.ToInteger();
                    break;

                case EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount:
                    mappingType = EventMultiplePrizeTypeEnum.SendBalanceByRank.ToInteger();
                    break;

                case EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting:
                    mappingType = EventMultiplePrizeTypeEnum.SendBalanceWithCash.ToInteger();
                    break;

                default:
                    break;
            }

            return mappingType;
        }

        public static bool IsVisibleDefine(EventPrizeTypeEnum prizeType)
        {
            //請參考此檔案最上面 建構式(Construct)會定義每個enum
            if (VendorAPIVisible.Any(x => x == prizeType))
                return true;

            if (VendorAPIInvisible.Any(x => x == prizeType))
                return true;

            return false;
        }


        private static void SetVisibleOrNot(bool isVendorAPIVisible, EventPrizeTypeEnum prizeType)
        {
            if (isVendorAPIVisible)
            {
                VendorAPIVisible.Add(prizeType);
            }
            else
            {
                VendorAPIInvisible.Add(prizeType);
            }
        }


    }






}
