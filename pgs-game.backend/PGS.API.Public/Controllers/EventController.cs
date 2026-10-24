using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Utility.Extensions;
using example.invalid;
using example.invalid.Http;
using Microsoft.AspNetCore.Mvc;
using target.Web.Modules.Filter;
using target.Utility.Helper;
using target.Models;
using target.Models.Enums;

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class EventController : BaseController
    {

        public IPromotionEventService PromotionEventService { get; set; }

        public IEventLuckyDrawService EventLuckyDrawService { get; set; }


        /// <summary>
        /// 取得活動資訊
        /// </summary>
        [Route("events/event-info")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [ApiConfig(IsHeaderLogEnable = false , IsWriteToOtherLogTable = true)]
        [HttpGet]
        public PublicPageEventViewModel GetAllPublicEventInfo()
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = PromotionEventService.GetAllPublicEventInfo(playerID);
            return query;
        }

        /// <summary>
        /// 取得該商戶有開啟哪些類型的活動
        /// hint的意思是 有紅包未領的提示
        /// </summary>
        [Route("events/enable-events")]
        [Route("events/enable-events-and-hint")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpGet]
        public PublicPageEnableEventsViewModel GetLobbyEnableEventsAndMessageHint()
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = PromotionEventService.GetLobbyEnableEventsAndMessageHint(playerID);
            return query;
        }


        /// <summary>
        /// 取得單一活動資訊 (指定type)
        /// </summary>
        [Route("events/event-info-detail/{eventType}")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpGet]
        public PublicPageSingleEventDetailViewModel GetEventDetailByType(EventTypeEnum eventType)
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = PromotionEventService.GetSingleEventDetail(playerID , new PublicPageSingleEventDetailRequestModel
            {
                EventType = eventType.ToInteger()
            });
            return query;
        }


        /// <summary>
        /// 取得單一活動資訊 (其他參數)
        /// </summary>
        [Route("events/event-info-detail")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpGet]
        public PublicPageSingleEventDetailViewModel GetEventDetailByFilter([FromQuery] PublicPageSingleEventDetailRequestModel filter)
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = PromotionEventService.GetSingleEventDetail(playerID, filter);
            return query;
        }



        /// <summary>
        /// 活動抽獎 (開啟紅包)
        /// </summary>
        [Route("events/lucky-draw/redEnvelope")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpPost]
        public LuckyDrawResultViewModel RedEnvelopeLuckyDraw([FromBody] EventLuckyDrawRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            var drawResult = PromotionEventService.RedEnvelopeOpen(playerID , model);
            return drawResult;
        }


        /// <summary>
        /// 活動抽獎 (幸運轉盤)
        /// </summary>
        [Route("events/lucky-draw/LuckyWheel")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpPost]
        public LuckyDrawResultViewModel LuckyWheelStartDraw([FromBody] EventLuckyDrawRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            var drawResult = PromotionEventService.LuckyWheelStartDraw(playerID, model);
            return drawResult;
        }



        [Route("events/prize-records")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpGet]
        public PublicApiEventPrizeRecordModel GetPrizeRecords([FromQuery] PublicApiEventPrizeRecordRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            var drawResult = PromotionEventService.GetPrizeRecords(playerID, model);
            return drawResult;
        }


        /// <summary>
        /// 取得活動說明 (此API設計上 無需登入)
        /// </summary>
        [Route("events/description")]
        [HttpGet]
        public PublicPageEventDescriptionViewModel GetEventDescription([FromQuery]PublicPageEventDescriptionRequestViewModel filter)
        {
            int playerID = TryGetLobbyTokenPlayerID();
            var query = PromotionEventService.GetEventDescription(playerID, filter);
            return query;
        }


        /// <summary>
        /// 取得可挑戰任務
        /// </summary>
        [Route("events/quest")]
        [HttpGet]
        public PublicPageEventQuestViewModel GetAvailableQuest([FromQuery] PublicPageEventQuestRequestModel filter)
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = PromotionEventService.GetAvailableQuest(playerID, filter);
            return query;
        }

        /// <summary>
        /// 完成任務領取獎勵
        /// </summary>
        [Route("events/quest-reward")]
        [HttpPost]
        public void ReceiveQuestReward([FromBody] PublicPageEventQuestReceiveRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            PromotionEventService.ReceiveQuestReward(playerID, model);
            return;
        }


        /// <summary>
        /// 活動抽獎 (紅包)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("debug123/events/lucky-draw/redEnvelope")]
        [HttpGet]
        public LuckyDrawResultViewModel Debug_RedEnvelopeLuckyDraw(int playerID = 1017041)
        {
            var drawResult = PromotionEventService.RedEnvelopeOpen(playerID, new EventLuckyDrawRequestModel
            {
                Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
            });

            return drawResult;
        }


    }
}
