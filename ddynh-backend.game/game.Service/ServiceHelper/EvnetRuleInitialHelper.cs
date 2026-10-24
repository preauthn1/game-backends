using game.Entity;
using game.Entity.Area;
using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions;

namespace game.Infrastructure.Helper
{
    public class EvnetRuleInitialHelper
    {

        /// <summary>
        /// 此處要定義每一種 ApplyEventTypeEnum 預設建立的項目
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CreateEventRuleInitailModel InitialByEventType(int eventSettingID, ApplyEventTypeEnum type)
        {
            switch (type)
            {
                case ApplyEventTypeEnum.SeventDaysBonus:
                    return Initail_7Days_Bonus(eventSettingID);

                case ApplyEventTypeEnum.FirstChargeBonus:
                    return Initail_FirstCharge_Bonus(eventSettingID);

                case ApplyEventTypeEnum.Roulette_FlowAmount:
                case ApplyEventTypeEnum.Roulette_Recharge:
                    return Initail_Recharge_Roulette(eventSettingID);

                case ApplyEventTypeEnum.BindBankCard:
                    return Initail_BindCard_Bonus(eventSettingID);

                case ApplyEventTypeEnum.BindAlipay:
                    return Initail_BindAlipay_Bonus(eventSettingID);

                default:
                    throw new Exception("若新增一种 ApplyEventTypeEnum 你需要定义好此项目");
            }
        }


        private static CreateEventRuleInitailModel Initail_7Days_Bonus(int eventSettingID)
        {
            var listBonus = new List<EvnetSettingBonusEntity>();

            for (int i = 1; i <= 7; i++)
            {
                listBonus.Add(new EvnetSettingBonusEntity
                {
                    EventSettingID = eventSettingID,
                    BonusType = EventBonusTypeEnum.CashMoney.ToInteger(),
                    Sequence = i,
                    Value = 0,
                    FlowRate = 1,
                });
            }

            var rules = new List<EventSettingRuleItemEntity>
                {
                    new EventSettingRuleItemEntity
                    {
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.TotalDepositAmount.ToInteger(),
                        RuleValue = 0,
                    },
                    new EventSettingRuleItemEntity
                    {
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.Login_BonusReset.ToInteger(),
                        RuleValue = 0,
                    }
                };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                BonuItems = listBonus,
                RuleItems = rules
            };
        }



        private static CreateEventRuleInitailModel Initail_BindCard_Bonus(int eventSettingID)
        {
            var listBonus = new List<EvnetSettingBonusEntity>();
            listBonus.Add(new EvnetSettingBonusEntity
            {
                EventSettingID = eventSettingID,
                BonusType = EventBonusTypeEnum.CashMoney.ToInteger(),
                Sequence = 1,
                Value = 0,
                FlowRate = 1,
            });

            var rules = new List<EventSettingRuleItemEntity>
                {
                    new EventSettingRuleItemEntity
                    {
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.Users_IsBindBankAccount.ToInteger(),
                        RuleValue = 1,
                    }
                };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                BonuItems = listBonus,
                RuleItems = rules
            };
        }



        private static CreateEventRuleInitailModel Initail_BindAlipay_Bonus(int eventSettingID)
        {
            var listBonus = new List<EvnetSettingBonusEntity>();
            listBonus.Add(new EvnetSettingBonusEntity
            {
                EventSettingID = eventSettingID,
                BonusType = EventBonusTypeEnum.CashMoney.ToInteger(),
                Sequence = 1,
                Value = 0,
                FlowRate = 1,
            });

            var rules = new List<EventSettingRuleItemEntity>
                {
                    new EventSettingRuleItemEntity
                    {
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.Users_IsBindAlipayAccount.ToInteger(),
                        RuleValue = 1,
                    }
                };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                BonuItems = listBonus,
                RuleItems = rules
            };
        }



        private static CreateEventRuleInitailModel Initail_FirstCharge_Bonus(int eventSettingID)
        {
            var listBonus = new List<EvnetSettingBonusEntity>();
            int bonusGroup = 1;

            //需求有調整過，原本允許設定三組。改成一組

            for (int i = 1; i <= 1; i++)
            {
                listBonus.Add(new EvnetSettingBonusEntity
                {
                    BonusGroup = bonusGroup++,
                    EventSettingID = eventSettingID,
                    BonusType = EventBonusTypeEnum.PercentageFromChargeAmount.ToInteger(),
                    Sequence = i,
                    Value = 0.1M,
                    FlowRate = 1,
                });
            }

            var rules = new List<EventSettingRuleItemEntity>
                {
                    new EventSettingRuleItemEntity
                    {
                        BonusGroup = 1,
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.DepositAmount_FirstDeposit.ToInteger(),
                        RuleValue = 100 * SystemConst.CashAmountRate,
                    }
                };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                BonuItems = listBonus,
                RuleItems = rules
            };
        }

        private static CreateEventRuleInitailModel Initail_Recharge_Roulette(int eventSettingID)
        {
            var awards = new List<EventSettingAwardItemEntity>
            {
                new EventSettingAwardItemEntity {
                    EventSettingID = eventSettingID,
                    AwardName = "金币",
                    ImageUrlDomain = "",
                    ImageUrl = "",
                    AwardType = EventAwardTypeEnum.Cash.ToInteger(),
                    AwardAmount = 0,
                    AwardRate = 1,
                    AwardLimit = 0,
                }
            };
            var lotteryConditionsItem = new List<EventSettingLotteryConditionsItemEntity>
            {
                new EventSettingLotteryConditionsItemEntity {
                    EventSettingID = eventSettingID,
                    LotteryAmount = 0,
                    LotteryCount = 0
                }
            };

            var rules = new List<EventSettingRuleItemEntity>
                {
                    new EventSettingRuleItemEntity
                    {
                        EventSettingID = eventSettingID,
                        RuleType = EventRuleTypeEnum.LuckyRoulette_CumulativeTopUpOfTheDay.ToInteger(),
                        RuleValue = 1,
                    }
                };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                RuleItems = rules,
                AwardItems = awards,
                LotteryConditionsItem = lotteryConditionsItem
            };
        }

        private static CreateEventRuleInitailModel Initail_Lottery_Conditions(int eventSettingID)
        {
            var lotteryConditionsItem = new List<EventSettingLotteryConditionsItemEntity>
            {
                new EventSettingLotteryConditionsItemEntity {
                    EventSettingID = eventSettingID,
                    LotteryAmount = 0,
                    LotteryCount = 0
                }
            };

            return new CreateEventRuleInitailModel
            {
                EventSettingID = eventSettingID,
                LotteryConditionsItem = lotteryConditionsItem
            };
        }


    }
}
