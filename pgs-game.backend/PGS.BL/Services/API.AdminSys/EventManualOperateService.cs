using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public partial class EventManualOperateService : BaseEventService, IEventManualOperateService
    {
        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public OneTimesJobSettingRepository OneTimesJobSettingRepository { get; set; }

        public IEventCommonPrizeSendService EventCommonPrizeSendService { get; set; }


        public EventRankPreviewViewModel PreviewEventPlayerRank(int userID,  EventRankPreviewRequestModel filter)
        {
            filter.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();
            filter.PageSize = filter.PageSize <= 0 ? 200 : filter.PageSize;

            var queryEvent = GetEventFullInformation(filter.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var queryPrizeEmptyTag = queryEvent.CustomizedSettings.GetBySettingKey(EventCustomizedSettingKeyEnum.Tag_PrizeEmpty);
         
            var viewModel = new EventRankPreviewViewModel
            {
                EventID = queryEvent.EventID,
                EventName = queryEvent.DisplayName,
                BeginTimeText = queryEvent.BeginTime.AddHours(8).ToString("yyyy-MM-dd"),
                EndTimeText = queryEvent.EndTime.AddHours(8).ToString("yyyy-MM-dd"),
                IsPrizeEmpty = queryPrizeEmptyTag != null,
            };

            var competiteTypeSetting = queryEvent.CustomizedSettings.FirstOrDefault(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.RankEventCompetiteType);

            if(competiteTypeSetting != null)
            {
                viewModel.DefaultCompetiteType = competiteTypeSetting.SettingValueOfInt;
            }

            //-1 代表沒填值
            if(filter.OrderByType == -1)
            {
                filter.OrderByType = viewModel.DefaultCompetiteType;
            }

            var queryRankingList = EventBetCounterRepository.GetTopRankingPlayers(queryEvent.EventID, filter.PageSize, (EventCounterOrderByEnum)filter.OrderByType);

            var queryPlayersDic = PlayerRepository.GetByLargeAmountIDs(queryRankingList.Select(x => x.PlayerID).ToList())
                .ToDictionary(x => x.PlayerID, x => x.MerchantID);

            var rankingItems = queryRankingList.Select(z =>
            {
                var convert = z.ReturnInjecteObject<EventRankPreviewViewItemModel>();
                convert.MerchantID = queryPlayersDic.GetDictionaryOrDefaultValue(z.PlayerID, 0);
                return convert;

            }).ToList();

            SetExtraProperty(rankingItems);

            viewModel.Players = rankingItems;

            var queryEventJob = OneTimesJobSettingRepository.GetJobByEventID(queryEvent.EventID);

            if (queryEventJob != null)
            {
                viewModel.IsJobExist = true;
                viewModel.HintMemo = $"排程預定時間: {queryEventJob.NextTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}";
            }

            return viewModel;
        }



        public void ManualTriggerRankPrize(int userID , EventSimpleRequestModel model)
        {
            model.RuleFor(x => x.EventID).Rule_NotZeroNotMinus();

            int eventID = model.EventID;

            EventCommonPrizeSendService.SendRankEventPrize(eventID , new RankEventTriggerSendPrizeParameter
            {
                OrderBy = model.OrderBy
            });

            UserOperateRecordRepository.Create(new UserOperateRecordEntity
            {
                UserID = userID,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                CreateIPAddress = IPHelper.GetIP(),
                OperateType = UserOperateTypeEnum.ManualTriggerSendRankPrize.ToInteger(),
                DebugInfo = $"eventID:{eventID}",
            });
        }


        private void SetExtraProperty(List<EventRankPreviewViewItemModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            List<int> playerIDs = items.Select(z => z.PlayerID).ToList();

            var queryPlayers = PlayerRepository.GetByIds(playerIDs);

            int sequence = 0;

            foreach (var item in items)
            {
                sequence++;

                item.Sequence = sequence;
                item.PlayerName = queryPlayers.FirstOrDefault(z => z.PlayerID == item.PlayerID)?.PlayerName;
            }
        }

    }

}
