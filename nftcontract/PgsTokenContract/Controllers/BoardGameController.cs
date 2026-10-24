using Microsoft.AspNetCore.Mvc;
using target.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Web.Modules.Filter;
using PgsTokenContract.Models;
using PgsTokenContract.Services;
using System;

namespace target.API.AdminSys.Controllers
{

    public class BoardGameController : BaseController
    {
        public BoardGameBetService BoardGameBetService = new BoardGameBetService();


        [PlayerTokenAuthorizeFilter]
        [Route("boardGame/dragonTiger/lastRound/info")]
        [HttpGet]
        public ApiResult<BoardGameLastRoundBriefInfoModel> GetLastRoundInfo([FromQuery] LazyCommonRequestModel model)
        {
            var result = BoardGameBetService.GetLastRoundInfo(model.TokenID);

            //內部測試方便使用
            if (model != null && model.IsIncludeFullInfo)
            {
                result.DebugObject = BoardGameBetService.GetRoundInfoByID(result.RoundID);
            }

            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("boardGame/dragonTiger/round/{roundID}/info")]
        [HttpGet]
        public ApiResult<BoardGameCurrentRoundInfoModel> GetRoundInfoByID(string roundID)
        {
            var result = BoardGameBetService.GetRoundInfoByID(roundID);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("boardGame/dragonTiger/placeBet")]
        [HttpPost]
        public ApiResult<BetPlaceBetAndTransactionIDModel> PlaceBet([FromBody] BoardGameBetRequestModel model)
        {
            model.RuleFor(x => x.TokenID);
            model.Address = GetCurrentUserAccount();

            var result = BoardGameBetService.PlaceBet(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("boardGame/dragonTiger/{transactionHash}/bet-result-status")]
        [HttpGet]
        public ApiResult<BoardGameBetResultAndReasonModel> GetBetResult(string transactionHash)
        {
            var result = BoardGameBetService.GetBetResult(new LazyCommonRequestModel
            {
                TransactionHash = transactionHash
            });

            return ConvertToStandardFormat(result);
        }


        /// <summary>
        /// 內部測試用-手動觸發
        /// </summary>
        [Route("boardGame/dragonTiger/v3/extend-job-expiredTime")]
        [HttpGet, HttpPost]
        public ApiResult<string> ExtendExpiredTime(int minutes = 5)
        {
            BoardGameBetService.ExtendExpiredTime(minutes);
            return ConvertToStandardFormat("OK");
        }


        /// <summary>
        /// 內部測試用-手動觸發
        /// </summary>
        [Route("boardGame/dragonTiger/ManualCreateRoundAndAutoSettle")]
        [HttpGet, HttpPost]
        public ApiResult<CreateRoundResultModel> CreateRoundAndAutoSettle([FromQuery] LazyCommonRequestModel model)
        {
            model = model ?? new LazyCommonRequestModel();

            var result = BoardGameBetService.CreateRoundAndAutoSettle(new CreateRoundParameterModel
            {
                RoundSettleSeconds = model.Seconds <= 0 ? SystemConstant.DefaultRoundSettleSeconds : model.Seconds,
                IsSendNotifyToInternalChannel = true,
                IsThrowException = true,
                IsCheckNotSettleRound = model.IsForceCreateRound ? false : true,
            });

            return ConvertToStandardFormat(result);
        }

    }
}
