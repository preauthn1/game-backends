using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Utility.Helper;
using target.Models;
using target.Models.Attributes;
using target.Utility;
using game.Entity.Game;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class BetSummaryInfoService : BaseGameService, IBetSummaryInfoService
    {

        public ServerListRepository ServerListRepository { get; set; }

        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }

        public BetRecordRepository BetRecordRepository { get; set; }


        public BetSummaryRequestViewModel GetBetSummaryByServer(BetSummaryRequestModel model)
        {
            model.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.End).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.Begin).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.Begin).Rule_StartTimeEndTimeRange(model.End , 25);

            if(!RegexRuleHelper.IsValidServerID(model.ServerID))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ServerID"));
            }

            int gameID = AppHelper.GetGameIdByServerID(model.ServerID);

            if(!AppHelper.IsPgsSelfGame(gameID))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
            }

            var query = BetRecordRepository.GetBetSummaryByServer(model.ServerID, model.Begin, model.End);

            return new BetSummaryRequestViewModel
            {
                ServerID = model.ServerID,
                TotalBetAmount = query.TotalBetAmount,
                TotalValidAmount = query.TotalValidAmount,
                TotalWinLossAmount = query.TotalWinLoss,
            };
        }


    }
}
