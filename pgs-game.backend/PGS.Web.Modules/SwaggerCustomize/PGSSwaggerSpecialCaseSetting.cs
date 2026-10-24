using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper.HttpContext;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static example.invalid.WebRequestMethods;


namespace target.API.AdminSys
{

    public static class PGSSwaggerSpecialCaseSetting
    {

        static PGSSwaggerSpecialCaseSetting()
        {
            InitialDescription();
        }



        private const string OptionalParameterCN = "(非必填参数)";
        private const string OptionalParameterEN = "(Optional)";


        private static void InitialDescription()
        {

            AddTextToDic("orderByColumn",
              new ApiDescriptionModel("使用哪個欄位排序", ""),
              new ApiDescriptionModel("sort by", ""));

            AddTextToDic("isASC",
              new ApiDescriptionModel("true: 正向排序、false: 反向排序", ""),
              new ApiDescriptionModel("sort direction (ascending、descending)", ""));

            AddTextToDic("signKey",
              new ApiDescriptionModel("加密驗簽，需將Ts以及部分欄位加密，以檢查防止API偽造", ""),
              new ApiDescriptionModel("加密驗簽，需將Ts以及部分欄位加密，以檢查防止API偽造", ""));

            AddTextToDic("ts",
              new ApiDescriptionModel("時間戳記timestamp", ""),
              new ApiDescriptionModel("timestamp", ""));

            AddTextToDic("merchantSecretKey",
              new ApiDescriptionModel("运营商密钥", "`游戏商预先提供`"),
              new ApiDescriptionModel("operator's secret key", "`provides in advance`"));


            AddTextToDic("merchantID",
              new ApiDescriptionModel("运营商編號", "`游戏商预先提供`"),
              new ApiDescriptionModel("operator's API ID", "`provides in advance`"));

            AddTextToDic("playerUniqueID",
              new ApiDescriptionModel("玩家唯一識別碼 (系统ID或唯一值帐号)", "`#帐号格式限制請參考文件說明`"),
              new ApiDescriptionModel("Player’s Unique ID (systemID or Unique Account)", "`#refer account rules in document`"));

            AddTextToDic("startTime",
              new ApiDescriptionModel("查询起始时间 (判断大于等于 >=)  (+0时区) 格式 yyyy-MM-dd HH:mm:ss", ""),
              new ApiDescriptionModel("Start time of this query (greater than or equal to >=) (UTC +0) format: yyyy-MM-dd HH:mm:ss", ""));

            AddTextToDic("endTime",
              new ApiDescriptionModel("查询结束时间 (判断小于 <) (+0时区) 格式 yyyy-MM-dd HH:mm:ss `时间区间不得大于7天` ", ""),
              new ApiDescriptionModel("End time of this query (small than <) (UTC +0) format: yyyy-MM-dd HH:mm:ss ` begin & endTime interval cannot exceed 7 days` ", ""));

            AddTextToDic("pageIndex",
              new ApiDescriptionModel("查询页数", "从1开始"),
              new ApiDescriptionModel("pageIndex", "Start from 1"));

            AddTextToDic("pageSize",
              new ApiDescriptionModel("每页几笔", "限制每次查询10000笔"),
              new ApiDescriptionModel("pageSize", "(maximum: 10000)"));

            AddTextToDic("totalPage",
              new ApiDescriptionModel("总页数", ""),
              new ApiDescriptionModel("total page", ""));

            AddTextToDic("totalCount",
              new ApiDescriptionModel("总笔数 ", ""),
              new ApiDescriptionModel("total count ", ""));

            AddTextToDic("gameID",
              new ApiDescriptionModel("游戏编号", "开放游戏以 4.6.1 API (Get game list)為準"),
              new ApiDescriptionModel("ID of game", "please refer 4.6.1 API (Get game list)"));

            AddTextToDic("roundID",
              new ApiDescriptionModel("游戏局号(棋牌游戏)", ""),
              new ApiDescriptionModel("game round ID (board game)", ""));

            AddTextToDic("currency",
              new ApiDescriptionModel("商户货币", ""),
              new ApiDescriptionModel("currency", ""));

            AddTextToDic("gameName",
              new ApiDescriptionModel("游戏名称", ""),
              new ApiDescriptionModel("game display name", ""));

            AddTextToDic("isExist",
              new ApiDescriptionModel("资料是否已存在", ""),
              new ApiDescriptionModel("If the account exists or not", ""));

            AddTextToDic("status",
              new ApiDescriptionModel("状态", ""),
              new ApiDescriptionModel("status", ""));

            AddTextToDic("freeSpinId",
             new ApiDescriptionModel("freeSpin唯一ID", ""),
             new ApiDescriptionModel("The unique identifier for the FreeSpin campaign.", ""));

            AddTextToDic("enable",
            new ApiDescriptionModel("是否启用", ""),
            new ApiDescriptionModel("enable or not", ""));

            AddTextToDic("name",
            new ApiDescriptionModel("显示名称/识别名称", ""),
            new ApiDescriptionModel("display name", ""));

            AddTextToDic("remark",
            new ApiDescriptionModel("备注信息", ""),
            new ApiDescriptionModel("remark", ""));

            AddTextToDic("budgetId",
            new ApiDescriptionModel("freeSpin budgetId配置ID", ""),
            new ApiDescriptionModel("The unique identifier for the FreeSpin budget.", ""));

            AddTextToDic("remainAmount",
            new ApiDescriptionModel("剩余额度, 若额度不足将无法派发至玩家", ""),
            new ApiDescriptionModel("Budget remaining amount. If the remaining are insufficient, freeSpin cannot be distributed.", ""));

            AddTextToDic("remainRounds",
            new ApiDescriptionModel("剩余局数, 若剩余局數不足将无法派发至玩家", ""),
            new ApiDescriptionModel("Budget remaining rounds. If the remaining are insufficient, freeSpin cannot be distributed. ", ""));



            AddTextToDic("useEndTime",
             new ApiDescriptionModel("使用到期时间", ""),
             new ApiDescriptionModel("Deadline for players to use FreeSpin.", ""));            

            AddTextToDic("gameIDs",
             new ApiDescriptionModel("(array int) 指定要哪几款游戏 (gameID)", ""),
             new ApiDescriptionModel("(array int) Specify which games (gameID).", ""));

            AddTextToDic("gameSummary",
             new ApiDescriptionModel("(array object) 每个物件项目包含一款游戏的统计数据", ""),
             new ApiDescriptionModel("(array object) Each item contains statistics for one game.", ""));

            AddTextToDic("gameSettings",
             new ApiDescriptionModel("(array object) 指定支援的游戏以 & 对应的投注额，包含 gameID & bet 兩個欄位 (⚠️每款游戏可设置的投注额不同)", ""),
             new ApiDescriptionModel("(array object) The specified supported games and betting amounts, Includes 2 fields: gameID and bet. (⚠️The betting amount varies for each game)", ""));

            AddTextToDic("players",
             new ApiDescriptionModel("(array) 指定对哪个玩家帐号生效 ", ""),
             new ApiDescriptionModel("(array) Specify which player account it applies to ", ""));

            AddTextToDic("rounds",
             new ApiDescriptionModel("freespin 可以游玩几局", ""),
             new ApiDescriptionModel("Number of rounds a single player can freespin ", ""));

            AddTextToDic("freeSpinUseStatus",
             new ApiDescriptionModel("freespin 状态 (Default / InProgress / EndProcess / Finish)", ""),
             new ApiDescriptionModel("freespin status (Default / InProgress / EndProcess / Finish)", ""));

            AddTextToDic("gameInfoList",
             new ApiDescriptionModel("(array object) 每一款游戏可设定投注额 ", ""),
             new ApiDescriptionModel("(array object) Each game allows to set the betting amount. ", ""));

            AddTextToDic("lastRounds",
                new ApiDescriptionModel("剩余局数 ", ""),
                new ApiDescriptionModel("Remaining rounds ", ""));

            AddTextToDic("totalRounds",
                new ApiDescriptionModel("总局数 ", ""),
                new ApiDescriptionModel("Total of rounds.  ", ""));

            AddTextToDic("totalPayout",
              new ApiDescriptionModel("总赢分(Payout) ", ""),
              new ApiDescriptionModel("Total of payout", ""));

            AddTextToDic("totalBet",
              new ApiDescriptionModel("总投注 ", ""),
              new ApiDescriptionModel("Total of betAmount  ", ""));

            AddTextToDic("totalUser",
              new ApiDescriptionModel("总玩家人数 ", ""),
              new ApiDescriptionModel("Total of users ", ""));

            AddTextToDic("bet",
              new ApiDescriptionModel("该游戏freespin赠送的投注额" , ""),
              new ApiDescriptionModel("The game's freespin betting amount", ""));

            AddTextToDic("betRanges",
              new ApiDescriptionModel("每款游戏可以设定的投注额 (array decimal)", ""),
              new ApiDescriptionModel("The betting amount can be set for each game (array decimal)", ""));

            AddTextToDic("betTime",
              new ApiDescriptionModel("下注时间", ""),
              new ApiDescriptionModel("bet time", ""));

            AddTextToDic("betAmount",
              new ApiDescriptionModel("投注金额", ""),
              new ApiDescriptionModel("bet amount", ""));

            AddTextToDic("payoutAmount",
              new ApiDescriptionModel("派彩金额", ""),
              new ApiDescriptionModel("payout amount", ""));

            AddTextToDic("validAmount",
              new ApiDescriptionModel("有效流水", ""),
              new ApiDescriptionModel("valid amount", ""));

            AddTextToDic("winLossAmount",
              new ApiDescriptionModel("输赢金额", ""),
              new ApiDescriptionModel("winLoss amount", ""));

            AddTextToDic("taxAmount",
              new ApiDescriptionModel("税收金额", ""),
              new ApiDescriptionModel("tax amount", ""));

            AddTextToDic("jackpotAmount",
              new ApiDescriptionModel("jp彩金 (仅限特定游戏 gameID: 290001~ 299999)", ""),
              new ApiDescriptionModel("jackpot Amount (only for gameID: 290001~ 299999)", ""));

            AddTextToDic("settlementTime",
              new ApiDescriptionModel("结算时间", ""),
              new ApiDescriptionModel("settlement time", ""));

            AddTextToDic("totalWinLossAmount",
              new ApiDescriptionModel("总输嬴", ""),
              new ApiDescriptionModel("Total WinLoss Amount", ""));

            AddTextToDic("totalValidAmount",
              new ApiDescriptionModel("总有效流水", ""),
              new ApiDescriptionModel("Total Valid Amount", ""));

            AddTextToDic("totalPayoutAmount",
              new ApiDescriptionModel("总派彩金额", ""),
              new ApiDescriptionModel("Total Payout Amount", ""));

            AddTextToDic("totalBetAmount",
              new ApiDescriptionModel("总投注金额", ""),
              new ApiDescriptionModel("Total Bet Amount", ""));

            AddTextToDic("totalTaxAmount",
              new ApiDescriptionModel("总税收金额", ""),
              new ApiDescriptionModel("Total Tax Amount", ""));

            AddTextToDic("isIncludeExtraInfo",
              new ApiDescriptionModel("若无需要、建议不带此参数 (部分API可透过此参数要求提供更多信息)", ""),
              new ApiDescriptionModel("include extra information for detail.(only for some specific merchant)", ""));

            AddTextToDic("link",
              new ApiDescriptionModel("网址/链结", ""),
              new ApiDescriptionModel("link url", ""));

            AddTextToDic("supportedOrientation",
              new ApiDescriptionModel("游戏画面的水平方向 (Landscape/Portrait)", ""),
              new ApiDescriptionModel("Game screen orientation (Landscape/Portrait)", ""));

            AddTextToDic("gameCode",
              new ApiDescriptionModel("游戏代码", ""),
              new ApiDescriptionModel("game Code", ""));

            AddTextToDic("imageUrl",
              new ApiDescriptionModel("图片链结", ""),
              new ApiDescriptionModel("image Url", ""));

            AddTextToDic("isAutoCreateNonExistPlayer",
              new ApiDescriptionModel("若指定true，当玩家不存在时会自动创建", ""),
              new ApiDescriptionModel("If tru, the player will be automatically created if it does not exist.", ""));

            AddTextToDic("imageUrl_H",
              new ApiDescriptionModel("图片链结 (横幅样式)", "仅 GameID: 290001~299999 支援此"),
              new ApiDescriptionModel("image Url (Horizontal version)", "Only gameID: 290001~299999 support this"));

            AddTextToDic("userDevice",
              new ApiDescriptionModel($"{OptionalParameterCN} 部分游戏需指定装置类型进入不同装置用的页面", " `PC=1 , H5=2` "),
              new ApiDescriptionModel($"{OptionalParameterEN} Some games need to specify the device type to enter the page for different devices", " `PC=1 , H5=2` "));

            AddTextToDic("totalPromotionBonus",
              new ApiDescriptionModel("活动奖励", ""),
              new ApiDescriptionModel("Promotion Bonus", ""));

            AddTextToDic("tagID",
              new ApiDescriptionModel($"{OptionalParameterCN} 仅供特定客户使用。额外筛选条件", ""),
              new ApiDescriptionModel($"{OptionalParameterEN} For specific merchant only. additional filter", ""));

            AddTextToDic("totalRound",
              new ApiDescriptionModel("总局数／注单数", ""),
              new ApiDescriptionModel("Total Round", ""));

            AddTextToDic("gameType",
              new ApiDescriptionModel("游戏分类", ""),
              new ApiDescriptionModel("game Type", ""));

            AddTextToDic("gameTypeName",
              new ApiDescriptionModel("游戏分类名称", ""),
              new ApiDescriptionModel("game Type Name", ""));

            AddTextToDic("isTrialModeV2",
              new ApiDescriptionModel("使用试玩模式启动游戏(不计算帐务)", ""),
              new ApiDescriptionModel("launch game in trial mode (without billing)", ""));

            AddTextToDic("roomLevelFlagID",
              new ApiDescriptionModel("暫無作用", ""),
              new ApiDescriptionModel("", "*`NOT AVAILABLE FOR NOW`"));

            AddTextToDic("lang",
              new ApiDescriptionModel("语系代码", "(e.g. `zh-cn`、`en-us`)"),
              new ApiDescriptionModel("Language Code", "(e.g. `zh-cn`、`en-us`)"));

            AddTextToDic("amount",
              new ApiDescriptionModel("交易金额", "最小位數到`小数点两位` 转帐金额最低至少0.1"),
              new ApiDescriptionModel("transaction amount", "Minimum Support to `second decimal places` min value:`0.1` "));

            AddTextToDic("sessionToken",
              new ApiDescriptionModel("此token运营商由自行产生。作为玩家每次进入游戏的身分验证令牌，并於运callback API時检查身份合法性(e.g. 5.1.1 API)，需传入长度15~80字串 (建议使用uuid或hash) , 每次帶入的值不得重複", ""),
              new ApiDescriptionModel("This token is generated by operator side. For Operator API identifying players and manage life circle. (e.g. 5.1.1 API), The format should be 15~80 string length(uuid or hash),each value can use only one-times"));

            AddTextToDic("merchantOrderNo",
              new ApiDescriptionModel("运营商自定转账单号 (*需唯一)，长度限制50字元。格式需为英文、数字。特殊符号仅允许 `-` `_`(Dash与底线)", ""),
              new ApiDescriptionModel("Operator’s order ID, should be unique, the limitation is 50 characters. The format must be English and numbers. Symbols are only support `-` `_` (Dash & underline)", ""));

            AddTextToDic("transferType",
              new ApiDescriptionModel("转账类型", ""),
              new ApiDescriptionModel("transfer Type", ""));

            AddTextToDic("isSearchExtraDetail",
              new ApiDescriptionModel($"{OptionalParameterCN} 仅提供部分商户使用、开启查询额外信息 (若设为true。将可能导致查询变慢)", ""),
              new ApiDescriptionModel($"{OptionalParameterEN} Only for few of merchant, specific search extra columns. (might slow down the response time.)", ""));

            AddTextToDic("isFilterByCreateTime",
              new ApiDescriptionModel("使用建立时间来筛选时间 （旧版本使用）", ""),
              new ApiDescriptionModel("Filter create time for data query (old version logic)", ""));

            AddTextToDic("receiveTime",
            new ApiDescriptionModel("领取时间", ""),
            new ApiDescriptionModel("Receive Time", ""));

            AddTextToDic("ExtraInfo",
              new ApiDescriptionModel("此栏位仅对部分商户生效。显示额外资讯", ""),
              new ApiDescriptionModel("Only for few of merchant, response extra information.", ""));

            AddTextToDic("playerID",
              new ApiDescriptionModel("玩家ID", ""),
              new ApiDescriptionModel("player ID", ""));

            AddTextToDic("ExtraLink",
              new ApiDescriptionModel("商户额外指定的跳转链结(仅部分商户支援此)", ""),
              new ApiDescriptionModel("Additional redirect link (only specific merchant support this)", ""));

            AddTextToDic("beforeAmount",
              new ApiDescriptionModel("交易前金额", ""),
              new ApiDescriptionModel("Amount before this transaction", ""));

            AddTextToDic("afterAmount",
              new ApiDescriptionModel("交易后金额", ""),
              new ApiDescriptionModel("Amount after this transaction", ""));

            AddTextToDic("createTime",
              new ApiDescriptionModel("建立时间", ""),
              new ApiDescriptionModel("create Time", ""));

            AddTextToDic("modifyTime",
              new ApiDescriptionModel("修改时间", ""),
              new ApiDescriptionModel("modify Time", ""));

            AddTextToDic("statTime",
            new ApiDescriptionModel("统计日时间", ""),
            new ApiDescriptionModel("statistical time", ""));

            AddTextToDic("pgsTransactionNo",
              new ApiDescriptionModel("[游戏商] 系统转账单号", ""),
              new ApiDescriptionModel("[Game Provider] Transaction number", ""));

            AddTextToDic("merchantTransactionNo",
              new ApiDescriptionModel("[营运商] 转账单号", ""),
              new ApiDescriptionModel("[Operator] Transaction number of transfer", ""));

            AddTextToDic("transactionNo",
              new ApiDescriptionModel("交易单号", ""),
              new ApiDescriptionModel("Transaction number", ""));

            AddTextToDic("AmountWithConvert",
              new ApiDescriptionModel("經過比例轉換的金額", "(玩家游戏内实际看到钱包余额)"),
              new ApiDescriptionModel("Amount With Convert ratio", "(Players actually see balance in the game)"));

            AddTextToDic("backUrl",
              new ApiDescriptionModel($"{OptionalParameterCN} 游戏中返回按钮的返回指定网址，需进行 `urlEncode` 编码", " 最大字数: 1000. *`并非所有游戏都提供返回按钮`"),
              new ApiDescriptionModel($"{OptionalParameterEN} The return url for back button, please make `urlEncode` before send.  maxLength: 1000. ", " `Not all games provide a back button`"));

            AddTextToDic("LiveCasinoLevelID",
              new ApiDescriptionModel($"{OptionalParameterCN} 真人视讯游戏专属参数，指定玩家使用特定的限红配置、需搭配限红对应文件 (默认: VIP000) "),
              new ApiDescriptionModel($"{OptionalParameterEN} For live casino games only , specify the player to use a specific bet limit setting (default: VIP000) "));

            AddTextToDic("IsExchangeToOriginalRate",
              new ApiDescriptionModel($"{OptionalParameterCN} 仅限MYR100 (自动兑换比例模式) 商户使用。指定为true。注单数字将会回传除以100的数值 "),
              new ApiDescriptionModel($"{OptionalParameterEN} Only available to merchants with an MYR100 (auto conversion mode) only. if value= true. The amount value will be returned divided by 100 "));

            AddTextToDic("prizeAmount",
              new ApiDescriptionModel("獎品金額", ""),
              new ApiDescriptionModel("Prize Amount", ""));

            AddTextToDic("drawPrizeRecordID",
              new ApiDescriptionModel("系統派獎ID", ""),
              new ApiDescriptionModel("系統派獎ID", ""));

            AddTextToDic("referenceTransactionNo",
              new ApiDescriptionModel("注單編號", ""),
              new ApiDescriptionModel("Bet WagerID", ""));

            AddTextToDic("referenceTransactionTime",
              new ApiDescriptionModel("注單投注時間", ""),
              new ApiDescriptionModel("Bet Time", ""));

            AddTextToDic("ReferenceBetAmount",
              new ApiDescriptionModel("注單投注金額", ""),
              new ApiDescriptionModel("Bet Amount", ""));


            AddTextToDic("eventID",
              new ApiDescriptionModel("活动ID", ""),
              new ApiDescriptionModel("EventID", ""));

            AddTextToDic("eventName",
              new ApiDescriptionModel("活动名称", ""),
              new ApiDescriptionModel("EventName", ""));

            AddTextToDic("eventPrizeRecordID",
              new ApiDescriptionModel("(已舍弃) 请改用 prizeRecordTransactionNo", ""),
              new ApiDescriptionModel("(Discard) Please use prizeRecordTransactionNo instead", ""));

            AddTextToDic("prizeRecordTransactionNo",
              new ApiDescriptionModel("奖品发放系统单号", ""),
              new ApiDescriptionModel("Event Prize Record ID", ""));

            AddTextToDic("eventPrizeType",
              new ApiDescriptionModel("活动奖品类型(系统定义编号)", ""),
              new ApiDescriptionModel("Event Prize Type", ""));

            AddTextToDic("before",
              new ApiDescriptionModel("更新前", ""),
              new ApiDescriptionModel("Before modified", ""));

            AddTextToDic("after",
              new ApiDescriptionModel("更新后", ""),
              new ApiDescriptionModel("After modified", ""));

            AddTextToDic("eventPrizeTypeText",
              new ApiDescriptionModel("活动奖品类型名称", ""),
              new ApiDescriptionModel("Event Prize Type Name", ""));

            AddTextToDic("quantityOrReferenceRatio",
              new ApiDescriptionModel("发送奖品数量／倍率／金币", ""),
              new ApiDescriptionModel("The amount or quantity", ""));

            AddTextToDic("prizeCashAmount",
              new ApiDescriptionModel("若有赠送金币，请以此栏位数字为主", ""),
              new ApiDescriptionModel("If the prize is coins, please use this field as the cash-amount ", ""));

            AddTextToDic("statusText",
              new ApiDescriptionModel("状态名称", ""),
              new ApiDescriptionModel("Status name", ""));

            AddTextToDic("memo",
              new ApiDescriptionModel("备注信息", ""),
              new ApiDescriptionModel("Memo description", ""));

            AddTextToDic("url",
              new ApiDescriptionModel("网址", ""),
              new ApiDescriptionModel("Url", ""));

            AddTextToDic("IsPrePaidMode",
                 new ApiDescriptionModel("是否使用预付模式 (参考文件说明 #预付模式)", ""),
                 new ApiDescriptionModel("Is prepaid mode (Refer document description #Prepaid mode games)", ""));

            AddTextToDic("IsLaunchAvailable",
                 new ApiDescriptionModel("该款游戏是否允许直接进入启用", ""),
                 new ApiDescriptionModel("Does this game allow direct launch?", ""));

            AddTextToDic("gameNameMappings",
                 new ApiDescriptionModel("游戏名称(各个语系)", ""),
                 new ApiDescriptionModel("Game name (each languages)", ""));

            AddTextToDic("Version",
               new ApiDescriptionModel(" `(API route路径)` 请带入固定版号v1 ", " 此参数为url上的固定版号 "),
               new ApiDescriptionModel(" `(Route Path)` Api Version , v1", " (this is not query string or post parameter)"));

            AddTextToDic("playerName",
              new ApiDescriptionModel("玩家账号"),
              new ApiDescriptionModel("player account"));

            AddTextToDic("convertRate",
              new ApiDescriptionModel("转换比例 (兑换比例)"),
              new ApiDescriptionModel("ConvertRate (ExchangeRate)"));

            AddTextToDic("reportApiKey",
              new ApiDescriptionModel("The credential key of API"),
              new ApiDescriptionModel("The credential key of API"));

            AddTextToDic("reportCategoryID",
              new ApiDescriptionModel("The credential ID of API"),
              new ApiDescriptionModel("The credential ID of API"));

            AddTextToDic("specialOrderSequence",
              new ApiDescriptionModel("(非必填参数) 指定特殊排序顺序 (仅供特定商户使用)"),
              new ApiDescriptionModel("(Optional) specify sort sequence (for specific merchant only)"));

            //特殊API 同樣的欄位名稱說明 會有額外調整
            //特殊API 同樣的欄位名稱說明 會有額外調整
            //特殊API 同樣的欄位名稱說明 會有額外調整
            //特殊API 同樣的欄位名稱說明 會有額外調整
            //特殊API 同樣的欄位名稱說明 會有額外調整

            AddTextToDic($"{nameof(CreatePlayerAccountExistRequestModel)}.playerName",
              new ApiDescriptionModel($" 玩家帐号", "`#格式限制請參考文件說明`"),
              new ApiDescriptionModel($" playerName", " `#account valid rules please refer to document description`"));

            AddTextToDic($"{nameof(GameRecordRequestModel)}.playerName",
              new ApiDescriptionModel($"{OptionalParameterCN} 玩家帐号", "(请传入完整帐号)"),
              new ApiDescriptionModel($"{OptionalParameterEN} playerName", " only support full match. "));

            AddTextToDic($"{nameof(PromotionRecordRequestModel)}.playerName",
              new ApiDescriptionModel($"{OptionalParameterCN} 玩家帐号", "(请传入完整帐号)"),
              new ApiDescriptionModel($"{OptionalParameterEN} playerName", " only support full match. "));

            //AddTextToDic($"{nameof(GameRecordViewModel)}.roundID",
            //   new ApiDescriptionModel("游戏局号(棋牌游戏) ", "此栏位已废弃"),
            //   new ApiDescriptionModel("game round ID (board game)", "* discard field"));


            //AddTextToDic("",
            //  new ApiDescriptionModel("", ""),
            //  new ApiDescriptionModel("bet time", ""));

            AddTextToDic($"{nameof(VendorAPIFreespinListReqeustParameter)}.freespinID",
              new ApiDescriptionModel("(非必填参数) 指定查詢特定freespinID"),
              new ApiDescriptionModel("(Optional) Specify query for a specific freespinID"));

            AddTextToDic($"{nameof(PromotionRecordRequestModel)}.eventPrizeType",
              new ApiDescriptionModel("(非必填参数) 活动奖品类型(系统定义编号)"),
              new ApiDescriptionModel("(Optional) Event Prize Type"));

            AddTextToDic($"{nameof(VendorAPIFreespinBudgetDepositReqeustParameter)}.amount",
              new ApiDescriptionModel("充值額度"),
              new ApiDescriptionModel("Top-up Amount"));

            AddTextToDic($"{nameof(VendorAPIFreespinBudgetWithdrawalReqeustParameter)}.amount",
              new ApiDescriptionModel("扣除额度"),
              new ApiDescriptionModel("Withdrawal Amount"));

            AddTextToDic($"{nameof(VendorAPIFreespinBudgetDepositReqeustParameter)}.rounds",
             new ApiDescriptionModel("增加可使用局数"),
             new ApiDescriptionModel("top-up rounds"));

            AddTextToDic($"{nameof(VendorAPIFreespinBudgetWithdrawalReqeustParameter)}.rounds",
              new ApiDescriptionModel("减少可使用局数"),
              new ApiDescriptionModel("Deduction rounds"));

            AddTextToDic($"{nameof(WalletCashPlayerAmountViewModel)}.amount",
              new ApiDescriptionModel("钱包余额", "实际可提领的余额为此栏位 (若有设定转帐比例，此数值已经过自动转换)"),
              new ApiDescriptionModel("Balance amount", "The actual withdrawable balance. (if the auto transfer conversion ratio is set, this value has been automatically converted)"));

            AddTextToDic($"{nameof(GameRecordViewModel)}.betTime",
              new ApiDescriptionModel("結算时间", "系统使用此栏位作为帐务统计时间基准"),
              new ApiDescriptionModel("SettleTime", "accounting report use this column."));

            AddTextToDic($"{nameof(GameRecordViewModel)}.roundID",
             new ApiDescriptionModel("*此栏位已弃用。无用途", "*弃用栏位"),
             new ApiDescriptionModel("This field is discard . useless now", "*discard"));

            AddTextToDic($"{nameof(GameRecordViewModel)}.beforeAmount",
             new ApiDescriptionModel("*此栏位已弃用。无用途", "*弃用栏位"),
             new ApiDescriptionModel("This field is discard . useless now", "*discard"));

            AddTextToDic($"{nameof(GameRecordViewModel)}.roundStartTime",
                new ApiDescriptionModel("*此栏位已弃用。无用途", "*弃用栏位"),
                new ApiDescriptionModel("This field is discard . useless now", "*discard"));

            AddTextToDic($"{nameof(GameRecordViewModel)}.roundEndTime",
                new ApiDescriptionModel("*此栏位已弃用。无用途", "*弃用栏位"),
                new ApiDescriptionModel("This field is discard . useless now", "*discard"));


            AddTextToDic($"{nameof(GameRecordViewModel)}.betID",
              new ApiDescriptionModel("投注单号", "(舊版使用. 建議使用 transactionNo)"),
              new ApiDescriptionModel("Bet TransactionNo", "(for Old version use. We suggest use transactionNo)"));

            AddTextToDic($"{nameof(GameRecordViewModel)}.transactionNo",
              new ApiDescriptionModel("投注单号", "(唯一单号。建议使用此)"),
              new ApiDescriptionModel("Bet TransactionNo", "(UniqueID, We suggest use this one.)"));

            AddTextToDic($"{nameof(GameInfoViewModel)}.gameCode",
             new ApiDescriptionModel("此栏位已无作用(旧版使用) ", ""),
             new ApiDescriptionModel("useless column for current version, please use `gameID` instead.", ""));

            AddTextToDic($"{nameof(BetRecordDetailInfoLinkGenerateRequestModel)}.TransactionNo",
                 new ApiDescriptionModel("投注单号(32个字)", "建议使用此参数查询、效能较佳。`两个参数选一个使用`"),
                 new ApiDescriptionModel("Transaction No.", "choose one between `TransactionNo` and `BetID`, We suggest use TransactionNo."));

            AddTextToDic($"{nameof(BetRecordDetailInfoLinkGenerateRequestModel)}.BetID",
                 new ApiDescriptionModel("⚠️此栏位即将弃用，不要再使用。投注单号(数字单号)", " `两个参数选一个使用` "),
                 new ApiDescriptionModel("⚠️This field is going to deprecated, Bet ID.", "choose one between `TransactionNo` and `BetID`, We suggest use TransactionNo."));

            AddTextToDic($"{nameof(QueryTransferRecordRequestModel)}.PGSTransactionNo",
                 new ApiDescriptionModel("[游戏商] 系统转账单号", "`两个参数选一个使用`"),
                 new ApiDescriptionModel("[Game Provider] Transaction number", "`choose one`"));

            AddTextToDic($"{nameof(QueryTransferRecordRequestModel)}.MerchantTransactionNo",
                new ApiDescriptionModel("[营运商] 转账单号", "`两个参数选一个使用`"),
                new ApiDescriptionModel("[Operator] Transaction number of transfer", "`choose one`"));

        }



        /// <summary>
        /// 請求參數，若名稱符合、swagger UI上會顯示此處定義的欄位說明
        /// </summary>
        public static Dictionary<string, ApiDescriptionModel> APIQueryParameterDescriptionDefinedCN = new Dictionary<string, ApiDescriptionModel>();

        /// <summary>
        /// 請求參數，若名稱符合、swagger UI上會顯示此處定義的欄位說明
        /// </summary>
        public static Dictionary<string, ApiDescriptionModel> APIQueryParameterDescriptionDefinedEN = new Dictionary<string, ApiDescriptionModel>();


        /// <summary>
        /// 回應參數，若名稱符合、swagger UI上會顯示此處定義的範例值
        /// 型別: string 類型
        /// </summary>
        public static Dictionary<string, string> OutputParameterSampleValueDefined = new Dictionary<string, string>()
        {
            { "link" , "https://example.invalid" },
            { "url" , "https://example.invalid" },
            { "imageUrl" , "https://example.invalid/myPicture.png" },
            { "betID" , "1052102551" },
            { "lang" , "zh-tw" },
            { "defaultLanguage" , "zh-tw" },
            { "defaultLang" , "zh-tw" },
            { "currency" , "CNY" },
            { "memo" , "備註欄位" },
            { "createBy" , "adminUser" },
            { "updateBy" , "adminUser" },
            { "userName" , "用戶名／用戶帳號" },
            { "displayName" , "顯示名稱" },
            { "password" , "輸入您的密碼" },
            { "confirmPassword" , "再次確認您的密碼" },
            { "apiSecretKey" , "8acc72e29907" },
            { "apiDoaminUrl" , "http://0.0.0.0" },
            { "apiValidatePlayerSessionUrl" , "target/validatePlayerSession" },
            { "ipAddress" , "0.0.0.0" },
            { "userGameDetail" , "10001:2000:-10:20180401,10002:1000:-10:20180402" },
            { "serverName" , "多福多財伺服器_1.0.1" },
            { "merchantName" , "PGS娛樂投注平台" },
            { "organizationName" , "PGS天馬團隊" },
            { "gameName" , "DFDZ" },
            { "gameCode" , "DFDZ" },
            { "supportedOrientation" , "Portrait" },
            { "gameTypeName" , "Slots" },
            { "orderNo" , "2019010100751241" },
            { "merchantOrderNo" , "AZY201906553641" },
            { "playerName" , "player_account" },
            { "transactionNo" , "202001011234567" },
            { "merchantSecretKey" , "4c269eb2aa8f690598df" },
            { "languageCode" , "en-us" },
        };


        /// <summary>
        /// 回應參數，若名稱符合、swagger UI上會顯示此處定義的範例值
        /// 型別: int / bigint
        /// </summary>
        public static Dictionary<string, ColumnSampleValueAndType> OutputParameterSampleNumberValueDefined = new Dictionary<string, ColumnSampleValueAndType>()
        {
            { "merchantID" ,new ColumnSampleValueAndType(175001)    },
            { "pageIndex" , new ColumnSampleValueAndType(1)     },
            { "pageSize" ,  new ColumnSampleValueAndType(10)    },
            { "totalPage" , new ColumnSampleValueAndType(5)     },
            { "gameID" ,    new ColumnSampleValueAndType(290088)  },
            { "rounds" ,    new ColumnSampleValueAndType(1)  },




            { "serverID" ,  new ColumnSampleValueAndType(102300410) },
            { "tradeID" ,   new ColumnSampleValueAndType(typeof(long), 20191200000095331 )},
            { "gameType" ,  new ColumnSampleValueAndType(1000)      },
            { "playerID" ,  new ColumnSampleValueAndType(1000003)   },
            { "faceID" ,    new ColumnSampleValueAndType(15)        },
            { "gameTimeSecond" ,new ColumnSampleValueAndType(360)},
        };


        /// <summary>
        /// 回應參數，若名稱符合、swagger UI上會顯示此處定義的範例值
        /// 型別: decimal
        /// </summary>
        public static Dictionary<string, decimal> OutputParameterSampleDecimalValueDefined = new Dictionary<string, decimal>()
        {
            { "betAmount" , 10.0M},
            { "validAmount" , 10.0M},
            { "payoutAmount" , 20.0M},
            { "bet" , 1.0M},
            { "rtp" , 96.0M},       
            { "currentAmount" , 5000.75M},
            { "updateAmount" , 2000.10M},
            { "flowAmount" , 100.50M},
            { "taxAmount" , 5.5M},
            { "amount" , 100.0M},
            { "amountWithConvert" , 100.0M},
            { "convertRate" , 1 },
        };


        /// <summary>
        /// 這些變數不會產生文件 因為會有無窮迴圈
        /// </summary>
        public static HashSet<string> OutputParameterIgnoreOutputDocumentColumns = new HashSet<string>()
        {
            "childPermissions"
        };


        public static HashSet<string> SkipOutputRequestBodyList = new HashSet<string>();

        public static HashSet<string> SkipOutputResponseColumnWhenPagedList = new HashSet<string>();

        public static HashSet<string> RequiredFieldColumnList = new HashSet<string>();



        public class ColumnSampleValueAndType
        {
            public ColumnSampleValueAndType()
            {

            }

            public ColumnSampleValueAndType(long sampleValue)
            {
                this.SampleValue = sampleValue;
                this.DataType = typeof(int);
            }

            public ColumnSampleValueAndType(Type type, long sampleValue)
            {
                this.SampleValue = sampleValue;
                this.DataType = type;
            }

            public long SampleValue { get; set; }

            public Type DataType { get; set; }

        }



        public static Dictionary<string, ApiDescriptionModel> GetParameterDescriptionByWebsite()
        {
            bool isEN = IsLanguageEN();

            if (isEN)
            {
                return APIQueryParameterDescriptionDefinedEN;
            }
            else
            {
                return APIQueryParameterDescriptionDefinedCN;
            }
        }



        public static bool IsColumnExist(string columnName)
        {
            columnName = columnName.ToLower();
            Dictionary<string, ApiDescriptionModel> dic = GetParameterDescriptionByWebsite();

            if (dic.ContainsKey(columnName))
            {
                return true;
            }

            return false;
        }



        public static ApiDescriptionModel GetParameterDescriptionByColumnName(string columnName, string specialColumnName)
        {
            columnName = columnName.ToLower();
            specialColumnName = specialColumnName.ToLower();

            Dictionary<string, ApiDescriptionModel> dic = GetParameterDescriptionByWebsite();

            if (dic.ContainsKey(specialColumnName))
            {
                return dic[specialColumnName];
            }

            if (dic.ContainsKey(columnName))
            {
                return dic[columnName];
            }

            return new ApiDescriptionModel("", "");
        }


        public static bool IsVendorAPI()
        {
            return SettingHelper.WebSiteName.Contains(".API.Vendor");
        }


        public static bool IsLanguageEN()
        {
            if (MyHttpContext.Current == null)
                return true;

            var value = MyHttpContext.Current.Request.Cookies[SystemConstant.HeaderKey_Lang];

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value == LanguageEnum.en_us.ToString();
        }



        public static void AddToPagedIgnoreDictionary(string name)
        {
            name = name.ToLower();

            if (SkipOutputResponseColumnWhenPagedList.Contains(name))
                return;

            SkipOutputResponseColumnWhenPagedList.Add(name);
        }


        public static void AddToRequiredFieldDictionary(string name)
        {
            name = name.ToLower();

            if (RequiredFieldColumnList.Contains(name))
                return;

            RequiredFieldColumnList.Add(name);
        }



        public static void AddToRequestBodyIgnoreDictionary(string name)
        {
            name = name.ToLower();

            if (SkipOutputRequestBodyList.Contains(name))
                return;

            SkipOutputRequestBodyList.Add(name);
        }


        /// <summary>
        /// 若判斷有在定義的清單中 則不輸出此欄位
        /// </summary>
        public static bool IsExistInSkipOutputResponseColumnWhenPagedList(string className, string columnName , int loopTimes)
        {
            string columnNameLower = columnName.ToLower();
            string keyName = $"{className.ToLower()}_{columnNameLower}";

            if (SkipOutputResponseColumnWhenPagedList.Contains(keyName))
                return true;

            if(loopTimes <= 1 && className.ContainsWihtoutCase("pagedlist"))
            {
                if(SkipOutputResponseColumnWhenPagedList.IsNotEmptyNull())
                {
                    var pageListColumns = SkipOutputResponseColumnWhenPagedList
                        .Select(x => x.Replace("pagedlist`1_", "")
                        .ToLower())
                        .ToList();

                    if(pageListColumns.Any(c => c == columnNameLower))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool IsExistInSkipOutputRequestBodyList(string className, string columnName)
        {
            string keyName = $"{className.ToLower()}_{columnName.ToLower()}";

            if (SkipOutputRequestBodyList.Contains(keyName))
                return true;

            return false;
        }


        public static bool IsExistInRequiredField(string className, string columnName)
        {
            className = className ?? "";
            columnName = columnName ?? "";

            string keyName = $"{className.ToLower()}_{columnName.ToLower()}";

            if (RequiredFieldColumnList.Contains(keyName))
                return true;

            return false;
        }

        public static string GenerateRequiredFieldHtmlStyle(bool isRequired)
        {
            bool isEN = IsLanguageEN();
            string myText = isEN ? "Required" : "必填栏位";
            return (isRequired ? $" <span class='required'> {myText}* <span>" : "");
        }


        public static bool IsOutputNumberColumnsExist(string columnName)
        {
            if (OutputParameterSampleNumberValueDefined.ContainsKey(columnName))
                return true;

            return false;
        }

        public static bool IsOutputDecimalColumnsExist(string columnName)
        {
            if (OutputParameterSampleDecimalValueDefined.ContainsKey(columnName))
                return true;

            return false;
        }



        public static bool IsOutputKeyExist(string columnName)
        {
            if (OutputParameterSampleValueDefined.ContainsKey(columnName))
                return true;

            return false;
        }

        private static void AddTextToDic(string key, ApiDescriptionModel CN, ApiDescriptionModel EN)
        {
            string keyToLower = key.ToLower();
            APIQueryParameterDescriptionDefinedCN.Add(keyToLower, CN);
            APIQueryParameterDescriptionDefinedEN.Add(keyToLower, EN);
        }

    }


    public class ApiDescriptionModel
    {

        public ApiDescriptionModel(string desc, string memo = "")
        {
            this.Description = desc;
            this.ExtraMemo = memo; //這欄會以醒目顏色顯示
        }


        public string Description { get; set; }

        /// <summary>
        /// 额外说明
        /// </summary>
        public string ExtraMemo { get; set; }


        public string FullDescription
        {
            get
            {
                return this.Description + " " + this.ExtraMemo;
            }
        }


    }


    public class SwaggerApiDescriptionEN : Attribute
    {
        public SwaggerApiDescriptionEN(string desc)
        {
            this.Description = desc;
        }

        public string Description { get; set; }
    }

    public class SwaggerApiDescriptionCN : Attribute
    {
        public SwaggerApiDescriptionCN(string desc)
        {
            this.Description = desc;
        }

        public string Description { get; set; }
    }

}
