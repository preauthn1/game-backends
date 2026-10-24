using Microsoft.VisualBasic;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public class TelegramAnnouncementService : BaseAdminSysService, ITelegramAnnouncementComplexService
    {

        public GameListRepository GameListRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public AccountingAdjustmentRuleRepository AccountingAdjustmentRuleRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public IVendorMaintenanceService VendorMaintenanceService { get; set; }


        public int TempValue_TemplateID = 0;



        private ConcurrentQueue<string> DebugQueue = new ConcurrentQueue<string>();

        private static string _eventDescriptionUrl = "https://example.invalid/lobby/?page=eventDescription&eventID=250070&checkCode=cee582afa93e9cea3897ce9855c60790&lang=en-us";

        private static string _demoSiteUrl = "https://example.invalid/try-game?gid={0}";

        public TelegramAnnouncementSubmitViewModel GetDefaultContentByTemplateID(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TemplateID).Rule_NotZeroNotMinus();

            var template = (TelegramAnnouncementTemplateEnum)model.TemplateID;


            if (template == TelegramAnnouncementTemplateEnum.NewGameRelease ||
               template == TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscountCN ||
               template == TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscount)
            {
                model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            }

            string gameIDText = model.GameID > 0 ? model.GameID.ToString() : "";

            string demoLink = string.Format(_demoSiteUrl, gameIDText);

            var linkExpiredTime = DateTimeHelper.GetCurrentTime().AddDays(14);
            string encryptToken = [REDACTED];
            string downloadLink = AppHelper.GenerateGameListDownloadUrl();

            TelegramAnnouncementSubmitViewModel viewModel = new TelegramAnnouncementSubmitViewModel();
            viewModel.TemplateID = model.TemplateID;
            viewModel.Buttons = new List<TelegramAnnouncementButtonItemModel>();

            var gameUrlPrefix = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.LobbyPageGameImageUrl).SettingValue;

            GameListEntity queryGame = null;

            GameInfoQueryTempModel gameExtraInfo = null;

            AccountingAdjustmentRuleEntity matchDiscountRule = null;

            if (model.GameID > 0)
            {
                queryGame = GameListRepository.GetById(model.GameID);

                var gameNameCN = DbMultiLanguageTextHelper.GetGameDisplayName(model.GameID, LanguageEnum.zh_cn);
                var gameNameEN = DbMultiLanguageTextHelper.GetGameDisplayName(model.GameID, LanguageEnum.en_us);

                var setting_RTP = GameListCustomizedSettingRepository.GetGameSettingParseModel(model.GameID, GameListCustomizedSettingKeyEnum.GameInfo_RTP);
                var setting_lines = GameListCustomizedSettingRepository.GetGameSettingParseModel(model.GameID, GameListCustomizedSettingKeyEnum.GameInfo_LinesWays);
                var setting_maxWin = GameListCustomizedSettingRepository.GetGameSettingParseModel(model.GameID, GameListCustomizedSettingKeyEnum.GameInfo_MaximumWin);
                var setting_grid = GameListCustomizedSettingRepository.GetGameSettingParseModel(model.GameID, GameListCustomizedSettingKeyEnum.GameInfo_GridFormat);

                gameExtraInfo = new GameInfoQueryTempModel
                {
                    GameNameCN = gameNameCN,
                    GameNameEN = gameNameEN,
                    Setting_RTP = setting_RTP.SettingValue,
                    Setting_lines = setting_lines.SettingValue,
                    Setting_GridFormat = setting_grid.SettingValue,
                    Setting_maxWin = setting_maxWin.SettingValue,
                };

                viewModel.BasicGameImageUrl = AppHelper.GenerateGameImageUrl(gameUrlPrefix, queryGame.GameCode);

                matchDiscountRule = AccountingAdjustmentRuleRepository.GetByFilter(new QueryAccountingAdjustmentRuleRequestViewModel
                {
                    Types = AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount.ToInteger().ToListCollection(),
                    IsUpCommingOrNotExpired = true
                })
                .Where(x => x.GameIDs.ParseToNumberOrZero() == model.GameID)
                .Where(x=> x.BeginTime.HasValue)
                .FirstOrDefault();      
            }


            switch (template)
            {
                case TelegramAnnouncementTemplateEnum.MaintainmHasDone:
                    viewModel.Content = $"【{GetDisplayBrandName()} maintenance finish ／ 系統维护结束】" + Environment.NewLine+
                                         "Hi Team, Maintenance has done. Thank you." + Environment.NewLine +
                                         "您好，维护已结束。感谢您的等候。"; ;

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = true,
                        LinkUrl = "",
                        Text = "",
                    });

                    break;

                case TelegramAnnouncementTemplateEnum.MaintainmAnouncement:

                    var queryUpcommingItems = VendorMaintenanceService.GetByPagingFilter(userID, new QueryVendorMaintenanceApiRequestParameterModel
                    {
                        OrderByColumn = " Id ",
                        IsASC = false,
                    });

                    var queryUpcomming = queryUpcommingItems
                        .Items
                        .Where(x => x.IsUpComming || x.IsProgressing)
                        .OrderBy(x => x.IsUpComming ? 0 : 999)
                        .FirstOrDefault();

                    //https://example.invalid/public/10579b33-7e93-4a36-b5b3-e16a1e04b628.png 黑色版本
                    //https://example.invalid/public/b43dda8a-55bd-4166-b476-82e4bf937369.png
                    viewModel.BannerImageUrl = "https://example.invalid/public/c43dda8a-55bd-4166-b476-82e4bf937370.png";
                    viewModel.Content = "系統維護公告❌❌❌❌❌❌❌❌";

                    if (queryUpcomming != null)
                    {
                        viewModel.Content = queryUpcomming.TgApiTemplateMessage.Replace("/ION" , "");
                        viewModel.ContentForSkype = queryUpcomming.MaintainMessage;
                    }

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = true,
                        LinkUrl = "",
                        Text = "",
                    });
                    break;

                case TelegramAnnouncementTemplateEnum.NewGameRelease:
                    viewModel.BannerImageUrl = AppHelper.GenerateGameImageUrl(gameUrlPrefix, queryGame.GameCode);

                    #region 預設文案 (ps. 前面不要亂補空白)

                    viewModel.Content =
$@"【New Game Release Notice】
Our newest game '{gameExtraInfo.GameNameEN}' has officially launched. Please assist in synchronizing it to your website and mobile platform. Thank you!
最新款游戏《{gameExtraInfo.GameNameCN}》已正式上线。诚挚邀请您将此款游戏同步更新到您的网站和行动平台。谢谢

⭐ _Game Information(游戏资讯)_
Game Name: {gameExtraInfo.GameNameEN} ({gameExtraInfo.GameNameCN})
Game ID (API): {queryGame.GameID}
Game Format(游戏格式): {gameExtraInfo.Setting_GridFormat}
Payline(线数): {gameExtraInfo.Setting_lines}
RTP: {gameExtraInfo.Setting_RTP}
Maximum Multiplier(最大派奖): {gameExtraInfo.Setting_maxWin}
Game Features(游戏特色): ❌❌❌❌❌";

                    #endregion

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = demoLink,
                        Text = "⭐ Demo Link (试玩网址) ⭐",
                    });

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = downloadLink,
                        Text = "⤵️Download gamelist (最新游戏清单)⤵️",
                    });

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = "",
                        Text = "🏞 Marketing Material (图片素材) 🏞",
                    });

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = true,
                        LinkUrl = "",
                        Text = "📝Game Info (游戏玩法)📝",
                    });
                    break;

                case TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscount:
                    {
                        viewModel.BannerImageUrl = AppHelper.GenerateGameImageUrl(gameUrlPrefix, queryGame.GameCode);

                        var beginDateEN = "日期❌❌❌";
                        var endDateEN = "日期❌❌❌";

                        int eventTotalDays = 0;

                        if (matchDiscountRule != null)
                        {
                            beginDateEN = MultiLanguageTextHelper.GetDateTextEN(matchDiscountRule.BeginTime.Value, 8);
                            endDateEN = MultiLanguageTextHelper.GetDateTextEN(matchDiscountRule.EndTime.Value.AddDays(-1), 8); // -1是因為他們習慣這樣公告
                            eventTotalDays = (int)(matchDiscountRule.EndTime.Value - matchDiscountRule.BeginTime.Value).TotalDays;
                        }

                        #region 預設文案 (ps. 前面不要亂補空白)

                        string week2TextEN = eventTotalDays >= 13 && eventTotalDays <= 15 ? "a 2-week" : "";

                        viewModel.Content =
    $@"【{GetDisplayBrandName()} New Game Release and free royalty fee promotion】
Dear agents, 
On {beginDateEN}, Pegasus will be launching a new game '{gameExtraInfo.GameNameEN}' which also supports Feature Game.

⭐ _Game Information_
Game Name : {gameExtraInfo.GameNameEN}
Game ID (API): {queryGame.GameID}
Game Format: {gameExtraInfo.Setting_GridFormat}
Payline: {gameExtraInfo.Setting_lines}
RTP: {gameExtraInfo.Setting_RTP}
Maximum Multiplier Jackpot: {gameExtraInfo.Setting_maxWin}
Game Features: ❌❌❌❌❌❌

🎁 _Free Royalty Fee Promotion_
Those who participate in the event will enjoy {week2TextEN} ({beginDateEN} - {endDateEN}) free royalty fee promotion (subject to terms and conditions). Pegasus extends a sincere invitation to all agents to participate enthusiastically in the new game promotion. 

The rules for participating in the event are as follows:
- Place the '{gameExtraInfo.GameNameEN}' game banner at the top.
- If you willing to participate. Please apply with our AM 

Thank you for your cooperation and continued support!";

                        #endregion

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = demoLink,
                            Text = "⭐ Demo Link (试玩网址) ⭐",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = downloadLink,
                            Text = "⤵️ Download gamelist (最新游戏清单) ⤵️",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = "",
                            Text = "🏞 Marketing Material (图片素材) 🏞",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = true,
                            LinkUrl = "",
                            Text = "📝Game Info (游戏玩法)📝",
                        });
                        break;
                    }
                    

                case TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscountCN:
                    {
                        viewModel.BannerImageUrl = AppHelper.GenerateGameImageUrl(gameUrlPrefix, queryGame.GameCode);

                        var beginDateCN = "日期❌❌❌";
                        var endDateCN = "日期❌❌❌";

                        int eventTotalDays = 0;

                        if (matchDiscountRule != null)
                        {
                            DateTime beginTW = matchDiscountRule.BeginTime.Value.AddHours(8);
                            DateTime endTW = matchDiscountRule.EndTime.Value.AddHours(8).AddDays(-1); //-1是因為他們公告習慣這樣使用

                            beginDateCN = $"{beginTW.Year}年{beginTW.Month}月{beginTW.Day}日";
                            endDateCN = $"{endTW.Year}年{endTW.Month}月{endTW.Day}日";

                            eventTotalDays = (int)(matchDiscountRule.EndTime.Value - matchDiscountRule.BeginTime.Value).TotalDays;
                        }

                        string week2TextCN = eventTotalDays >= 13 && eventTotalDays <= 15 ? "为期2个星期" : "";

                        #region 預設文案 (ps. 前面不要亂補空白)

                        viewModel.Content =
    $@"【{GetDisplayBrandName()} 新游戏通知 & 代理优惠活动】
尊敬的代理们，在{beginDateCN}, Pegasus 推出全新游戏 '{gameExtraInfo.GameNameCN}'，并同时有盘口免费的活动邀请您参与(见下方)。

⭐ _游戏资讯_
游戏名称 : {gameExtraInfo.GameNameCN}
游戏ID (API): {queryGame.GameID}
游戏格式: {gameExtraInfo.Setting_GridFormat}
线数: {gameExtraInfo.Setting_lines}
RTP: {gameExtraInfo.Setting_RTP}
最大乘数奖金: {gameExtraInfo.Setting_maxWin}
游戏特色: ❌❌❌❌❌❌

🎁 _盘口免费资讯_
参加活动将享有{week2TextCN} ({beginDateCN} - {endDateCN}) 盘口免费福利 (须符合条规) (参与活动规则如下). 

代理需符合以下活动规则:
- 把 '{gameExtraInfo.GameNameCN}' 游戏 Banner 排上轮放广告中.
- 若您有意愿参加。请跟我们AM申请登记

Pegasus公司诚意邀请代理们踊跃参与新游戏活动!";

                        #endregion

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = demoLink,
                            Text = "⭐ 试玩网址 ⭐",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = downloadLink,
                            Text = "⤵️ 最新游戏清单 ⤵️",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = false,
                            LinkUrl = "",
                            Text = "🏞 图片素材 🏞",
                        });

                        viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                        {
                            IsDisabled = true,
                            LinkUrl = "",
                            Text = "📝 游戏玩法 📝",
                        });
                        break;
                    }
                    

                case TelegramAnnouncementTemplateEnum.UnlimitCashDrop:

                    var nowTW = DateTimeHelper.GetCurrentTime(8);
                    var beginMonth = new DateTime(nowTW.Year, nowTW.Month, 1).AddMonths(1);
                    var endMonth = beginMonth.AddMonths(1);

                    var beginTextEN = MultiLanguageTextHelper.GetDateTextEN(beginMonth, 0);
                    var endTextEN = MultiLanguageTextHelper.GetDateTextEN(beginMonth.AddMonths(1), 0);

                    viewModel.BannerImageUrl = "";
                    viewModel.Content =
$@"(❌❌❌看完請刪掉-本次文案預設起始結束日為: {beginMonth.Month}月 - {endMonth.Month}月 若有誤請調整❌❌❌)
【{GetDisplayBrandName()} Event Announcement】
Dear partners:
{GetDisplayBrandName()} have an upcoming event, 'Unlimited Cashdrop,' taking place from {beginTextEN} to {endTextEN}.

We would like your assistance in promoting this event on the homepage of your website. Thank you very much. 《*MYR only》

尊敬的合作伙伴：
{GetDisplayBrandName()} 將於 {beginMonth.Year}年{beginMonth.Month}月1日 至 {endMonth.Year}年{endMonth.Month}月1日期间举办一场无限现金掉落的活动。我们希望能够在贵网站的首页上宣传此活动。非常感谢您的协助。 MYR 市场限定";

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = "",
                        Text = "🏞Marketing Material (图片素材)🏞",
                    });

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = _eventDescriptionUrl,
                        Text = "🧧 Event Rule (活动规则说明) 🧧",
                    });

                    viewModel.Buttons.Add(new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = true,
                        LinkUrl = "",
                        Text = "",
                    });
                    break;
            }

            return viewModel;
        }



        public CRUDResultModel SendTGNotifyByAPI(int userID, TelegramAnnouncementSubmitViewModel model)
        {
            model.RuleFor(x => x.SendTargetsByTagID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TemplateID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Content).Rule_NotEmptyText();

            TempValue_TemplateID = model.TemplateID;

            DateTime process_beginTime = DateTimeHelper.GetCurrentTime();

            if (string.IsNullOrWhiteSpace(model.BannerImageUrl))
            {
                if(model.TemplateID == (int)TelegramAnnouncementTemplateEnum.MaintainmHasDone)
                {
                    //部分訊息可以不強制圖片
                }
                else
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("此模板 已限制一定要搭配一張主視覺圖片"));
                }
            }

            if(model.Content.Contains("❌"))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("內文禁止包含 ❌❌ emoji符號。"));
            }

            if (SettingHelper.PromoteBrandTypeEnum == PromoteBrandTypeEnum.target && model.Content.ContainsWihtoutCase("BWG"))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("內文禁止包含 BWG 關鍵字"));
            }

            if (model.Buttons.IsNotEmptyNull())
            {
                foreach (var item in model.Buttons.Where(x => !x.IsDisabled))
                {
                    if (string.IsNullOrWhiteSpace(item.Text))
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, "按鈕的text文字為空(若不需要使用請勾選禁用)");
                    }

                    if (string.IsNullOrWhiteSpace(item.LinkUrl))
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, "按鈕的LinkUrl網址為空(若不需要使用請勾選禁用)");
                    }

                    if (!RegexRuleHelper.IsUrlFormat(item.LinkUrl))
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, "按鈕的LinkUrl格式不正確");
                    }
                }
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.SendTgAnnouncement, model.SendTargetsByTagID);
            WriteOperateLog(log);

            var buttons = new List<List<TGApiButtonList>>();

            foreach (var item in model.Buttons.Where(x => !x.IsDisabled))
            {
                bool isDemoLink = item.LinkUrl.ContainsWihtoutCase("//demo.");

                var myButton = new TGApiButtonList
                {
                    text = item.Text,
                    url = item.LinkUrl,
                };

                buttons.Add(new List<TGApiButtonList>
                {
                     myButton
                });
            }
                      
            var queryChannels = GetChannelsByTag(model.SendTargetsByTagID);

            if(model.ExcludeChannelIDs.IsNotEmptyNull())
            {
                queryChannels = queryChannels
                    .Where(x => !model.ExcludeChannelIDs.Contains(x.IMChannelID))
                    .ToList();
            }

            bool isNotMaintainAnnounce = model.TemplateID != (int)TelegramAnnouncementTemplateEnum.MaintainmAnouncement;
            bool isMaintainAllOrAGG = model.Content.ContainsWihtoutCase("全游戏") || model.Content.ContainsWihtoutCase("29000"); //e.g. 只維護jili就不要發通知

            //若非'全遊戲維護'公告訊息 可能要排除特定頻道
            if (isNotMaintainAnnounce || !isMaintainAllOrAGG)
            {
                List<int> skipChannels = GetChannelsByTag((int)GeneralTagEnum.SendMaintainMsgOnly , false)
                    .Select(x => x.IMChannelID)
                    .ToList();

                //排除定義好的頻道
                queryChannels = queryChannels
                    .Where(x => !skipChannels.Contains(x.IMChannelID))
                    .ToList();
            }

            var crudResult = new CRUDResultModel();
            crudResult.ResultListText = new List<string>();

            if (SettingHelper.IsMSTest)
                return crudResult;

            // 使用正则表达式查找包含在 _hello_ 中的文本
            string pattern = @"_(.*?)_";

            string tgContent = Regex.Replace(model.Content, pattern, match =>
            {
                return $"<ins>{match.Groups[1].Value}</ins>";
            });

            tgContent = tgContent.Replace("_", "");
            model.Content = model.Content.Replace("_", "");

            List<Task> tasks = new List<Task>();

            //清空還原
            DebugQueue.Clear();

            int typeOfSkype = IMChannelTypeEnum.Skype.ToInteger();
            int typeOfTG = IMChannelTypeEnum.TG.ToInteger();

            var channels_TG = queryChannels.Where(item => item.Type == typeOfTG).ToList();
            var channels_SK = queryChannels.Where(item => item.Type == typeOfSkype).ToList();

            var splitTGArrayToSmallSize_TG = channels_TG.SplitListBySeperateSize(3);
            var splitTGArrayToSmallSize_Skype = channels_SK.SplitListBySeperateSize(3);

            DateTime sendBeginTime = DateTimeHelper.GetCurrentTime();

            //執行緒1. TG (切成三等份)
            foreach (var smallSizeTGChannels in splitTGArrayToSmallSize_TG)
            {
                tasks.Add(Task.Run(() =>
                {
                    int loopIndex = 1;

                    foreach (var item in (smallSizeTGChannels ?? new List<IMChannelEntity>()))
                    {
                        loopIndex++;
                        SendTGMessageByAPI(model, buttons, tgContent, item , loopIndex);
                    }
                }));
            }

            //執行緒2. Skype
            foreach (var smallSizeChannels in splitTGArrayToSmallSize_Skype)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var item in (smallSizeChannels ?? new List<IMChannelEntity>()))
                    {
                        SendSkypeMessageByAPI(model, item);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray(), 1000 * 300);

            if (model.TemplateID == (int)TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscount ||
                model.TemplateID == (int)TelegramAnnouncementTemplateEnum.UnlimitCashDrop ||
                model.TemplateID == (int)TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscountCN)
            {
                DebugQueue.Enqueue($"請注意是否 英文群/中文群 都有發布通知");
            }

            DateTime process_endTime = DateTimeHelper.GetCurrentTime();

            int totalSeconds = ((int)(process_endTime - process_beginTime).TotalSeconds);

            if (SettingHelper.IsProduction && channels_TG.Count() >= 5)
            {
                if (model.TemplateID == (int)TelegramAnnouncementTemplateEnum.MaintainmAnouncement ||
                    model.TemplateID == (int)TelegramAnnouncementTemplateEnum.MaintainmHasDone)
                {
                    string typeName = "維護公告";

                    if(model.TemplateID == (int)TelegramAnnouncementTemplateEnum.MaintainmHasDone)
                    {
                        typeName = "維護結束公告";
                    }

                    //string NEW_LINE = Environment.NewLine;
                    //string msg1 = $"已於TG/Skype群組發送 《維護公告》《{GetDisplayBrandName()} Maintenance notice》{NEW_LINE}" +
                    //             $"若公告內容為 系統維護／臨時維護 則需要協助確認以下{NEW_LINE}" +
                    //             $"⚠️於維護結束15分鐘後與我方確認維護是否結束{NEW_LINE}";

                    //MySkypeUtility.SendMessage(new SkypeMessageParameter
                    //{
                    //    Message = msg1,
                    //    TargetConversationID = SystemConstant.Skype_BotFrameworkAPI_PGSInternalChannelName,
                    //});


                    //List<string> extraChannls = new List<string>
                    //{
                    //    "API ION SLOT X SINAR INTEGRATION GROUP",
                    //    "Pegasus X DK",
                    //    "ATG - IONSLOT Integration",
                    //    "LotteryTogel - Komunitas4D x ION",
                    //};

                    string NEW_LINE = Environment.NewLine;
                    string extraMsgForCS = $" @pgscs01 @bya_kko @Chelsea_Chang @Andy {NEW_LINE}你好，請協助將{typeName}  手動發給Teams還有運行中的客戶";

                    var queryManualChannels = GetChannelsByTag(GeneralTagEnum.MustManualSendToTeams.ToInteger(), isThrowNotFoundException: false);
                   
                    if(queryManualChannels.IsNotEmptyNull())
                    {
                        List<string> channels_orderBy = queryManualChannels
                            .Select(x => $"{x.DisplayName}".Replace("[", "［").Replace("]", "］")) //TG API貌似會取代特殊符號
                            .OrderBy(x => x)
                            .ToList();

                        extraMsgForCS += Environment.NewLine + string.Join(NEW_LINE, channels_orderBy) + NEW_LINE + NEW_LINE;
                        extraMsgForCS += @"⚠️若是ION Slots客戶請使用以下文案" + NEW_LINE;
                        extraMsgForCS += ReplacePGSKeywordToOtherText(model.Content) + NEW_LINE;
                    }

                    MyTelegramUtility.SendMessage(new TelegramMessageParameter
                    {
                        TelegramExtraSetting = new TelegramExtraSetting
                        {
                            IsCleanPrefixText = true
                        },
                        Message = extraMsgForCS,
                        TelegramChannelEnum = TelegramChannelEnum.internal_CS_cowork
                    });
                }
            }

            if(totalSeconds > 30)
            {
                SendMessageToMultipleChannel($"發送公告，本次花費時間 {totalSeconds} 秒", TelegramChannelEnum.pgs_adminSys_api);
            }

            DebugQueue.Enqueue($"發送時間: {process_beginTime.ToString("MM-dd HH:mm:ss")} - {process_endTime.ToString("MM-dd HH:mm:ss")} ");
            DebugQueue.Enqueue($"※若有異常 建議查看 [EazyTempValue] WHERE Type1 = {EazyTempValueTypeEnum.SendTGNotify.ToInteger()}　");
            DebugQueue.Enqueue($"※若有錯誤，會發送至Slack頻道 #n_error_pgs_common　");



            //n_error_pgs_common
            crudResult.ResultListText = DebugQueue.Select(x => x).OrderBy(x=> x).ToList();

            return crudResult;
        }

     

        private void SendTGMessageByAPI(TelegramAnnouncementSubmitViewModel model, List<List<TGApiButtonList>> buttons, string tgContent, IMChannelEntity item , int loopIndex = 1)
        {
            if (string.IsNullOrWhiteSpace(item.ThirdPartyChatID) || item.ThirdPartyChatID.Length <= 5)
            {
                DebugQueue.Enqueue($"{item.IMChannelID}｜{item.DisplayName} 《未配置頻道ID》");
                return;
            }

            string channelName = item.DisplayName;

            if (CommonLogicHelper.HasAvatarKeyword(channelName))
            {
                tgContent = ReplacePGSKeywordToOtherText(tgContent , "Avatar／target");
            }

            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = tgContent,
                TelegramExtraSetting = new TelegramExtraSetting
                {
                    ParseMode = "HTML",
                    LogActionCallback = WriteEazyTempLog,
                    TargetChatID = item.ThirdPartyChatID,
                    IsCleanPrefixText = true,
                    MessageThreadID = item.TGThreadID,
                    ImageUrl = model.BannerImageUrl,
                    MultiRowButtonList = buttons,
                    IsNotifyHttpError = true,
                }
            });

            if(loopIndex % 7 == 0)
            {
                Thread.Sleep(1000); //每個之間延遲毫秒。TG API 每秒最多發送30筆
            }
        }


        public string ReplacePGSKeywordToIONSLOT(string skypeNotifyMsg)
        {
            return ReplacePGSKeywordToOtherText(skypeNotifyMsg);
        }


        public List<IMChannelAnnouncementHistoryViewModel> GetNotifyHistory(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.IMChannelID).Rule_IsNotNullNotZeroNotMinus();

            var matchChannel = IMChannelRepository.GetByIMChannelID(model.IMChannelID.Value);

            var query = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                Memo1 = matchChannel.ThirdPartyChatID,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 180),
                Types = new List<int>
                {
                    EazyTempValueTypeEnum.SendSkypeNotify.ToInteger(),
                    EazyTempValueTypeEnum.SendTGNotify.ToInteger(),
                }
            })
            .OrderByDescending(x => x.CreateTime)
            .ToList();

            var listResult = new List<IMChannelAnnouncementHistoryViewModel>();

            foreach (var item in query)
            {
                var templateID = item.Type2 ?? 0;
                var templateTypeText = "";

                if (templateID > 0)
                {
                    templateTypeText = MultiLanguageTextHelper.GetEnumsCultureText((TelegramAnnouncementTemplateEnum)templateID);
                }

                listResult.Add(new IMChannelAnnouncementHistoryViewModel
                {
                    CreateTime = item.CreateTime,
                    UUID = item.UUID.ToString(),
                    TemplateID = templateID,
                    TemplateTypeText = templateTypeText,
                    DebugResponse = item.DebugInfo,
                    Memo3 = item.Memo3,
                    ResultMessageID = item.Memo2,
                });
            }

            return listResult;
        }



        public void RemoveSingleMessage(int userID , LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.UUID).Rule_NotEmptyText();

            var queryMatch = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                UUIDs = new List<Guid>
                {
                    Guid.Parse(model.UUID)
                },
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 180),
                Types = new List<int>
                {
                    EazyTempValueTypeEnum.SendSkypeNotify.ToInteger(),
                    EazyTempValueTypeEnum.SendTGNotify.ToInteger(),
                }
            })
            .FirstOrDefault();

            if (queryMatch == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            bool isTG = queryMatch.Type1 == EazyTempValueTypeEnum.SendTGNotify.ToInteger();

            if(isTG)
            {
                var res = MyTelegramUtility.DeleteMessage(queryMatch.Memo1, queryMatch.Memo2);

                if(res.ResponseStatusCode >= 400)
                {
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel(res.ResponseText));
                }
            }
            else
            {
                MySkypeUtility.DeleteMessage(queryMatch.Memo1, queryMatch.Memo2);
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.RemoveTgAnnouncement, 0);
            log.Memo = $"{queryMatch.Memo1}／{queryMatch.Memo2}";

            queryMatch.Memo3 = "⚠️訊息已刪除";

            EazyTempValueRepository.Update(queryMatch);

            WriteOperateLog(log);
        }



        public void RemoveLatestMessages(int userID , LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TagID).Rule_NotZeroNotMinus();

            var query = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                Types = new List<int>
                {
                    EazyTempValueTypeEnum.SendTGNotify.ToInteger(),
                    EazyTempValueTypeEnum.SendSkypeNotify.ToInteger()
                },
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-1 * 3),
            });

            if (query.IsEmptyOrNull())
                return;

            model.Size = model.Size <= 0 ? 1 : model.Size;

            var queryChannelIDs = GetChannelsByTag(model.TagID);

            List<Guid> removeUUIDs = new List<Guid>();

            foreach (var setting in queryChannelIDs)
            {
                if (string.IsNullOrWhiteSpace(setting.ThirdPartyChatID))
                    continue;

                var matchMessages = query
                    .Where(x=> x.Memo2.IsNotEmptyTextNotNull())
                    .Where(x => x.Memo1 == setting.ThirdPartyChatID)
                    .OrderByDescending(x=> x.CreateTime)
                    .Take(model.Size)
                    .ToList();

                if (matchMessages.IsEmptyOrNull())
                    continue;

                bool isTG = setting.Type == IMChannelTypeEnum.TG.ToInteger();

                foreach (var item in matchMessages)
                {
                    string channelID = item.Memo1;
                    string messageID = item.Memo2;

                    if (string.IsNullOrWhiteSpace(channelID) ||
                       string.IsNullOrWhiteSpace(messageID))
                    {
                        continue;
                    }

                    removeUUIDs.Add(item.UUID);

                    if (isTG)
                    {
                        MyTelegramUtility.DeleteMessage(channelID, messageID);
                    }
                    else
                    {
                        MySkypeUtility.DeleteMessage(channelID, messageID);
                    }

                }
            }

            foreach(var eachUUID in removeUUIDs)
            {
                var matchData = query.FirstOrDefault(x => x.UUID == eachUUID);

                if(matchData != null)
                {
                    matchData.Memo3 = "⚠️訊息已刪除"; ;
                    EazyTempValueRepository.Update(matchData);
                }
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.RemoveTgAnnouncement, 0);
            log.DebugInfo = $"共移除{removeUUIDs.Count()}筆訊息";

            WriteOperateLog(log);


            SendMessageToMultipleChannel(log.DebugInfo, TelegramChannelEnum.pgs_adminSys_api);
        }

        public void SendLog(TelegramAnnouncementSendLogRequestModel model)
        {
            model.RuleFor(x => x.NotifyMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.NotifyContent).Rule_NotEmptyText();

            var query = IMChannelRepository.GetByMerchantID(model.NotifyMerchantID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (string.IsNullOrWhiteSpace(query.ThirdPartyChatID))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted , new ErrorDetailModel("ThirdPartyChatID"));

            string finalMessage = model.NotifyContent;
            finalMessage += $@"⬆️⬆️⬆️
MerchantID: {model.NotifyMerchantID}
您好，以上是我司调用贵司seamless API拿到的结果。需要请贵司技術人員确认异常原因
Hi, the above is our system callback your seamless API. Please report to your tech-team confirm issue.";

            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = finalMessage,
                TelegramExtraSetting = new TelegramExtraSetting
                {
                    TargetChatID = query.ThirdPartyChatID,
                    IsCleanPrefixText = true,
                    IsSkipWhenMSTest = true,
                }
            });
        }


        public void SycnTGBotAndChatID()
        {
            var botToken = [REDACTED];

            var apiResult = APIRequestHelper.RequestApiResult<TGAPIGetUpdateResponse>(new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"bot{botToken}/getUpdates",
                Method = HttpMethodType.HttpGet,
            });

            var queryTGChannels = IMChannelRepository.GetByFilter(new QueryIMChannelRequestViewModel
            {
                IMChannelTypes = IMChannelTypeEnum.TG.ToInteger().ToListCollection(),
            });

            if (apiResult.ResultObject != null && apiResult.ResultObject.result.IsNotEmptyNull())
            {
                foreach (var item in apiResult.ResultObject.result)
                {
                    string myChatID = item.message?.chat?.id;
                    string myChatChannelName = item.message?.chat?.title;

                    if (string.IsNullOrWhiteSpace(myChatID))
                        continue;

                    double targetScore = 80;

                    var matchChannel = queryTGChannels
                        .Select(x =>
                        {
                            var myScore = x.DisplayName.IsKeywordSimular(myChatChannelName, targetScore);

                            var obj = new
                            {
                                Score = myScore.Score,
                                MyEntity = x,
                            };

                            return obj;
                        })
                        .Where(x=> x.Score >= targetScore)
                        .OrderByDescending(x=> x.Score)
                        .ToList();

                    if (matchChannel.IsNotEmptyNull())
                    {
                        if (matchChannel.Count() >= 2 && matchChannel.All(x=> x.Score < 95))
                        {
                            string names = string.Join("、", matchChannel.Select(x => x.MyEntity.DisplayName).ToList());
                            SendMessageToMultipleChannel($"有兩個以上的匹配相似結果 , {names}");
                        }
                        else
                        {
                            var matchEntity = matchChannel.FirstOrDefault().MyEntity;

                            if (string.IsNullOrWhiteSpace(matchEntity.ThirdPartyChatID))
                            {
                                matchEntity.ThirdPartyChatID = myChatID;

                                IMChannelRepository.Update(matchEntity);

                                SendMessageToMultipleChannel($"{Environment.NewLine}{matchEntity.IMChannelID} {matchEntity.DisplayName} TG頻道已更新chatID: {matchEntity.ThirdPartyChatID}");

                            }
                        }
                    }
                }
            }
        }

        public string ReplacePGSKeywordToOtherText(string skypeNotifyMsg, string newText = "ION SLOT")
        {
            string tempText = "XXXXXXXXXXXX";

            //這邊取代的順序麻煩不要亂調
            skypeNotifyMsg = skypeNotifyMsg.Replace("target maintenance", $"{tempText} maintenance");
            skypeNotifyMsg = skypeNotifyMsg.Replace("target System", $"{tempText} System");
            skypeNotifyMsg = skypeNotifyMsg.Replace("target 系统", $"{tempText} 系统");
            skypeNotifyMsg = skypeNotifyMsg.Replace("PGS系统", $"{tempText} 系统");
            skypeNotifyMsg = skypeNotifyMsg.Replace("target ", $"{tempText} ");
            skypeNotifyMsg = skypeNotifyMsg.Replace("【target", $"【{tempText}");

            skypeNotifyMsg = skypeNotifyMsg.Replace(tempText, newText);
            return skypeNotifyMsg;
        }


        private void SendSkypeMessageByAPI(TelegramAnnouncementSubmitViewModel model, IMChannelEntity item)
        {
            if (string.IsNullOrWhiteSpace(item.ThirdPartyChatID) || item.ThirdPartyChatID.Length <= 5)
            {
                DebugQueue.Enqueue($"{item.IMChannelID}｜{item.DisplayName} 《未配置頻道ID》");
                return;
            }

            string NEW_LINE = Environment.NewLine;
            string skypeNotifyMsg = model.Content + NEW_LINE + NEW_LINE + NEW_LINE;
            skypeNotifyMsg = skypeNotifyMsg.Replace("- ", "． ");

            //2025年 改成Teams後。換行變得怪怪的。改成切割成多個訊息發送
            //2025年 改成Teams後。換行變得怪怪的。改成切割成多個訊息發送
            //2025年 改成Teams後。換行變得怪怪的。改成切割成多個訊息發送
            List<string> teamsVersionExtraMessage = new List<string>();

            var templateType = (TelegramAnnouncementTemplateEnum)model.TemplateID;

            if (templateType == TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscount ||
                templateType == TelegramAnnouncementTemplateEnum.NewGameReleaseAndFeeDiscountCN ||
                templateType == TelegramAnnouncementTemplateEnum.NewGameRelease)
            {
                //skypeNotifyMsg = skypeNotifyMsg + NEW_LINE + "Image (游戏图片)" + NEW_LINE;
                //skypeNotifyMsg = skypeNotifyMsg + model.BasicGameImageUrl + NEW_LINE;

                string myNewLineMsg = "🔥Image (游戏图片)🔥" + model.BasicGameImageUrl;
                teamsVersionExtraMessage.Add(myNewLineMsg);
            }

            foreach (var buttonItem in model.Buttons.Where(x => !x.IsDisabled))
            {
                //skypeNotifyMsg = skypeNotifyMsg + NEW_LINE + buttonItem.Text + NEW_LINE;
                //skypeNotifyMsg = skypeNotifyMsg + buttonItem.LinkUrl + NEW_LINE;

                string myNewLineMsg = buttonItem.Text + Environment.NewLine + buttonItem.LinkUrl;
                teamsVersionExtraMessage.Add(myNewLineMsg);
            }

            string channelName = item.DisplayName ?? "";

            //如果是ION頻道
            if (CommonLogicHelper.HasIONSlotKeyword(channelName))
            {
                skypeNotifyMsg = ReplacePGSKeywordToOtherText(skypeNotifyMsg);
            }

            //如果是F11捕魚
            if (CommonLogicHelper.HasF11Keyword(channelName))
            {
                skypeNotifyMsg = ReplacePGSKeywordToOtherText(skypeNotifyMsg, "F11");
            }

            //如果是Jili專門群
            if (CommonLogicHelper.HasJiliKeyword(channelName))
            {
                skypeNotifyMsg = ReplacePGSKeywordToOtherText(skypeNotifyMsg, "Jili／target");
            }

            var skypeResult = MySkypeUtility.SendMessage(new SkypeMessageParameter
            {
                Message = skypeNotifyMsg,
                TargetConversationID = item.ThirdPartyChatID,
                ExtraSetting = new SkypeExtraSetting
                {
                    LogActionCallback = WriteEazyTempLog_Skype
                }
            });

            Thread.Sleep(1000 * 1);

            foreach(var text in teamsVersionExtraMessage)
            {
                Thread.Sleep(500 * 1);

                MySkypeUtility.SendMessage(new SkypeMessageParameter
                {
                    Message = text,
                    TargetConversationID = item.ThirdPartyChatID,
                });
            }

            if (string.IsNullOrWhiteSpace(skypeResult.id))
            {
                string myMsg = $"{item.IMChannelID}｜{item.DisplayName} 《API發送失敗》";
                DebugQueue.Enqueue(myMsg);

                SendMessageToMultipleChannel(myMsg + $"、【{skypeResult.RawResponseText}】");
            }
        }

        private string GetDisplayBrandName()
        {
            return SettingHelper.PromoteBrandCodeName;
        }

        private void WriteEazyTempLog(IMMessageSendCallbackParameter model)
        {
            var now = DateTimeHelper.GetCurrentTime();

            EazyTempValueRepository.CreateOrQueueRetry(new EazyTempValueEntity
            {
                UUID = Guid.NewGuid(),
                Type1 = EazyTempValueTypeEnum.SendTGNotify.ToInteger(),
                Type2 = TempValue_TemplateID,
                Memo1 = model.ChatID,
                Memo2 = model.MessageID,
                DebugInfo = model.ResponseText,
                CreateTime = now,
            });
        }

        private void WriteEazyTempLog_Skype(IMMessageSendCallbackParameter model)
        {
            var now = DateTimeHelper.GetCurrentTime();

            EazyTempValueRepository.CreateOrQueueRetry(new EazyTempValueEntity
            {
                UUID = Guid.NewGuid(),
                Type1 = EazyTempValueTypeEnum.SendSkypeNotify.ToInteger(),
                Memo1 = model.ChatID,
                Memo2 = model.MessageID,
                DebugInfo = model.ResponseText,
                CreateTime = now,
            });
        }


        private List<IMChannelEntity> GetChannelsByTag(int tagID , bool isThrowNotFoundException = true)
        {
            var queryIMChannels = GeneralTagMappingRepository.GetByFilter(new QueryGeneralTagMappingRequestViewModel
            {
                TagIDs = tagID.ToListCollection()
            });

            if (queryIMChannels.IsEmptyOrNull() && isThrowNotFoundException)
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("此標籤無指派頻道 TagMapping"));

            var channelIDs = queryIMChannels.Select(x => x.TargetID).ToList();

            var querySettings = IMChannelRepository.GetByIMChannelIDs(channelIDs);

            if (querySettings.IsEmptyOrNull() && isThrowNotFoundException)
            {
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("查無頻道 OneToManyCustomizedSetting"));
            }

            return querySettings;
        }


    }

}
