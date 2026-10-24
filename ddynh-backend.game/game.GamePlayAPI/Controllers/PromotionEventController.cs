using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using FW.Core.Engine;
using example.invalid;

namespace game.GamePlayAPI.Controllers
{
    public class PromotionEventController : BaseController
    {

        public IPromotionEventApiService PromotionEventApiService { get; set; }

        /// <summary>
        /// 取得轮盘相关活动
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("events")]
        [HttpGet]
        public PromotionEventViewModel GetEvents([FromUri]PromotionEventRequestViewModel model)
        {
            var result = PromotionEventApiService.GetEvents(model);
            return result;
        }

        /// <summary>
        /// 取得轮盘活动明细
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("events/{eventID}")]
        [HttpGet]
        public PromotionEventDetailViewModel GetEventDetail(int eventID ,[FromUri]PromotionEventDetailRequestViewModel model)
        {
            model = model ?? new PromotionEventDetailRequestViewModel();
            model.EventID = eventID;

            var result = PromotionEventApiService.GetEventDetail(model);
            return result;
        }

        /// <summary>
        /// 幸运轮盘
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("events/lucky-roulette/request")]
        [HttpPost]
        public PromotionEventLuckyRoulettDrawResultModel GetLuckyRouletteBonus([FromBody]PromotionEvenBonusDrawRequestViewModel model)
        {
            var result = PromotionEventApiService.LuckyRoulette_DrawPrize(model);
            return result;
        }


    }
}