using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
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
    public partial class EventBetCounterService : BaseEventService
    {
        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public IEventCommonPrizeSendService EventCommonPrizeSendService { get; set; }



        public EventRankPreviewViewModel GetXXXXXXXXX(int userID, QueryEventBetCounterApiRequestParameter filter)
        {
            //filter.RuleFor(z => z.EventID).Rule_Rule_IsNotNull();
            //filter.EventIDs = filter.EventID.ToIntegerListCollection();

            //int eventID = filter.EventID.Value;

            //var queryEvent = EventRepository.GetById(eventID);

            //if (queryEvent == null)
            //    throw new TerproException(ApiErrorCode.NotFoundEvent);

            //var query = EventBetCounterRepository.GetPagingByFilter(filter);

            //var viewModels = ConvertToNewTypePagingObject<MerchantEntity, MerchantViewModel>(query);
            //viewModels.Items = query.Items.Select(x =>
            //    {
            //        var convert = x.ReturnInjecteObject<>();
            //        return convert;
            //    })
            //    .ToList();

            throw new TerproException(ApiErrorCode.NotImplement);
        }



    }

}
