using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models;
using example.invalid;
using TG_Hook_API.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Collections;

namespace target.BL.Services
{
    public class AdminSysTGBotHookComplexService : BaseAdminSysService, IAdminSysTGBotHookComplexService
    {

        public IIMChannelService IMChannelService { get; set; }

        public IMerchantService MerchantService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }

        public IBetRecordService BetRecordService { get; set; }

        public IThirdPartyGameVendorService ThirdPartyGameVendorService { get; set; }



        public IMChannelRepository IMChannelRepository { get; set; }

        public IMChannelCustomizedSettingRepository IMChannelCustomizedSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        private static string NEW_LINE = Environment.NewLine;


        private bool IsSendMarkdownFormat = false;

        private bool IsPGSNotifyErrorChannel = false;

        private List<LanguageEnum> PreferLanguages = new List<LanguageEnum>();

        private static List<string> botUserNames = new List<string>
        {
            "pgs_it_bot",
            "bwg_notify_bot",
        };


        /// <summary>
        /// 根據裡面的參數不同 決定走向不同的處理邏輯
        /// </summary>
        public void CheckWebHookActionAndExecute_TG(TgWebhookParsingResult parse)
        {
            var messageInfo = parse.ParsingModel.message;

            bool isInternalUser = IsInternalUser(messageInfo);

            IsPGSNotifyErrorChannel = CheckIsPGSNotifyErrorChannel(parse);

            //發送super group通知
            SendSuperGroupWarning(parse);

            if (!string.IsNullOrWhiteSpace(messageInfo.new_chat_title))
            {
                RenameChannelName(new TgRenameChannelTitleParameter
                {
                    NewTitleName = messageInfo.new_chat_title,
                    Chat_ID = messageInfo.chat?.id,
                });
            }
            else if (messageInfo.new_chat_participant != null &&
                     messageInfo.new_chat_participant.is_bot &&
                     botUserNames.Any(name => name == messageInfo.new_chat_participant.username))
            {
                CreateNewChannelTG(new TgNewChannelJoinParameter
                {
                    ChannelName = messageInfo.chat?.Title,
                    Chat_ID = messageInfo.chat?.id,
                    IsFromInternalUser = isInternalUser
                });
            }
            else if (messageInfo.group_chat_created)
            {
                CreateNewChannelTG(new TgNewChannelJoinParameter
                {
                    ChannelName = messageInfo.chat?.Title,
                    Chat_ID = messageInfo.chat?.id,
                    IsFromInternalUser = isInternalUser
                });
            }
            else
            {
                string myText = messageInfo.text ?? "";
                string myChatID = messageInfo.chat?.id;

                if (myText.ContainsWihtoutCase("/my_id"))
                {
                    //允許外部人員
                    string myMsg = $"發送者: {messageInfo.from?.id} ({messageInfo.from?.username}), 來自頻道:{myChatID} ({messageInfo.chat?.Title})";
                    SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_common_error);
                }
                else if (isInternalUser)
                {
                    if (myText.ContainsWihtoutCase("/trigger_notify"))
                    {
                        string myMsg = $"已觸發TG通知 chatID: {messageInfo.chat?.id}";
                        SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
                    }
                    else
                    {
                        IMMessageGenerateResult reply = null;

                        var myIMChannelType = IMChannelTypeEnum.TG;

                        PreferLanguages = IMChannelCustomizedSettingRepository.GetIMChannelPreferLanguages(myChatID, myIMChannelType);

                        if (myText.ContainsWihtoutCase("/key_en") || myText.ContainsWihtoutCase("/keyen"))
                        {
                            reply = CopyCreateNewMerchantAndGenerateMessage(myText, messageInfo.chat?.id, myIMChannelType, isCN: false);
                        }
                        else if (myText.ContainsWihtoutCase("/key_cn") || myText.ContainsWihtoutCase("/keycn"))
                        {
                            reply = CopyCreateNewMerchantAndGenerateMessage(myText, messageInfo.chat?.id, myIMChannelType, isCN: true);
                        }
                        else if (myText.StartWihtoutCase("/wager") || myText.StartWihtoutCase("/wg"))
                        {
                            reply = GetWagerServerInfo(myText, myChatID, myIMChannelType);
                        }
                        else if (myText.StartWihtoutCase("/round"))
                        {
                            reply = GetWagerByRoundID(myText, myChatID, myIMChannelType);
                        }
                        else if (myText.ContainsWihtoutCase("/apis") || myText.ContainsWihtoutCase("/apilist") || myText.ContainsWihtoutCase("/api_list"))
                        {
                            reply = GetAllApiList(myText, myChatID, IMChannelTypeEnum.TG);
                        }
                        else
                        {
                            reply = GenerateReplyMessageByKeywordType(myText, myIMChannelType, myChatID);
                        }

                        if (reply.ReplyMessage.IsNotEmptyTextNotNull() && myChatID.IsNotEmptyTextNotNull())
                        {
                            //如果有發送file先獨立發送
                            if (reply.FileName.IsNotEmptyTextNotNull())
                            {
                                MyTelegramUtility.SendMessage(new TelegramMessageParameter
                                {
                                    Message = "EMPTY",
                                    TelegramExtraSetting = new TelegramExtraSetting
                                    {
                                        PostFileBytes = reply.FileBytes,
                                        PostFileName = reply.FileName,
                                        ParseMode = "HTML",
                                        IsCleanPrefixText = true,
                                        TargetChatID = myChatID
                                    }
                                });
                            }

                            MyTelegramUtility.SendMessage(new TelegramMessageParameter
                            {
                                Message = reply.ReplyMessage,
                                TelegramExtraSetting = new TelegramExtraSetting
                                {
                                    Reply_to_MessageID = parse.ParsingModel.message?.reply_to_message?.message_id,
                                    ParseMode = IsSendMarkdownFormat ? "Markdown" : "HTML",
                                    IsSkipWhenMSTest = parse.IsForceNotifyWhenMSTest ? false : true,
                                    IsCleanPrefixText = true,
                                    TargetChatID = myChatID
                                }
                            });
                        }
                    }
                }

            }
        }



        /// <summary>
        /// 根據裡面的參數不同 決定走向不同的處理邏輯
        /// </summary>
        public void CheckWebHookActionAndExecute_Skype(SkypeWebHookRequest parse)
        {
            string analyzeText = parse.text ?? "";

            if (analyzeText.ContainsWihtoutCase("/my_id"))
            {
                string myMsg = $"發送者: {parse.from?.id} (來自Skype訊息) , 頻道: {parse?.conversation?.id} ";
                SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_common_error);
            }
            else if (IsInternalUserOfSkype(parse))
            {
                List<string> replaceTextList = new List<string>();

                foreach (var name in botUserNames)
                {
                    replaceTextList.Add(name);
                    replaceTextList.Add("@" + name);
                }

                foreach (var text in replaceTextList)
                {
                    analyzeText = analyzeText.Replace(text, "");
                }

                if (string.IsNullOrWhiteSpace(analyzeText))
                    return;

                if (string.IsNullOrWhiteSpace(analyzeText.Trim()))
                    return;

                if (analyzeText.ContainsWihtoutCase("trigger_noti"))
                {
                    string myMsg = $"已觸發Skype通知 chatID: {parse.conversation?.id}";
                    SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
                }
                else
                {
                    string myChatID = parse.conversation?.id;

                    var myIMChannelType = IMChannelTypeEnum.Skype;

                    PreferLanguages = IMChannelCustomizedSettingRepository.GetIMChannelPreferLanguages(myChatID, myIMChannelType);

                    IMMessageGenerateResult reply = null;

                    if (analyzeText.ContainsWihtoutCase("/key_en"))
                    {
                        reply = CopyCreateNewMerchantAndGenerateMessage(analyzeText, myChatID, myIMChannelType, isCN: false);
                    }
                    else if (analyzeText.ContainsWihtoutCase("/key_cn"))
                    {
                        reply = CopyCreateNewMerchantAndGenerateMessage(analyzeText, myChatID, myIMChannelType, isCN: true);
                    }
                    else if (analyzeText.ContainsWihtoutCase("/wager"))
                    {
                        reply = GetWagerServerInfo(analyzeText, myChatID, myIMChannelType);
                    }
                    else if (analyzeText.ContainsWihtoutCase("/apis") || analyzeText.ContainsWihtoutCase("/apilist") || analyzeText.ContainsWihtoutCase("/api_list"))
                    {
                        reply = GetAllApiList(analyzeText, myChatID, IMChannelTypeEnum.TG);
                    }
                    else
                    {
                        //一些靜態文案
                        reply = GenerateReplyMessageByKeywordType(analyzeText, myIMChannelType, myChatID);
                    }

                    if (reply.ReplyMessage.IsNotEmptyTextNotNull() && myChatID.IsNotEmptyTextNotNull())
                    {
                        reply.ReplyMessage = reply.ReplyMessage.Replace("--------------------------------------------------", "–--–--–--–--–--–--–--–--–--–--–--–--–--");
                        reply.ReplyMessage = reply.ReplyMessage.Replace("- ", "－");

                        MySkypeUtility.SendMessage(new SkypeMessageParameter
                        {
                            Message = reply.ReplyMessage,
                            TargetConversationID = myChatID,
                            ExtraSetting = new SkypeExtraSetting
                            {

                            }
                        });
                    }
                    else
                    {
                        //其他關鍵字 視為新頻道創建
                        analyzeText = analyzeText.TruncateFirstWordIfMatch(' ');

                        //誤打指令 可能會是斜線符號 
                        if (analyzeText.StartsWith('/'))
                        {
                            return;
                        }

                        if (analyzeText.Contains("全部") && analyzeText.Count() < 5)
                        {
                            return;
                        }

                        CreateNewChannelSkype(new TgNewChannelJoinParameter
                        {
                            ChannelName = analyzeText,
                            Chat_ID = myChatID,
                        });
                    }
                }
            }
        }


        public void RenameChannelName(TgRenameChannelTitleParameter model)
        {
            model.RuleFor(x => x.Chat_ID).Rule_NotEmptyText();
            model.RuleFor(x => x.NewTitleName).Rule_NotEmptyText();

            var queryExistChannel = IMChannelRepository.GetByChatID(model.Chat_ID, IMChannelTypeEnum.TG);

            if (queryExistChannel == null)
                return;

            int defaultUserID = InstanceBOUserID();

            var queryViewModel = IMChannelService.GetByPagingFilter(defaultUserID, new IMChannelRequestParameterModel
            {
                IMChannelIDs = queryExistChannel.IMChannelID.ToListCollection(),
            })
                .Items
                .FirstOrDefault();

            IMChannelService.Update(defaultUserID, new IMChannelUpdateRequestModel
            {
                ChannelName = model.NewTitleName,
                IMChannelID = queryViewModel.IMChannelID,
                Memo = queryViewModel.InternalMemo,
                ReferIPAddressTemplateID = queryViewModel.ReferIPTemplateID,
            });
        }


        public void CreateNewChannelTG(TgNewChannelJoinParameter model)
        {
            model.RuleFor(x => x.Chat_ID).Rule_NotEmptyText();
            model.RuleFor(x => x.ChannelName).Rule_NotEmptyText();

            //如果是自己人拉群 可以自動新增
            if (!model.IsFromInternalUser)
            {
                //如果被拉到奇怪的路人喇機頻道 無視
                if (
                   !model.ChannelName.ContainsWihtoutCase("target") &&
                   !model.ChannelName.ContainsWihtoutCase("bwg") &&
                   !model.ChannelName.ContainsWihtoutCase("pegasus") &&
                   !model.ChannelName.ContainsWihtoutCase("integrat")
               )
                {
                    return;
                }
            }

            int userID = InstanceBOUserID();

            try
            {
                IMChannelService.Create(userID, new IMChannelCreateRequestModel
                {
                    ChannelName = model.ChannelName,
                    Type = IMChannelTypeEnum.TG.ToInteger(),
                    TG_Chat_ID = model.Chat_ID,
                });
            }
            catch (Exception ex)
            {
                if (!SettingHelper.IsMSTest)
                {
                    SendMessageToMultipleChannel($"CreateNewChannel 失敗 , {ex.GetMessageAndErrorLine()}");
                }
            }
        }


        public void CreateNewChannelSkype(TgNewChannelJoinParameter model)
        {
            model.RuleFor(x => x.Chat_ID).Rule_NotEmptyText();
            model.RuleFor(x => x.ChannelName).Rule_NotEmptyText();

            //skype可以不檢查頻道關鍵字(只會透過內部人員觸發)
            //skype可以不檢查頻道關鍵字(只會透過內部人員觸發)
            //skype可以不檢查頻道關鍵字(只會透過內部人員觸發)

            int userID = InstanceBOUserID();

            try
            {
                IMChannelService.Create(userID, new IMChannelCreateRequestModel
                {
                    ChannelName = model.ChannelName,
                    Type = IMChannelTypeEnum.Skype.ToInteger(),
                    TG_Chat_ID = model.Chat_ID,
                });
            }
            catch
            {

            }
        }


        private int InstanceBOUserID()
        {
            return SystemConstant.DefaultUserID;
        }


        private bool IsInternalUser(TGMessage messageInfo)
        {
            if (messageInfo.from == null)
                return false;

            //1242563509 Tina
            //5625326732 joker
            return messageInfo.from.id == "1242563509" ||
                   messageInfo.from.id == "5625326732" ||
                   messageInfo.from.id == "5730706500";
        }


        private bool IsInternalUserOfSkype(SkypeWebHookRequest messageInfo)
        {
            if (messageInfo.from == null)
                return false;

            //這個是誰? 忘了...
            if (messageInfo.from.id == "29:1Q9h83MNK7AduZKbki8HGzu9jtAjBigpLyf_4MulpdSwyi8xywcGaDrxRl6EeqhGV")
            {
                return true;
            }

            if (SystemConstant.Skype_WebHook_WhiteList_UserIDs.Contains(messageInfo.from.id))
            {
                return true;
            }

            return false;
        }



        public IMMessageGenerateResult CopyCreateNewMerchantAndGenerateMessage(string myText, string myChatID, IMChannelTypeEnum type, bool isCN)
        {
            try
            {
                int userID = 101;

                var parseCommand = ParseMerchantNameByCommand(myText);

                var targetIMChannel = IMChannelService.GetByChatID(myChatID, type);

                if (targetIMChannel == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("DB查無頻道"));
                }

                int templateMerchantID = 0;

                if (parseCommand.WalletMode == MerchantWalletModeEnum.TransferMode)
                {
                    templateMerchantID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_TemplateMerchantID_Trasnfer).SettingValueOfInt;
                }
                else
                {
                    templateMerchantID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_TemplateMerchantID_SingleWallet).SettingValueOfInt;
                }

                var result = AdminSysComplexService.CopyAndBatchCreateNewMerchant(userID, new MerchantBatchCopyAndCreateRequestModel
                {
                    SourceMerchantID = templateMerchantID,
                    IMChannelID = targetIMChannel.IMChannelID,
                    WalletMode = (int)parseCommand.WalletMode,
                    MerchantNameList = new List<string>
                            {
                                ("(測試用) " + parseCommand.MerchantName)
                            }
                });

                int newMerchantID = result.ResultId;

                var previewInfo = MerchantService.GetCopyPreviewInfo(SystemConstant.SkipValidateUserID, newMerchantID);

                string sendMessage = string.Empty;

                if (isCN)
                {
                    return new IMMessageGenerateResult
                    {
                        ReplyMessage = previewInfo.CredentialMessage_CN
                    };
                }
                else
                {
                    return new IMMessageGenerateResult
                    {
                        ReplyMessage = previewInfo.CredentialMessage_EN
                    };
                }
            }
            catch (Exception ex)
            {
                string msg = $"CopyCreateNewMerchantAndGenerateMessage {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return new IMMessageGenerateResult
            {

            };
        }


        public IMMessageGenerateResult GetWagerServerInfo(string myText, string myChatID, IMChannelTypeEnum type)
        {
            var emptyResult = new IMMessageGenerateResult();

            try
            {
                myText = myText.ToLower();
                myText = myText.Replace("/wager", "");
                myText = myText.Replace("/wg", "");

                string myTransactionNo = myText.Split(" ").OrderByDescending(text => text.Length).FirstOrDefault();

                if (!RegexRuleHelper.IsPgs32TransactionNo(myTransactionNo))
                {
                    return emptyResult;
                }

                var replyInfo = BetRecordService.GetGameVendorInfoTextByTransactionNo(myTransactionNo);

                if (!string.IsNullOrWhiteSpace(replyInfo.CopyText) &&
                    type == IMChannelTypeEnum.TG &&
                    !SettingHelper.IsMSTest)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var queryReplay = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, myTransactionNo);

                        if (RegexRuleHelper.IsUrlFormat(queryReplay?.Url))
                        {
                            MyTelegramUtility.SendMessage(new TelegramMessageParameter
                            {
                                Message = queryReplay.Url,
                                TelegramExtraSetting = new TelegramExtraSetting
                                {
                                    IsCleanPrefixText = true,
                                    TargetChatID = myChatID
                                }
                            });
                        }
                    });
                }

                return new IMMessageGenerateResult
                {
                    ReplyMessage = replyInfo.CopyText
                };
            }
            catch (Exception ex)
            {
                string msg = $"CopyCreateNewMerchantAndGenerateMessage {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return emptyResult;
        }

        public IMMessageGenerateResult GetWagerByRoundID(string myText, string myChatID, IMChannelTypeEnum type)
        {
            var emptyResult = new IMMessageGenerateResult();

            try
            {
                myText = myText.ToLower();
                myText = myText.Replace("/round", "");

                var allParam = myText.Split(" ").OrderByDescending(text => text.Length)
                    .FirstOrDefault()
                    .Split(",")
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                string roundID = allParam.FirstOrDefault();

                int gameVendorID = GameVendorTypeEnum.Galaxy.ToInteger();

                if (allParam.Count() == 2)
                {
                    string myGameVendor = allParam[1];

                    if (myGameVendor.IsNumbericOfPositiveInt32())
                    {
                        gameVendorID = Int32.Parse(myGameVendor);
                    }
                }

                int userID = InstanceBOUserID();
                int daysRange = 14;

                var queryWagers = BetRecordService.GetUnionBetRecordsByPagingFilter(userID, new QueryUnionBetRecordRequestParameterModel
                {
                    PageSize = 10,
                    PageIndex = 1,
                    GameVendorID = gameVendorID,
                    RoundID = roundID,
                    CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * daysRange),
                    CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                });

                string myMsg = "";

                IsSendMarkdownFormat = true;

                if (queryWagers.Items.IsEmptyOrNull())
                {
                    myMsg = $"查无注单资料 (近{daysRange}天)";
                }
                else
                {
                    myMsg += $"局號：{roundID} 有以下注單 ({queryWagers.Items.Count()}筆) {Environment.NewLine}{Environment.NewLine}";

                    foreach (var item in queryWagers.Items.OrderBy(x=> x.PlayerID))
                    {
                        int myPlayerID = item.PlayerID;

                        var statusText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)item.Status , LanguageEnum.zh_cn);

                        decimal totalPayoutOfThisPlayer = queryWagers.Items.Where(x => x.PlayerID == myPlayerID).Select(x => x.PayoutAmount).Sum();

                        myMsg += @$"✏️ `{item.BetRecordNoPGS}`
單號：{item.BetRecordNo}
货币：{item.Currency}
玩家：{myPlayerID} ({item.PlayerName})
状态：{statusText} ({item.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")} GMT+8)
金额：{Math.Round(item.BetAmount, 2)}／{Math.Round(item.PayoutAmount, 2)} (投注/派奖)
加總：{Math.Round(totalPayoutOfThisPlayer, 2)} (Round total payout)
{Environment.NewLine}";
                    }
                }

                return new IMMessageGenerateResult
                {
                    ReplyMessage = myMsg
                };
            }
            catch (Exception ex)
            {
                string msg = $"GetWagerByRoundID {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return emptyResult;
        }


        public IMMessageGenerateResult GetAllApiList(string myText, string myChatID, IMChannelTypeEnum type)
        {
            var emptyResult = new IMMessageGenerateResult();

            try
            {
                myText = myText.ToLower();
                myText = myText.Replace("/apis", "");
                myText = myText.Replace("/apilist", "");
                myText = myText.Replace("/api_list", "");

                string myType = myText.Split(" ").OrderByDescending(text => text.Length).FirstOrDefault().Trim().ToLower();

                if (string.IsNullOrWhiteSpace(myType))
                {
                    SendErrorMsgToChannel("apis指令 需要傳tf或sw的參數");
                    return emptyResult;
                }

                string apiUrlDomain = SystemConstant.DocumentVendorAPI;

                MySwaggerApiDocumentJsonModel apiResult = null;

                string replyText = "";

                var documentAPI = SystemConstant.DocumentVendorAPI;

                if (myType == "tf")
                {
                    replyText = "Hi, there are api list (轉帳模式／Transfer Mode). For more detail please refer our API document." + NEW_LINE + NEW_LINE;

                    if (IsPGSNotifyErrorChannel)
                    {
                        replyText += documentAPI + "/api-document/transfer/EN" + NEW_LINE;
                        replyText += documentAPI + "/api-document/transfer/CN" + NEW_LINE + NEW_LINE;
                    }

                    apiResult = MySwaggerApiDocumentHelper.GetAllApis(apiUrlDomain, "swagger/tfMode/swagger.json");
                }
                else
                {
                    replyText = "Hi, there are api list (無縫錢包模式／Seamless Mode). For more detail please refer our API document." + NEW_LINE;

                    if (IsPGSNotifyErrorChannel)
                    {
                        replyText += documentAPI + "/api-document/singleWallet/EN" + NEW_LINE;
                        replyText += documentAPI + "/api-document/singleWallet/CN" + NEW_LINE + NEW_LINE;
                    }

                    replyText += $@"
-------------------------------------
5.1.1  Get Platers Wallet amount
5.2.1  Player Place Bet
5.2.2  Settle Bet (Normal Wager)
5.2.5  Send Event prize (event-bonus)
5.2.9  Cancel Bet
-------------------------------------
{NEW_LINE}";

                    apiResult = MySwaggerApiDocumentHelper.GetAllApis(apiUrlDomain, "swagger/swMode/swagger.json");
                }

                if (apiResult == null || apiResult.paths.IsEmptyOrNull())
                {
                    SendErrorMsgToChannel("apiResult swagger物件無法取得任何API");
                    return emptyResult;
                }

                //var matchChannel = IMChannelRepository.GetByChatID(myChatID , type);

                //string channelName = (matchChannel?.DisplayName ?? "");

                //bool isAMChannel = channelName.ContainsWihtoutCase("(AM") || channelName == "target";

                ////為了避免AM亂保存連結
                //if(!isAMChannel)
                //{
                //}

                foreach (var eachApi in apiResult.paths)
                {
                    var myApi = eachApi.Value;

                    var detailInfo = myApi.IsHttpGet ? myApi.get : myApi.post;

                    string methodText = myApi.IsHttpGet ? "GET｜" : "POST｜";

                    replyText += $"`{methodText}{eachApi.Key}`{NEW_LINE} {detailInfo?.summary} {NEW_LINE}{NEW_LINE}";
                }

                IsSendMarkdownFormat = true;

                return new IMMessageGenerateResult
                {
                    ReplyMessage = replyText
                };
            }
            catch (Exception ex)
            {
                string msg = $"CopyCreateNewMerchantAndGenerateMessage {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return emptyResult;
        }


        private MerchantBatchCopyCommandParseModel ParseMerchantNameByCommand(string myText)
        {
            string myFinalText = myText;
            myFinalText = Regex.Replace(myFinalText, "/key_en", "", RegexOptions.IgnoreCase);
            myFinalText = Regex.Replace(myFinalText, "/key_cn", "", RegexOptions.IgnoreCase);
            myFinalText = Regex.Replace(myFinalText, "/keycn", "", RegexOptions.IgnoreCase);
            myFinalText = Regex.Replace(myFinalText, "/keyen", "", RegexOptions.IgnoreCase);

            var split = myFinalText.Split(",")
                .Where(x => x.IsNotEmptyTextNotNull())
                .Select(x => x.Trim())
                .ToList();

            if (split.IsEmptyOrNull() || split.Count() != 2)
            {
                SendMessageToTelegram($"語法錯誤無法解析參數 {myText}", TelegramChannelEnum.internal_prview_content);
                throw new TerproException(ApiErrorCode.InvalidParameter);
            }

            return new MerchantBatchCopyCommandParseModel
            {
                MerchantName = split[0],
                WalletMode = split[1].ToLower() == "sw" ? MerchantWalletModeEnum.SingleWalletMode : MerchantWalletModeEnum.TransferMode
            };
        }



        private IMMessageGenerateResult GenerateReplyMessageByKeywordType(string myText, IMChannelTypeEnum imType, string myChatID)
        {
            var myModel = new IMMessageGenerateResult();
            string replyMsg = "";

            //字數太多會認定為非指令 (e.g. 跟客戶說明某些API 剛好有url關鍵字match)
            if (string.IsNullOrWhiteSpace(myText) ||
               myText.Count() >= 20)
            {
                return myModel;
            }

            bool isTG = imType == IMChannelTypeEnum.TG;
            bool isSkype = imType == IMChannelTypeEnum.Skype;

            bool isCNMsg = PreferLanguages.Any(x => x == LanguageEnum.zh_cn);
            bool isENMsg = PreferLanguages.Any(x => x == LanguageEnum.en_us);
            string multiLangLINE = (isCNMsg && isENMsg) ? (NEW_LINE + NEW_LINE) : "";

            var brand = GenerateBrandName();

            string brandName_normal = brand.Name;
            string brandName_short = brand.ShortName;


            if (myText.ContainsWihtoutCase("/4.1.1") || myText.ContainsWihtoutCase("/411") || myText.ContainsWihtoutCase("/資訊不足"))
            {
                replyMsg = @$"您好，您目前提供的資訊可能不足、或問題描述過於模糊。技術仍難以判斷問題
- 需請您提供更多資訊(e.g. 帳號、商戶號、單號 etc...)
- 需請您提供錯誤訊息、錯誤畫面截圖/视频
- 截圖請提供全屏截圖、不要僅擷取一小塊畫面。會影響資訊判斷
- 若您是AM 建議參考手冊 #4.1.1 項目說明";
            }
            else if (myText.ContainsWihtoutCase("/4.6.1") || myText.ContainsWihtoutCase("/461"))
            {
                replyMsg = $"您好，需麻煩參考AM手冊 <ins>#4.6.1</ins>、提供更多資訊。{NEW_LINE}關於gameList問題。請先確保商戶配置無誤、代理使用正確的商戶號。若問題持續發生則需要提供log紀錄(見手冊)";
            }
            else if (myText.ContainsWihtoutCase("/注單號") || myText.ContainsWihtoutCase("/投注單號") || myText.ContainsWihtoutCase("/注單編號") ||
                     myText.ContainsWihtoutCase("/tranNo") || myText.ContainsWihtoutCase("/tran_No") || myText.ContainsWihtoutCase("/transactionNo"))
            {
                var sampleOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
                IsSendMarkdownFormat = true;

                replyMsg += isENMsg ? $"Hi, regarding bet-record issue. Please provide `betting transaction number` (API: TransactionNo, 32 chars). Thank you. {NEW_LINE}(⚠️You may require the assistance of your technical team){multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，注单相关问题。需要麻烦您提供 `32码注单号` (对应API: TransactionNo)。谢谢  {NEW_LINE}(⚠️此项资讯可能需您团队的技术人员协助)" : "";

                replyMsg += $"{NEW_LINE}{NEW_LINE}Sample(范例): ``` {sampleOrderNo} ```";

            }
            else if (myText.ContainsWihtoutCase("/包網問題") || myText.ContainsWihtoutCase("/包網頁面") || myText.ContainsWihtoutCase("/平台頁面"))
            {
                IsSendMarkdownFormat = true;

                replyMsg = @$"
您好，您反馈的问题是发生在代理网站(包网网站)中。此问题应为网站(包网)系统的问题
建議您與贵司自己的技术团队确认此问题。

`*遊戲清單問題`
❓ 确认您系统已经刷新至最新 game list (调用 4.6.1 games API)
❓ 确认您是否调用 4.6.1 games API有得到错误?
❓ 與您的技术团队确认是否已更新游戏清单? 游戏开关被设置关闭？
❓ 确认您的网站系统是否有设置游戏商维护? 导致游戏暂时被隐藏
❓ 您要寻找的是否为其他名称相似的GameProvider (e.g. PG Soft)，我方是{brandName_normal} ({brandName_short})。

`*其他问题`
❓ 确认您的网站系统配置是否被调整? (e.g. 游戏开关配置)
❓ 确认您的包网后台、代理号是否被非法登入/非法调整游戏设定? 
❓ 與您的技术团队确认，包网系统数据库忙碌中? 近期包网代码有更新导致功能异常? 
❓ 若API调用得到失败。请提供我方API回应的错误讯息或返回信息
";
            }
            else if (myText.ContainsWihtoutCase("/links"))
            {
                string sendMsgToOtherChannel = "";

                var documentAPI = SystemConstant.DocumentVendorAPI;

                sendMsgToOtherChannel += "API文件連結 (Swagger)" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/api-document/transfer/EN" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/api-document/transfer/CN" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/api-document/singleWallet/EN" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/api-document/singleWallet/CN" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel += "API文件連結 (Google doc) 轉帳 (中/英)" + NEW_LINE;
                sendMsgToOtherChannel += "https://example.invalid/document/d/1BQhauNjmVmWDQHWbCE_M1d8h9AKofH1HH4IvCcSfvx4/edit?usp=sharing" + NEW_LINE;
                sendMsgToOtherChannel += "https://example.invalid/document/d/1MGr8qB4Q5-RDPcUwC-Ojxk_9ysdtjPezRyXfDKu8eG4/edit?usp=sharing" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel += "API文件連結 (Google doc) 單錢包 (中/英)" + NEW_LINE;
                sendMsgToOtherChannel += "https://example.invalid/document/d/17l2IO3k_vKE_olbJxPZ61_-xSD0bUde0nKttIjTcAyg/edit?usp=sharing" + NEW_LINE;
                sendMsgToOtherChannel += "https://example.invalid/document/d/1Xiq6R4Pll2QPEKIMsmw1kRgqz-DSiEvs8POoT012b9Q/edit?usp=sharing" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel += "編號對照表" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/document/enums/en" + NEW_LINE;
                sendMsgToOtherChannel += documentAPI + "/document/enums/Cn" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel = sendMsgToOtherChannel.ReplaceDuplicateUrlChar();

                sendMsgToOtherChannel += "AGG常見啟動錯誤(錯誤碼)" + NEW_LINE;
                sendMsgToOtherChannel += "http://example.invalid:1688/slot/PgsGamingTickets/-/wikis/AGG%E5%95%9F%E5%8B%95%E9%81%8A%E6%88%B2%E9%8C%AF%E8%AA%A4-%E5%B8%B8%E8%A6%8B%E5%8E%9F%E5%9B%A0" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel += "Gitlab 文件連結總覽" + NEW_LINE;
                sendMsgToOtherChannel += "http://example.invalid:1688/backend/target/game.backend/-/wikis/API%E5%B0%8D%E6%8E%A5%E6%96%87%E4%BB%B6%E9%80%A3%E7%B5%90" + NEW_LINE + NEW_LINE;

                sendMsgToOtherChannel += "IP地區限制" + NEW_LINE;
                sendMsgToOtherChannel += "http://example.invalid:1688/slot/PgsGamingTickets/-/wikis/IP%E5%9C%B0%E5%8D%80%E9%99%90%E5%88%B6-(%E6%A0%B9%E6%93%9A%E8%B2%A8%E5%B9%A3%E4%B8%8D%E5%90%8C%E9%85%8D%E7%BD%AE)" + NEW_LINE + NEW_LINE;

                SendErrorMsgToChannel(sendMsgToOtherChannel, TelegramChannelEnum.pgs_common_error);
            }
            else if (myText.ContainsWihtoutCase("/cs_player"))
            {
                replyMsg += $"您好，請參考客服手冊 #2.5.1 協助查詢玩家資訊。(若客戶沒有提供、則需要跟客戶索取此資訊)。謝謝";
            }
            else if (myText.ContainsWihtoutCase("/收斂") || myText.ContainsWihtoutCase("/局數"))
            {
                replyMsg += isENMsg ? $"Hi, if the game-round number is insufficient. Extreme RTP are more likely occur. Typically, at least 10,000 games to reach the expected RTP of the game.{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，若游玩总局数不足。RTP较容易出现极端数字的状况。一般而言要至少10000局才会达到接近游戏预期RTP的数值" : "";
            }
            else if (myText.ContainsWihtoutCase("/m_id") || myText.ContainsWihtoutCase("/mid") || myText.ContainsWihtoutCase("/merchantID"))
            {
                replyMsg += isENMsg ? $"Hi team, we need you provide the MerchantID (6 digit numbers, e.g. 175001). Our tech team need use this to check issue. ⚠️If you don't know how to get this, please contact your tech team for assist.{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，需要提供您平台使用的商户MerchantID (六个数字格式 e.g. 175001)。 技术团队需要此资讯才能追查问题。⚠️若您不清楚此项资讯如何确认、可能需询问您团队的技术单位" : "";
            }
            else if (myText.ContainsWihtoutCase("/myIP") || myText.ContainsWihtoutCase("/my_IP") || myText.ContainsWihtoutCase("/pgs_ip") || myText.ContainsWihtoutCase("/pgsip"))
            {
                replyMsg = $"Here are our server IPs, please add to whiteList." + NEW_LINE + string.Join(NEW_LINE, SystemConstant.PgsServerWhiteListIPs);
            }
            else if (myText.ContainsWihtoutCase("/info_p2") || myText.ContainsWihtoutCase("/infop2"))
            {
                replyMsg = $"您好，我方已在上方讯息提供商务代理号 (或AppID)，其他资讯贵司技术能自己於系统中反查。不应由我方來提供。应由贵司技术协助确认，谢谢";
            }
            else if (myText.ContainsWihtoutCase("/info_p") || myText.ContainsWihtoutCase("/infop"))
            {
                replyMsg = $"您好，我方已在上方讯息提供【必要资讯】。透过这些资讯、贵司技术团队应能查询确认问题(e.g. 哪个代理商务号)。请确保讯息有转发给技术团队";
            }
            else if (myText.ContainsWihtoutCase("/r_id") || myText.ContainsWihtoutCase("/rid"))
            {
                replyMsg += @$"你好，需要麻煩提供下注單的 roundId (gameRoundId)。另外希望未來只要注單都提供這項資訊。謝謝您
";
            }
            else if (myText.ContainsWihtoutCase("/gv_info") || myText.ContainsWihtoutCase("/gameVendor_info"))
            {
                replyMsg += @$"⭐各個遊戲商溝通名詞
VPlus:  appID ／ gameRoundId
Fuma:  parent ／ roundId + freetotalwin
";
            }
            else if (myText.ContainsWihtoutCase("/gp") || myText.ContainsWihtoutCase("/gv") || myText.ContainsWihtoutCase("/gameVendor"))
            {
                this.IsSendMarkdownFormat = true;
                replyMsg += isENMsg ?
@$"Hi, It's looks like you describe is not our games. (or not our error message format)
Can you confirm again the game provider is `【{brandName_normal} ({brandName_short})】` ? 
or you looking for other game provider (e.g. PG／PS／PG Soft／PocketGame／Other){multiLangLINE}" : "";

                replyMsg += isCNMsg ?
@$"您好，您反馈的游戏似乎不是我方品牌的游戏 （或非我方的错误格式）
需要请您再次确认下，该款游戏，游戏商是 `【{brandName_normal} ({brandName_short})】`？
或您正在寻找其他游戏商（例如: PG／PS／PG Soft／PocketGame／其他）" : "";
            }
            else if (myText.ContainsWihtoutCase("/gmt8"))
            {
                replyMsg += isENMsg ? $"Hi, We need more precise time range for querying. Please provide a time range like below sample. (GMT+8) {NEW_LINE}e.g. 2024-01-01 00:00:00 ~ 2024-01-01 00:30:00 (GMT+8) {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，我方需要更精准时间范围做查询。需要请您提供类似下方的时间格式 (GMT+8 时区) {NEW_LINE}e.g. 2024-01-01 00:00:00 ~ 2024-01-01 00:30:00 (GMT+8)" : "";
            }
            else if (myText.ContainsWihtoutCase("/截圖") || myText.ContainsWihtoutCase("/screen"))
            {
                replyMsg += isENMsg ? $"Hi team, can you help provide the screenshot picture or video when issue happen? We need this to help tech team check the issue.{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，需要請您帮忙提供問題發生時《截图／视频》，这将能帮助技术单位确认问题、谢谢您" : "";
            }
            else if (myText.ContainsWihtoutCase("/done"))
            {
                replyMsg += $@"
Hi, technical team has update setup, please help confirm whether the problem has been solved (you may need to login again or re-operate again). Thank you.{NEW_LINE}
技术团队已调整配置，请客户确认问题是否已解决 (可能需要重新登入/重新操作一次)，感谢您";

            }
            else if (myText.ContainsWihtoutCase("/錄影") || myText.ContainsWihtoutCase("/record") || myText.ContainsWihtoutCase("/video"))
            {
                replyMsg += isENMsg ? $"Hi, We need player help provide 'video record' while operate. This will help the technical team check the issue, thank you. {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好 需要麻烦提供《操作过程录像》。这将能帮助技术单位确认问题、谢谢您" : "";
            }
            else if (myText.ContainsWihtoutCase("/ip_test") || myText.ContainsWihtoutCase("/iptest"))
            {
                string link1 = "https://example.invalid/lobby?page=speedTest";
                string link2 = "https://example.invalid/lobby/?page=speedTest";

                replyMsg += isENMsg ? $"Hi Teams, We need player help test the both links, and screenshot the information.{NEW_LINE}Please ask player to assist with this. (both link need validate){NEW_LINE}{link1}{NEW_LINE}{link2}{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，需要玩家协助测试以下网址{NEW_LINE}并将画面截图给我们 (两组网址都需要测试截图){NEW_LINE}{link1}{NEW_LINE}{link2}{NEW_LINE}" : "";
            }
            else if (myText.ContainsWihtoutCase("/enums"))
            {
                string enumsLink_CN = (SystemConstant.DocumentVendorAPI + "/document/enums/CN").ReplaceDuplicateUrlChar();
                string enumsLink_EN = (SystemConstant.DocumentVendorAPI + "/document/enums/EN").ReplaceDuplicateUrlChar();

                replyMsg += $"{enumsLink_EN}{NEW_LINE}{enumsLink_CN}{NEW_LINE}";
                replyMsg += isENMsg ? $"Hi, regarding the number code table / enums code, please refer this link.{NEW_LINE}" : "";
                replyMsg += isCNMsg ? $"您好，关于编号对照／编号代码表　请参考此连结{NEW_LINE}" : "";
            }
            else if (myText.ContainsWihtoutCase("/min_bet") || myText.ContainsWihtoutCase("/minbet"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"⚠️*in our Dev(Staging) environment. We only support one currency 'BETA coin'.
You may notice min/max bet amount value is large. but you can ignore it in this environment.
Please validate min/max amount in production environment. {multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"(⚠️环境限制) 测试环境不分币别。统一使用测试币(BETA Coin)
您可能会注意到最低 / 最大投注金额 数值很大。但您可以忽略此问题
建议您在正式环境阶段再进行验证(最低 / 最大投注金额). {multiLangLINE}";
                }
            }
            else if (myText.ContainsWihtoutCase("/op_support") ||
                     myText.ContainsWihtoutCase("/op_sup") ||
                     myText.ContainsWihtoutCase("/opsupport"))
            {
                replyMsg += @"
您好，这项问题需要请贵司OP维运人员协助、整理以下资讯。再转交我方技术人员进行确认
需请您帮忙tag这两位协助  @bya_kko @chouwilliam 

⭐ <ins>OP帮忙确认以下资讯</ins>
- 发生此错误是哪种模式：一般游玩模式／活动模式(刷分排行)
- 玩家游戏帐号：(一般模式 & 活动模式 *帐号不同)
- 游戏名称：
- 发生时间(GMT+8)：e.g. 2024/01/01 06:30 GMT+8
- 玩家IP： e.g. 0.0.0.0
- 截图图片／错误信息／影片录像
";
            }
            else if (myText.ContainsWihtoutCase("/sw_balance") || myText.ContainsWihtoutCase("/sw_bal"))
            {
                if (isCNMsg)
                {
                    replyMsg +=
@$"Hi, Your merchant is the seamless mode 
If player has balance issues. All of player's balance-update is working on your API side. Our side cannot query the balance-records.
You need to contact your technical team to confirm the balance change logs in your side. Thank you. {multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，贵司采用单钱包模式对接 (Seamless)
若有余额异常的问题。玩家余额的增减操作都是在贵司系统中执行。我方这边无法查询纪录。
需要联系貴方技术团队自行确认金币异动纪录。谢谢您. {NEW_LINE}";
                }

            }
            else if (myText.ContainsWihtoutCase("/sw_test2") || myText.ContainsWihtoutCase("/sw_test_QA") ||
                     myText.ContainsWihtoutCase("/self_test") || myText.ContainsWihtoutCase("/sw_test_self"))
            {

                if (isENMsg)
                {
                    replyMsg += @$"
Hi, our system only provide trigger some scenario for validate. most of case, operator need test by theirself.
Here are our suggest test scenario.
- correct enter to the game. 
- correct balance amount in the game.
- can betting/spin correctly
- can settle/payout correctly
- wager data (betAmount/payoutAmount/WinLossAmount) is correct in your BO. {multiLangLINE}";

                }

                if (isCNMsg)
                {
                    replyMsg += @$"
您好，我们的系统仅提供触发某些场景进行验证(Seamless callback)。运营商需要自行测试功能正常
以下是我们建议的测试情境：
- 能否正确进入游戏
- 游戏中显示的余额正确
- 可以正确下注/旋转
- 可以正确结算/派奖
- 运营商后台，显示的投注数据（投注金额/支付金额/赢输金额）数值正确。{multiLangLINE}";
                }

            }
            else if (myText.ContainsWihtoutCase("/sw_test") || myText.ContainsWihtoutCase("/swtest"))
            {
                replyMsg += isENMsg ? @$"
For seamless merchant. We can trigger some scenario for test (e.g. cancel/event-bonus..)
But you need provide those information for us.{NEW_LINE}
- MerchantID
- PlayerUniqueID, and need at least betting one times in recent 24 hours, and ensure the last sessionToken is alive. {multiLangLINE}" : "";

                replyMsg += isCNMsg ? @$"
对于无缝钱包商户，我方可以協助触发几种callback情境(包含 取消投注/活动派奖/重复结算等...)
但需要商户端提供以下资讯{NEW_LINE}
- 商户ID (MerchantID)
- PlayerUniqueID，玩家需于近期24小时内至少投注过一次、并确保最后一笔sessionToken存活有效" : "";

            }
            else if (myText.ContainsWihtoutCase("/lang"))
            {
                replyMsg += isENMsg ? $"Hi team, our game support language is Chinese (zh-cn) & English (en-us). {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，我们游戏支持语言是中文（zh-cn）& 英语（en-us）。{NEW_LINE}" : "";
            }
            else if (myText.ContainsWihtoutCase("/捕魚轉帳") || myText.ContainsWihtoutCase("/捕魚封鎖") || myText.ContainsWihtoutCase("/捕魚BAN"))
            {
                replyMsg += isENMsg ? $"If player operate too many transfers in a short period of time, the system will consider it an abnormality and block the account. Please notify to player and note this in the future. {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"若玩家在短时间内操作太多次转帐会被系统认定为异常而封锁帐号。请玩家未来多加注意" : "";
            }
            else if (myText.ContainsWihtoutCase("/hk_country") || myText.ContainsWihtoutCase("/hkcountry"))
            {
                replyMsg += isENMsg ? $"Hi, our regional policy had adjusted since September 2024, China/Taiwan/HongKong IP will be restricted. Players need to VPN to other regions. (e.g. Malaysia、 Thailand). {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，我方地区政策于2024年9月有调整，台/港/澳/中国IP 会被限制。玩家需VPN至其他地区才能游玩 (e.g. 马来、泰国)" : "";
            }
            else if (myText.ContainsWihtoutCase("/ip_country"))
            {

                if (isENMsg)
                {
                    replyMsg += $@"
Hi, our service currently does not support the following regions IP
China/Macau/Hong Kong/Taiwan{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += $@"
您好，目前我们服务不支援以下地区IP
中国／澳门／香港／台湾{Environment.NewLine}";
                }

                #region IP國家 (舊版)


                //                if (isENMsg)
                //                {
                //                    replyMsg += $@"
                //Regarding ip restrict policy. We base on merchant's Currency. 
                //Different currency have different ip country group.
                //For example. If merchant's currency MYR/SGD/AUD/THB/IDR/ ...etc
                //The setting is use default-region group. (We have default-region、us-region ...etc)

                //Available ip country is:
                //-MY (Malaysia)
                //-AU (Australia)
                //-VN (Vietnam)
                //-SG (Singapore)
                //-TH (Thailand)
                //-BN (Brunei)
                //-PH (Philippine)
                //-KH (Cambodia)
                //-HK (Hongkong)
                //-MM (Myanmar)
                //-ID (Indonesia)
                //-BR (Brazil)
                //-KR (Korea)
                //-KE (Kenya)
                //{multiLangLINE}";
                //                }

                //                if (isCNMsg)
                //                {
                //                    replyMsg += $@"
                //关于IP限制政策。我们会根据商户货币(Currency)为基础，配置有不同的區域群组
                //不同的货币有不同的ip国家限制。
                //例如。 若商家货币为MYR/SGD/AUD/THB/IDR...等。
                //将配置使用默认区域(default)设置組別。

                //可允許的來自這些国家的IPs
                //-MY (马来西亚)
                //-AU (澳洲)
                //-VN (越南)
                //-SG (新加坡)
                //-TH (泰国)
                //-BN (汶莱)
                //-PH (菲律宾)
                //-KH (柬埔寨)
                //-HK (香港)
                //-MM (缅甸)
                //-ID (印尼)
                //-BR (巴西)
                //-KR (韩国)
                //-KE (肯亚)
                //根据商户货币不同、以及我方对于各區域分组的设定、而有所不同
                //我方亦會根据评估、观察，不定期变动调整";
                //                }



                #endregion
            }
            else if (myText.ContainsWihtoutCase("/applykey") || myText.ContainsWihtoutCase("/apply_key"))
            {
                replyMsg = "";

                if (isENMsg)
                {
                    replyMsg +=
                        $"Hi if you need apply api key , please provide the following information. thank you.{NEW_LINE}" +
                        $"- Environment：(Staging / Production){NEW_LINE}- Platform Name / branding name：{NEW_LINE}- Currency：{NEW_LINE}- Wallet Mode：(Transfer / Seamless){NEW_LINE}- Operator's server outbound IP Address (we need to add whitelist)：{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
                        $"您好，申请API金钥，需要贵司提供以下资讯、谢谢您{NEW_LINE}" +
                        $"- 申请环境：(测试 / 正式){NEW_LINE}- 平台名称(品牌名)：{NEW_LINE}- 使用货币：{NEW_LINE}- 钱包模式：(转帐／无缝钱包){NEW_LINE}- 运营商伺服器IP(我方须设置防火墙)：{NEW_LINE}";
                }
            }
            else if (myText.ContainsWihtoutCase("/games_api_limit") || myText.ContainsWihtoutCase("/gamesapilimit") ||
                     myText.ContainsWihtoutCase("/max_times_games") || myText.ContainsWihtoutCase("/limit_times_games"))
            {
                replyMsg = "";

                if (isENMsg)
                {
                    replyMsg +=
                        @$"
Hi, games API has a request limit times (please refer API document).
Due to a past experience with a large number of abnormal requests, some APIs have limited access times.
Regarding games API. our suggest is request onece per day to update game list. or call API when we release new games.
{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
                        @$"
您好，游戏 API 有请求频率限制（请参阅 API 文档）。
過往曾出现大量异常请求，因此部分API的访问次数有設定限制

游戏 API，我们建议每天请求一次以更新游戏列表，或在新游戏发布时呼叫 API";
                }
            }
            else if (myText.ContainsWihtoutCase("/limit_times") || myText.ContainsWihtoutCase("/max_times") ||
                     myText.ContainsWihtoutCase("/req_limit") || myText.ContainsWihtoutCase("/request_limit") ||
                     myText.ContainsWihtoutCase("/request_times"))
            {
                replyMsg = "";

                if (isENMsg)
                {
                    replyMsg +=
                        @$"
Our system calculates the counter based on the parameter hash. 
If your parameters have different values. The calculator will calculate separately. (e.g. page=1 and page=2)

For example, If your system query those 5 reqeust in 1 hours.
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=2&startTime=2025-01-01
merchantID=175001&page=3&startTime=2025-01-01

Our system will have those counter-record
merchantID=175001&page=1&startTime=2025-01-01  【※3 times】
merchantID=175001&page=2&startTime=2025-01-01  【※1 times】
merchantID=175001&page=3&startTime=2025-01-01  【※1 times】

⭐Our suggestion is to fetch data every 3 to 5 minutes.
If you use it normally, it is difficult to trigger the blocking mechanism. (Production: 60 times)
Because an infinite-loop requests occurred by some platform’s system in the past.
Therefore, this mechanism is added to avoid excessive requests for our system.{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
                        @$"
我们系统是根据参数Hash(哈希)各别统计计数器。
如果您每次请求带入的参数有不同的值。计算器将「分别计算」。(e.g. page=1 and page=2)

例如，如果您的系统在 1 小时内查询了这 5 个请求
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=1&startTime=2025-01-01
merchantID=175001&page=2&startTime=2025-01-01
merchantID=175001&page=3&startTime=2025-01-01

在我们系统会有这些各別计数统计
merchantID=175001&page=1&startTime=2025-01-01  【※3 times】
merchantID=175001&page=2&startTime=2025-01-01  【※1 times】
merchantID=175001&page=3&startTime=2025-01-01  【※1 times】

⭐我们的建议是每3~5分钟捞取一次资料
若您正常使用是很难触发阻挡机制的。(正式环境: 60 times) 
因之前有平台的系统出现过无穷循环请求的情况，
所以增加了这个机制，避免我们的系统遇到无穷请求资源浪费。";
                }

            }
            else if (myText.ContainsWihtoutCase("/agg_rq") || myText.ContainsWihtoutCase("/aggrq"))
            {
                replyMsg +=
@$"您好，需要麻煩提供以下詳細資訊，幫助我方技術更精確查找問題。謝謝您
- request 發送時間
- request url
- request payload (POST)
- response text";

            }
            else if (myText.ContainsWihtoutCase("/404account") || myText.ContainsWihtoutCase("/account404") || myText.ContainsWihtoutCase("/404_account") ||
                     myText.ContainsWihtoutCase("/account_404") || myText.ContainsWihtoutCase("/404player") ||
                     myText.ContainsWihtoutCase("/player_account"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi team. The account you provided do not exist in our system. Please confirm you provided correct information or you need confirm with your tech-team.
(*⚠️ Game account may be different from platform account. you may need ask your tech team.){multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，您提供的帐户在我们系统中查无此帐号。请确认您提供了正确的信息、或您可能需要与您的技术团队确认
(*⚠️ 游戏帐号可能与平台帐号不同，您可能需要请技术单位协助提供)";
                }
            }
            else if (myText.ContainsWihtoutCase("/playerAccount"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi team. We need you provide the following information, tech team need those check issue.{NEW_LINE} 
- PlayerAccount / PlayerUniqueID ({brandName_short}'s account)

(*⚠️ Game account may be different from platform account. you may need ask your tech team.)
{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
$@"您好，需請您协助提供以下资讯、帮助技术确认问题 {NEW_LINE}
- 玩家帳號 ({brandName_short}游戏帳號)

(*⚠️ 游戏帐号可能与平台帐号不同，您可能需要请技术单位协助提供)";
                }
            }
            else if (
                myText.ContainsWihtoutCase("/game_issue") || myText.ContainsWihtoutCase("/gameissue") ||
                myText.ContainsWihtoutCase("/g_issue") || myText.ContainsWihtoutCase("/gissue") ||
                myText.ContainsWihtoutCase("/more") || myText.ContainsWihtoutCase("/更多資訊"))
            {
                bool isMore = myText.ContainsWihtoutCase("/more") || myText.ContainsWihtoutCase("/更多資訊");

                if (isENMsg)
                {
                    string extraText = isMore ? "" : "If player got game issue(launch/betting).";
                    replyMsg +=
@$"Hi team. {extraText} We need you provide the following information, tech team need those check issue.{NEW_LINE} 
- PlayerAccount / PlayerUniqueID： ({brandName_short}'s account)
- Game Name (or GameID)：
- Happening time (GMT+8)： e.g. 2024/01/01 05:30 ~ 2024/01/01 06:30
- IPAddress： e.g. 0.0.0.0
- Screenshot picutre / error message / video record

(*⚠️ Game account may be different from platform account. you may need ask your tech team.)
{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    string extraText = isMore ? "" : "若玩家在游戏上遇到问题(游戏启动/投注異常)，";
                    replyMsg +=
$@"您好，{extraText}需請您协助提供以下资讯、帮助技术确认问题 {NEW_LINE}
- 玩家帳號：({brandName_short}游戏帳號)
- 游戏名称： 
- 发生时间(GMT+8)： e.g. 2024/01/01 05:30 ~ 2024/01/01 06:30 GMT+8
- 玩家IP： e.g. 0.0.0.0
- 截图图片／错误信息／影片錄像

(*⚠️ 游戏帐号可能与平台帐号不同，您可能需要请技术单位协助提供)";
                }
            }
            else if (myText.ContainsWihtoutCase("/1008"))
            {
                if (isENMsg)
                {
                    replyMsg += @"#1008 error, usually caused by the system temporarily busy. or caused by netowrk issue." +
                                $"Can player confirm again whether the game is running normally now? Thank you.{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += @"#1008 错误，通常发生原因为系统暂时性忙碌或网路异常所造成" +
                                "能否请玩家确认。目前游戏进行是否已正常。谢谢";
                }
            }
            else if (myText.ContainsWihtoutCase("/3500"))
            {
                if (isENMsg)
                {
                    replyMsg += $@"Hi, #3500 is bet/spin failed.
Possible reasons include
- network issue
- server response timeout
- player token expiration
- seamless callback error (seamless mode)
We recommend that players re-enter the game and confirm whether this problem still occurs.{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += @"您好，#3500 为投注失败。
可能原因为 
- 网路连线异常
- 遊戲连线逾時
- 玩家令牌失效
- 无缝钱包模式callback异常
建议玩家重新进入游戏、确认是否仍出现此问题";
                }
            }
            else if (myText.ContainsWihtoutCase("/simulate_api") || myText.ContainsWihtoutCase("/simulate_400") ||
                     myText.ContainsWihtoutCase("/simulate_error") || myText.ContainsWihtoutCase("/400cant"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi team, due to company policy, We aren't allow directly use the API parameters to simulation same request (including the following reasons)
- Our outbound IPs are different from your server
- Different programming language environments, inconsistent environments, It's difficult to accurately simulate same situation.
- If the API involves wallet amount adjustment / data change / data creation, it may affect players'rights.

We need you help, use the HttpRequest tool (e.g. postman/cURL) to test the request and take a screenshot of the response result for us to confirm.
Or provide the full error response from API. Btw, Please ensure that your code has exception handling for 400 statusCode. Some language codes require additional handle to obtain the response content. " + multiLangLINE;
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，由于公司政策，我方技术无法直接使用贵司提供的参数进行API模拟 (包含以下可能原因)
- 我方IPs与贵司IPs不同
- 程序语言环境不同、环境不一致、无法精确模拟相同情境
- 若API涉及钱包金额调整或资料异动、创建，将可能对玩家权益造成影响

需由贵司使用HttpRequest工具(e.g. postman/cURL)测试请求、并截图response结果给我方确认
或提交我方API回应的错误response给我方确认 (请确保您的系统有针对400 statusCode做例外处理。部分语言代码需另外处理、才能获取response内容)";
                }

            }
            else if (myText.ContainsWihtoutCase("/promotion_merchant") ||
                     myText.ContainsWihtoutCase("/promotionmerchant") ||
                     myText.ContainsWihtoutCase("/promotion_m"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi, our position is GameProvider. We are not clear about the event mechanism or special integrate mode of your platform. If your merchant is promotion mode or trial mode.
Please ask your [Technical Team] provide the following information:
- ⚠️Please confirm under this mechanism. Do you use a different merchant ID (MerchantID) to enter the game?
- ⚠️Please confirm under this mechanism. Are game accounts and general game accounts separated into two groups?
- Player's game account
- Game Name (or GameID)
- Occurrence time (GMT+8): e.g. 2024/01/01 05:30 ~ 2024/01/01 06:30 GMT+8
- Player IP: e.g. 0.0.0.0
- Screenshots/Error messages/Video recordings

⚠️If you are not sure how to answer. We recommended to seek assistance from your technical team.
{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，我方仅是GameProvider。不清楚您平台的活动机制或特殊对接方式。 若您的商户属于特殊对接活动商户(e.g. promotion、免费试玩、刷分模式)
需要请您的 【技术团队】 协助提供以下资讯
- ⚠️请您确认此机制下。是否使用与一般模式不同的商户ID(MerchantID)进入游戏?
- ⚠️请您确认此机制下。游戏帐号是否与一般游戏帐号是分开两组?
- 玩家游戏帐号
- 游戏名称：
- 发生时间(GMT+8)： e.g. 2024/01/01 05:30 ~ 2024/01/01 06:30 GMT+8
- 玩家IP： e.g. 0.0.0.0
- 截图图片／错误信息／影片录像

⚠️若您不清楚如何回答。建议寻求您团队的技术人员协助
{multiLangLINE}";
                }
            }
            else if (myText.ContainsWihtoutCase("/promotion_issue") ||
                     myText.ContainsWihtoutCase("/promotion_i"))
            {
                //                if (isENMsg)
                //                {
                //                    replyMsg +=
                //@$"Hi, our position is GameProvider. We are not clear about the event mechanism or special integrate mode of your platform. If your merchant is promotion mode or trial mode.
                //Please ask your [Technical Team] provide the following information:
                //- ⚠️Please confirm under this mechanism. Do you use a different merchant ID (MerchantID) to enter the game?
                //- ⚠️Please confirm under this mechanism. Are game accounts and general game accounts separated into two groups?
                //- Player's game account
                //- Game Name (or GameID)
                //- Occurrence time (GMT+8): e.g. 2024/01/01 05:30 ~ 2024/01/01 06:30 GMT+8
                //- Player IP: e.g. 0.0.0.0
                //- Screenshots/Error messages/Video recordings

                //⚠️If you are not sure how to answer. We recommended to seek assistance from your technical team.
                //{multiLangLINE}";
                //                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，若您平台採用活动机制、或特殊对接方式(e.g. promotion、免费试玩、刷分模式)。您的商户可能属于特殊对接活动商户
需要请您的【技术团队】协助提供以下资讯
- ⚠️请您務必确认下。此机制下是否使用与一般模式不同的商户ID(MerchantID)进入游戏?
- ⚠️请您務必确认下。此机制下游戏帐号是否与一般游戏帐号是分开两组? 
- 商戶ID 
- 玩家游戏帐号 
- 注單號(32碼格式) *若玩家反馈有任何派奖未派发。需请您技术单位确认注单号
- 以上所有資訊都是必要提供項目。謝謝您，若您提供错误的玩家资讯。将可能导致问题无法被确认
⚠️若您不清楚如何回答。建议寻求您团队的技术人员协助
{multiLangLINE}";
                }
            }
            else if (myText.ContainsWihtoutCase("/400_reason") || myText.ContainsWihtoutCase("/400reason"))
            {
                this.IsSendMarkdownFormat = true;

                if (isENMsg)
                {
                    replyMsg +=
@"The following are common error reasons
- `All API`： Your server's IP is not in the system whitelist. (Does your server has new outbound IP recently?)
- `GameLaunch API`：Your merchant is seamless mode and the callbackUrl has not been setup.
- `GameLaunch API`：The game you select already disable (or not enable for your merchant).
- `Transfer API`： Your agent credit is insufficient. You need contact AM and deposit credit. " + multiLangLINE;
                }

                if (isCNMsg)
                {
                    replyMsg +=
@"以下是常见错误原因
- `所有API` : 您服务器的IP不在系统白名单中(近期是否有新增outboundIP?)
- `游戏启动 API` : 您的商户是无缝钱包模式，尚未配置callbackUrl
- `游戏启动 API` : 您选择的游戏已关闭、或未开放配置给您的商户
- `钱包转帐 API` : 您的代理额度不足。尚未加值credit";
                }
            }
            else if (myText.ContainsWihtoutCase("/400_s") || myText.ContainsWihtoutCase("/400s") || myText.ContainsWihtoutCase("/400sample") ||
                     myText.ContainsWihtoutCase("/400_sample") || myText.ContainsWihtoutCase("/400sp"))
            {

                this.IsSendMarkdownFormat = true;

                string extraLine = isTG ? "" : NEW_LINE;

                if (isENMsg)
                {
                    replyMsg +=
@"※ The response sample like this： " + extraLine +
@"``` { 'code':403610,'message':'The game you selected is currently set to disable.','data':{'DebugExtraInfo':' 175001 ／ 2024-01-01 23:30:22 (GMT+8)'}}  ```" + NEW_LINE +
@"Hi team. our api always response error message when 400 exception (above json format). We need your tech-team help the bellow (choose one).
🔍 Please test same reqeust by postman/cURL and screenshot result to us. (Picture)
🔍 (or) Provide the response content to us (response message from our API)

⚠️Please ensure that your code has exception handling for 400 statusCode. Some language codes need additional handle to obtain the response content." + multiLangLINE;
                }

                if (isCNMsg)
                {
                    replyMsg +=
@"※ 回应格式示例：" + extraLine +
@"```  {'code':403610,'message':'您选择的游戏已关闭.','data':{'DebugExtraInfo':' 175001 ／ 2024-01-01 23:30:22 (GMT+8)'}} ```" + NEW_LINE +
@"您好，我方API在400 exception必定会返回错误讯息 (上方json格式)，需请您的技术单位提供以下资讯 (择一)
🔍 請您的技术团队透过request工具 (e.g. postman/cURL)发送相同参数请求、并提交截图內容给我方确认
🔍 (或) 将我方API回应内容提供给我们

⚠️ 请确保您的系统有针对400 statusCode做例外处理。部分语言代码需另外处理、才能获取400案例的response内容";
                }
            }
            else if (myText.ContainsWihtoutCase("/400") || myText.ContainsWihtoutCase("/400error") || myText.ContainsWihtoutCase("/400_error"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi team. our api always response error message when exception (status 400/403). Please contact your tech-team provide those information. We need those to find issue. {NEW_LINE} 
- MerchantID (6 digit number e.g. 175001)
- API RequestUrl & parameters (include api domain)
- request payload/body (HTTP POST)
- http response text (or 403/429 error msg)
- RequestTime
- You can also test a sample by postman/cURL and screenshot it.

(*⚠️ If you don't know how to get those. you may need ask your tech team.)" + multiLangLINE;
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，我方API在错误时会有固定回应错误讯息(e.g. 400/403/429 statusCode) 需請您的技術單位提供调用API接口时的资讯、帮助技术确认问题 {NEW_LINE}
- 您的商户号 (6个数字格式 e.g. 175001)
- 调用API网址 & 参数 (含Http域名)
- 调用参数(post payload)
- Http回应 (或错误讯息)
- 调用时间
- 您也可以透过request工具测试请求、并截图结果给我方确认 (e.g. postman/cURL)

(*⚠️ 您可能需要请技术单位协助提供)";
                }
            }
            else if (myText.ContainsWihtoutCase("/req_info") || myText.ContainsWihtoutCase("/req_info"))
            {
                if (isENMsg)
                {
                    replyMsg +=
@$"Hi team, in order to help our team confirm issue. Please your tech-team provide those information.
- API RequestUrl (include API domain)
- Query string (HTTP GET)
- Request payload/body (HTTP POST)
- Http response text (or error msg)
- RequestTime
- You can also test a sample by postman/cURL and screenshot it.

(*⚠️ If you don't know how to get those. you may need ask your tech team.)" + multiLangLINE;
                }

                if (isCNMsg)
                {
                    replyMsg +=
@$"您好，為了帮助我方技术确认问题、需請您的技術團隊提供调用API接口时的资讯、 {NEW_LINE}
- 调用API网址 (含Http域名)
- 网址参数 (HttpGET)
- 调用参数post payload (HttpPOST)
- Http回应 (或错误讯息)
- 调用时间
- 您也可以透过request工具测试请求、并截图结果给我方确认 (e.g. postman/cURL)

(*⚠️ 您可能需要请技术单位协助提供)";
                }
            }
            else if (myText.ContainsWihtoutCase("/game_list") || myText.ContainsWihtoutCase("/gamelist"))
            {
                string extraPrefix = string.Empty;
                string downloadLink = AppHelper.GenerateGameListDownloadUrl();

                if (isTG)
                {
                    using (var client = new WebClient())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var stream = client.OpenRead(downloadLink);
                            stream.CopyTo(memoryStream);
                            myModel.FileBytes = memoryStream.ToArray();
                            myModel.FileName = $"{brandName_short.ToUpper()}_GameList_" + DateTimeHelper.GetCurrentTimeYYYYMMDD(8) + ".xlsx";
                        }
                    }
                }
                else
                {
                    extraPrefix = $"Download：{downloadLink}{NEW_LINE}{NEW_LINE}";
                }

                replyMsg += $"{extraPrefix}";
                replyMsg += isENMsg ? $"Hi team , You can also download the newest game list here.{NEW_LINE}【BO】>【Other】>【Resource download】.You can also fetch game-list via 4.6.1 API.{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，您也可以于后台下载最新的游戏清单{NEW_LINE}【后台】>【其他设置】>【资源下载】。您也可透过 4.6.1 games API 获取游戏清单" : "";
            }
            else if (myText.ContainsWihtoutCase("/pgslogo") || myText.ContainsWihtoutCase("/pgs_logo"))
            {
                if (IsBWGBrand())
                {
                    replyMsg = $"Hi team , You can contact our AM to get logo.{NEW_LINE}";
                }
                else
                {
                    replyMsg = $"https://example.invalid/target-PEGASUS-LOGO" + NEW_LINE + $"Hi team , You can download the logo here.{NEW_LINE}";
                }
            }
            else if (myText.ContainsWihtoutCase("/明天") || myText.ContainsWihtoutCase("/明日"))
            {
                replyMsg = $"※此問題明日會有其他人回覆，若客戶再次詢問、下方選擇合適訊息回復即可" + NEW_LINE +
                           $"Hi, already report issue to game-team. We will reply you later. thank you." + NEW_LINE +
                           $"Hi, this issue need game-team confirm. need more time to check. We will reply you later. " + NEW_LINE +
                           $"您好，已回报技术团队。会尽快回覆您。" + NEW_LINE +
                           $"您好，此问题需要游戏团队确认，会尽快回复您" + NEW_LINE;
            }
            else if (myText.ContainsWihtoutCase("/asap"))
            {
                replyMsg = $"※此問題需時間確認，若客戶再次詢問、下方選擇合適訊息回復即可" + NEW_LINE +
                           $"Hi, please wait." + NEW_LINE +
                           $"Hi already report issue to game-team. We will reply you ASAP. thank you." + NEW_LINE +
                           $"We are still waiting for tech-team reply, we will get back to you ASAP. thank you." + NEW_LINE +
                           $"Hi, this issue need game-team confirm. need more time to check. We will reply you ASAP. " + NEW_LINE +
                           $"Hi team, regarding the issue, tech-team still confirming. It will take some time to check issue. sorry for inconvenience." + multiLangLINE +
                           $"您好，请稍后" + NEW_LINE +
                           $"您好，已回报技术团队。会尽快回覆您。" + NEW_LINE +
                           $"您好，关于此问题仍在确认中。技术团队需要一段时间才能确认完成。造成貴司不便请见谅" + NEW_LINE +
                           $"您好，此问题需要游戏团队确认，会尽快回复您" + NEW_LINE +
                           $"您好，目前技术团队还在确认中，我们会尽快回复您" + NEW_LINE;
            }
            else if (myText.ContainsWihtoutCase("/api_document") || myText.ContainsWihtoutCase("/apidocument") || myText.ContainsWihtoutCase("/api_doc"))
            {
                if (isTG)
                {
                    var splitParameters = myText.Split(" ")
                         .Where(x => x.IsNotEmptyTextNotNull())
                         .Where(x => !x.Contains("document"))
                         .Select(x => x.Trim())
                         .ToList();

                    if (splitParameters.IsNotEmptyNull())
                    {
                        bool isAnyTF = splitParameters.Any(text => text.ContainsWihtoutCase("tf"));
                        bool isAnySW = splitParameters.Any(text => text.ContainsWihtoutCase("sw"));

                        if (isAnyTF || isAnySW)
                        {
                            var walletMode = isAnySW ? MerchantWalletModeEnum.SingleWalletMode : MerchantWalletModeEnum.TransferMode;

                            //檔案上傳較花時間 因此採用非同步
                            Task.Factory.StartNew(() =>
                            {
                                SendApiDocumentToChannel(myChatID, walletMode);
                            });
                        }
                    }
                }

                replyMsg += isENMsg ? $"Hi team , You can download the API Document here.{NEW_LINE}【BO】>【Other】>【Resource download】{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，您可以于后台下载API文档资源{NEW_LINE}【后台】>【其他设置】>【资源下载】{NEW_LINE}" : "";
            }
            else if (myText.ContainsWihtoutCase("/download") || myText.ContainsWihtoutCase("/document"))
            {
                if (isENMsg)
                {
                    replyMsg += $"Hi team , You can download the resource here.{NEW_LINE}【BO】>【Other】>【Resource download】{NEW_LINE}" +
                                $"- newest api document (pdf file){NEW_LINE}- newest game list(xlsx){NEW_LINE}- BO operate guide(pdf file){multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += $"您好，您可以于后台下载文档资源{NEW_LINE}【后台】>【其他设置】>【资源下载】{NEW_LINE}" +
                                $"- 最新的游戏清单(xlsx){NEW_LINE}- 最新的API对接文档(PDF){NEW_LINE}- 后台操作手册 (PDF)"; ;
                }
            }
            else if (myText.ContainsWihtoutCase("/beta_coin") || myText.ContainsWihtoutCase("/betacoin"))
            {
                if (isENMsg)
                {
                    replyMsg += @$"
⚠️*in our Dev(Staging) environment. We only support one currency 'BETA coin'.
You may notice min/max bet amount value is large. but you can ignore it in this environment.
Please validate min/max amount in production environment. {multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += @$"
(⚠️环境限制) 测试环境不分币别。统一使用测试币(BETA Coin)
您可能会注意到最低/最大投注金额 数值很大。但您可以忽略此问题
建议您在正式环境阶段再进行验证 (最低/最大投注金额)";
                }
            }
            else if (myText.ContainsWihtoutCase("/timezone"))
            {
                replyMsg += isENMsg ? $"in our BO. you can change timezone on top-right side.{NEW_LINE}in our API. all datetime value is UTC+0. (Include request and response parameters){NEW_LINE}For example, #3.1.1 game-records API. If you want to query 2024-01-02 00:00:00 - 2024-01-03 00:00:00 (GMT+8). you need pass startTime=2024-01-01 16:00:00&endTime=2024-01-02 16:00:00 {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"在我们后台中。您可以于画面右上方调整时区。{NEW_LINE}在我们API中，所有时间栏位都是UTC+0时间 (包含传入与返回参数) {NEW_LINE}举例来说 3.1.1 game-records API 若您想查询2024-01-02 00:00:00 - 2024-01-03 00:00:00 (GMT+8) 的纪录。 则需要传递这样的时间参数 startTime=2024-01-01 16:00:00&endTime=2024-01-02 16:00:00" : "";
            }
            else if (myText.ContainsWihtoutCase("/demo_mode"))
            {
                IsSendMarkdownFormat = true;
                replyMsg += isENMsg ? $"Hi, when you request game launch API (2.1.1／2.1.2) , you can pass parameter `isTrialModeV2=true` . You can specify to enter the game in trial mode (accounting will not be calculated). For more detail, you can refer to API document. {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，调用game launch API时 (2.1.1／2.1.2)，您可以指定带入此参数 `isTrialModeV2=true` 。即可指定用试玩模式进入游戏 (遊戲將不计算帐务)。您可以参考API文件中的说明" : "";
            }
            else if (myText.ContainsWihtoutCase("/fish_error") || myText.ContainsWihtoutCase("/fish_err"))
            {
                replyMsg = @$"
Player need to note that the game will cause invalid-bets /unsuccessful-bets under the following case:
- Player's network is abnormal, stuck and disconnected.
- Player balance is insufficient to fire cannon.
- Player's authentication session expired.
- (or)You are seeing the rewards of other players in the same room.

请玩家注意，游戏将在以下情况会有投注失效/投注不成功的状况
- 玩家网路异常、卡顿、断线
- 玩家余额不足、发射炮弹失敗
- 玩家身分令牌失效
- (或)玩家看到的是同房间其他玩家的擊殺派奖
";
            }
            else if (myText.ContainsWihtoutCase("/new_callback") || myText.ContainsWihtoutCase("/callback_edit") || myText.ContainsWihtoutCase("/endpoint_edit") || myText.ContainsWihtoutCase("/callbackurl_edit"))
            {
                replyMsg += isENMsg ? $"Hi , seamless merchant need to setup callback url. (Operator need setup themself, by our security policy). You can go to 【BO】>【AgentManagement】>【Agent List】>　【advance-setting】 to view/edit the callback url. (seamless mode) {multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，您好，无缝钱包模式需要完成callback url设置才能正常启动游戏 (基於安全政策考量，代理需要自行做設定)。您可以登入【后台】>【代理管理】>【代理管理】>【进阶设置】 查看或修改 callback 链结设置" : "";
            }
            else if (myText.ContainsWihtoutCase("/callback_200") || myText.ContainsWihtoutCase("/callback_success"))
            {
                if (isENMsg)
                {
                    replyMsg += @$"
Hi, after checking logs (see above log). 
Your merchant is seamless mode. Our system has already correctly callback to your API. (e.g. settle/cancel)

Please contact your technical team, and confirm by your side. Player wallet operations are all performed via your system side.{multiLangLINE}";
                }

                if (isCNMsg)
                {
                    replyMsg += @$"
您好，經過我司技術查看Log日誌 (參考上方)
貴司是採用無縫錢包(Seamless)對接。我方有正確callback通知貴司API了

後續需由你們技術自行確認喔。玩家錢包操作都是你們系統API中進行的";
                }
            }
            else if (myText.ContainsWihtoutCase("/new_ip") || myText.ContainsWihtoutCase("/ip_edit"))
            {
                replyMsg += isENMsg ? $"Hi team , You can go to 【BO】>【Other】>【IPddress whitelist】append new whitelist-IP for your site.{NEW_LINE}btw , dev/staging environment don't need setup whitelist.{multiLangLINE}" : "";
                replyMsg += isCNMsg ? $"您好，若需增加防火墙IP清单。 您可以登入【后台】>【其他设置】 > 【IP白名单管理】添加您的IP{NEW_LINE}*備註: 测试(Dev/Staging)环境不需要设置白名单" : "";
            }

            myModel.ReplyMessage = replyMsg;
            return myModel;
        }

        public void SendApiDocumentToChannel(string chatID, MerchantWalletModeEnum walletMode)
        {
            List<string> targetFileNames = new List<string>();
            targetFileNames.Add(GenerateApiDocumentFileName(LanguageEnum.en_us, walletMode));
            targetFileNames.Add(GenerateApiDocumentFileName(LanguageEnum.zh_cn, walletMode));

            string walletModeText = walletMode == MerchantWalletModeEnum.SingleWalletMode ? "Seamless Mode " : "Transfer Mode ";

            foreach (var fileName in targetFileNames)
            {
                bool isCN = fileName.Contains("中文");

                var param = new TelegramMessageParameter
                {
                    Message = walletModeText + (isCN ? "Document (中文)" : "Document (English)"),
                    TelegramExtraSetting = new TelegramExtraSetting
                    {
                        TargetChatID = chatID,
                        IsCleanPrefixText = true,
                        PostFileName = fileName.Replace("中文", "CN")
                    }
                };

                using (var client = new WebClient())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var stream = client.OpenRead($"{AppHelper.StaticResourceDownloadDomain()}/cs-download/{fileName}?v=" + DateTimeHelper.GetCurrentTimeTimestamp());

                        stream.CopyTo(memoryStream);
                        param.TelegramExtraSetting.PostFileBytes = memoryStream.ToArray();
                    }
                }

                var myFileID = MyTelegramUtility.UploadFileAndFileID(param);
            }


        }


        private (string Name, string ShortName) GenerateBrandName()
        {
            if (IsBWGBrand())
            {
                return ("Big Win Gaming", "BWG");
            }
            else
            {
                return ("Pegasus", "target");
            }
        }



        private bool IsBWGBrand()
        {
            if (SettingHelper.IsBWG)
            {
                return true;
            }

            return false;
        }



        private bool CheckIsPGSNotifyErrorChannel(TgWebhookParsingResult parse)
        {
            if (parse == null || parse.ParsingModel == null)
                return false;

            string channelName = parse.ParsingModel?.message?.chat?.Title;

            return channelName.ContainsWihtoutCase("target-Notify") || channelName.ContainsWihtoutCase("內部用PGS");
        }


        private void UploadFileToTelegramAndGenerateFileID()
        {

        }


        private string GenerateApiDocumentFileName(LanguageEnum lang, MerchantWalletModeEnum walletMode)
        {
            string lang_suffix = lang == LanguageEnum.zh_cn || lang == LanguageEnum.zh_tw ? "中文" : "EN";

            if (walletMode == MerchantWalletModeEnum.SingleWalletMode)
            {
                string version_suffix = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.Json_ApiDocument_SeamlessDocument_Version).SettingValue;
                return $"PGS_SeamlessAPI_{lang_suffix}_{version_suffix}.pdf";
            }
            else
            {
                string version_suffix = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.Json_ApiDocument_TransferDocument_Version).SettingValue;
                return $"PGS_TransferAPI_{lang_suffix}_{version_suffix}.pdf";
            }
        }

        private void SendSuperGroupWarning(TgWebhookParsingResult parse)
        {
            if (parse.ParsingModel?.message == null)
                return;

            var messageObj = parse.ParsingModel.message;

            if (string.IsNullOrWhiteSpace(messageObj.migrate_from_chat_id))
                return;

            string myMsg = $"頻道ID可能有異動: {messageObj.migrate_from_chat_id} {messageObj.chat?.id} {messageObj.chat?.type}";

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_error_pgs_common,
                Message = myMsg
            });
        }

        private void SendErrorMsgToChannel(string msg, TelegramChannelEnum channel = TelegramChannelEnum.pgs_common_error)
        {
            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = channel,
                TelegramExtraSetting = new TelegramExtraSetting
                {
                    IsCleanPrefixText = true,
                }
            });
        }


    }

}
