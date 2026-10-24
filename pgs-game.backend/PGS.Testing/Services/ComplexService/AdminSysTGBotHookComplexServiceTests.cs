using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TG_Hook_API.Models;

namespace target.Testing
{
    [TestClass]
    public class AdminSysTGBotHookComplexServiceTests : BaseTest
    {

        public IAdminSysTGBotHookComplexService ServiceInstance { get; set; }

        public IMerchantService MerchantService { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }


        private static string defaultChatID = "-576451364";  //chatID: -576451364 target-Notify-CommonError 這個頻道


        [TestMethod]
        public void LocalTest_WebHookScenario_Test()
        {
            if (IsNotDeveloperComputerName())
                return;

            //如果需要臨時測試 請調整日期 (建議填入明天的日期)
            if (IsSkipTest_WhenLargeThanTargetDate(new DateTime(2026, 1, 1)))
                return;

            string mySendMessage = $"/round 6239473 ";

            ServiceInstance.CheckWebHookActionAndExecute_TG(new TgWebhookParsingResult
            {
                IsForceNotifyWhenMSTest = true,
                ParsingModel = new TgWebhookRequest
                {
                    message = new TGMessage
                    {
                        text = mySendMessage,
                        chat = new Chat
                        {
                            id = defaultChatID,
                        },
                        from = new From
                        {
                            id = "5625326732" //這個是發送者ID
                        },
                        reply_to_message = new TGReplyTo
                        {

                        }
                    }
                }
            });

            return;
        }


        [TestMethod]
        public void GetWagerServerInfo_Test()
        {
            var newServiceInstance = new AdminSysTGBotHookComplexService();

            var queryPerGame = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 7),
                PageSize = 5
            })
            .Items
            .FirstOrDefault();

            if (queryPerGame != null)
            {
                var myResult = newServiceInstance.GetWagerServerInfo(queryPerGame.TransactionNo, "-123456", IMChannelTypeEnum.TG);

                if (string.IsNullOrWhiteSpace(myResult.ReplyMessage))
                {
                    Assert.Fail($"單號: {queryPerGame.TransactionNo} 結果不如預期");
                }
            }

            return;
        }




        [TestMethod]
        public void CheckTGWebHookActionAndExecute_Test()
        {
            string channeName = "MSTest_TG_hook_PGS_" + DateTimeHelper.GetCurrentTimeYYYYMMDD();
            string existChatID = "-12345678";

            ServiceInstance.CheckWebHookActionAndExecute_TG(new TgWebhookParsingResult
            {
                ParsingModel = new TgWebhookRequest
                {
                    message = new TGMessage
                    {
                        new_chat_title = channeName,
                        chat = new Chat
                        {
                            id = existChatID,
                        }
                    }
                }
            });

            return;
        }



        [TestMethod]
        public void WebHookScenario_CreateNewMerchant_Test()
        {
            var newName = "TGHook_NEW_" + DateTimeHelper.GetCurrentTimeYYYYMMDD(8);
            string mySendMessage = $"/key_CN {newName},sw";

            var isExist1 = MerchantService.IsMerchantNameExist(newName, true).IsExist;
            var isExist2 = MerchantService.IsMerchantNameExist("(測試用) " + newName, true).IsExist;

            if (isExist1 || isExist2)
                return;

            ServiceInstance.CheckWebHookActionAndExecute_TG(new TgWebhookParsingResult
            {
                ParsingModel = new TgWebhookRequest
                {
                    message = new TGMessage
                    {
                        text = mySendMessage,
                        chat = new Chat
                        {
                            id = defaultChatID,
                        },
                        from = new From
                        {
                            id = "5625326732"
                        },
                        reply_to_message = new TGReplyTo
                        {

                        }
                    }
                }
            });

            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                Keyword = newName,
                IsIncludeDeleteData = false
            })
                .FirstOrDefault();

            if (queryMerchant != null)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, queryMerchant.MerchantID);
            }

            return;
        }


        [TestMethod]
        public void WebHookScenario_CommonMsgCommand_Test()
        {
            string mySendMessage = $"/400";

            ServiceInstance.CheckWebHookActionAndExecute_TG(new TgWebhookParsingResult
            {
                ParsingModel = new TgWebhookRequest
                {
                    message = new TGMessage
                    {
                        text = mySendMessage,
                        chat = new Chat
                        {
                            id = defaultChatID,
                        },
                        from = new From
                        {
                            id = "5625326732"
                        },
                        reply_to_message = new TGReplyTo
                        {

                        }
                    }
                }
            });

            return;
        }



        [TestMethod]
        public void WebHookScenario_CreateNewChannel_Skype_Test()
        {
            var newName = "Hook_Sykpe_New_" + DateTimeHelper.GetCurrentTimeYYYYMMDD(8);
            string mySendMessage = $"@pgs_it_bot_01 {newName}";

            var queryExist = IMChannelRepository.GetIMChannels();

            var matchData = queryExist
                .Where(x => x.DisplayName == newName)
                .FirstOrDefault();

            if (matchData != null)
                return;

            ServiceInstance.CheckWebHookActionAndExecute_Skype(new SkypeWebHookRequest
            {
                conversation = new SkypeConversation
                {
                    id = "MSTest_00112233"
                },
                from = new SkypeFrom
                {
                    id = SystemConstant.Skype_WebHook_WhiteList_UserIDs.FirstOrDefault()
                },
                text = mySendMessage,
            });

            return;
        }



        [TestMethod]
        public void CheckTGWebHookActionAndExecute_NewGroupCreated_Test()
        {
            string channeName = "MSTest_TG_NewCreate_PGS_009";

            ServiceInstance.CheckWebHookActionAndExecute_TG(new TgWebhookParsingResult
            {
                ParsingModel = new TgWebhookRequest
                {
                    message = new TGMessage
                    {
                        group_chat_created = true,
                        chat = new Chat
                        {
                            id = "-111222333",
                            Title = channeName
                        }
                    }
                }
            });

            return;
        }


        [TestMethod]
        public void CheckSkypeWebHookActionAndExecute_Test()
        {
            string channeName = "MSTest_hook_PGS_001";

            ServiceInstance.CheckWebHookActionAndExecute_Skype(new TG_Hook_API.Models.SkypeWebHookRequest
            {
                from = new SkypeFrom
                {
                    id = SystemConstant.Skype_WebHook_WhiteList_UserIDs.FirstOrDefault(),
                },
                text = $"@pgs_it_bot_01 {channeName}",
                conversation = new TG_Hook_API.Models.SkypeConversation
                {
                    id = "AAABBBCCCDDDEEE",
                }
            });

            return;
        }

    }
}

