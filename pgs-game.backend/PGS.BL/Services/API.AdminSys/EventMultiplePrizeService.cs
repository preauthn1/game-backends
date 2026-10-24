using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.CultureText;
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
using System.Transactions;

namespace target.BL.Services
{
    public partial class EventMultiplePrizeService : BaseEventService, IEventMultiplePrizeService
    {

        public GameListRepository GameListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public GameRTPConfigRepository GameRTPConfigRepository { get; set; }

        public EventDrawPrizeRepository EventDrawPrizeRepository { get; set; }

        public EventDrawPrizeItemRepository EventDrawPrizeItemRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }

        public IPlayerRTPChangeRefService PlayerRTPChangeRefService { get; set; }



        public PagedList<EventDrawPrizeViewModel> GetByPagingFilter(int userID, QueryEventDrawPrizeApiRequestParameterModel filter)
        {
            var query = EventDrawPrizeRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<EventDrawPrizeEntity, EventDrawPrizeViewModel>(query);
            viewModels.Items = query.Items
                .Select(x => x.ReturnInjecteObject<EventDrawPrizeViewModel>())
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public EventDrawPrizeDetailViewModel GetDrawPrizeDetail(int userID, int eventDrawPrizeID)
        {
            var query = EventDrawPrizeRepository.GetById(eventDrawPrizeID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryItems = EventDrawPrizeItemRepository.GetByFilter(new QueryEventDrawPrizeItemRequestViewModel
            {
                EventDrawPrizeIDs = eventDrawPrizeID.ToListCollection()
            });

            var viewModel = query.ReturnInjecteObject<EventDrawPrizeDetailViewModel>();

            var multipleType = (EventMultiplePrizeTypeEnum)query.Type;
            viewModel.TypeText = MultiLanguageTextHelper.GetEnumsCultureText(multipleType);

            viewModel.Items = queryItems
                .Select(z => z.ReturnInjecteObject<EventDrawPrizeItemViewModel>())
                .ToList();

            viewModel.PageControl = new EventMultiplePrizePageControl
            {
                IsProbabilityWeightsDisable = false,
                AmountColumnDisplayText = "倍率",
            };

            //編輯頁面的客製化 (UI、文字差異)
            EventDefinitionHelper.SetUIControl_ByMultiplePrizeType(multipleType, viewModel);
            return viewModel;
        }


        public void UpdateDrawPrizeDetail(int userID, EventDrawPrizeDetailViewModel model)
        {
            model.RuleFor(z => z.EventDrawPrizeID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.DisplayName).Rule_NotEmptyText();
            model.RuleFor(z => z.MaxReferenceValue).Rule_NotMinus();

            var query = EventDrawPrizeRepository.GetById(model.EventDrawPrizeID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var user = GetCurrentOwnPermissionByCache(userID);

            DateTime now = DateTimeHelper.GetCurrentTime();
            query.DisplayName = model.DisplayName;
            query.MaxReferenceValue = model.MaxReferenceValue;
            query.UpdateTime = now;
            query.UpdateBy = user.UserName;
            query.DisplayCurrency = model.DisplayCurrency;

            EventDrawPrizeRepository.Update(query);

            var queryItems = EventDrawPrizeItemRepository.GetByFilter(new QueryEventDrawPrizeItemRequestViewModel
            {
                EventDrawPrizeIDs = model.EventDrawPrizeID.ToListCollection()
            });

            foreach(var updateEntity in queryItems)
            {
                var matchViewModel = model.Items.FirstOrDefault(z => z.EventDrawPrizeItemID == updateEntity.EventDrawPrizeItemID);

                if (matchViewModel == null)
                    continue;

                if(matchViewModel.LimitQuantity.HasValue && 
                   matchViewModel.LimitQuantity.Value > 0)
                {
                    matchViewModel.RuleFor(z => z.AmountOrQuantity).Rule_NotZeroNotMinus();
                }

                matchViewModel.RuleFor(z => z.LimitQuantity).Rule_NotMinus();
                matchViewModel.RuleFor(z => z.ProbabilityWeights).Rule_NotMinus();


                updateEntity.AmountOrQuantity = matchViewModel.AmountOrQuantity;
                updateEntity.LimitQuantity = matchViewModel.LimitQuantity;
                updateEntity.ProbabilityWeights = matchViewModel.ProbabilityWeights;
                updateEntity.UpdateBy = user.UserName;
                updateEntity.UpdateTime = now;

                //迴圈更新 但這功能應該不頻繁 還好
                EventDrawPrizeItemRepository.Update(updateEntity);
            }

            if(query.EventID.HasValue)
            {
                var para = new
                {
                    EventID = query.EventID.Value,
                    SettingKey = EventCustomizedSettingKeyEnum.Tag_PrizeEmpty.ToInteger()
                };

                //只要有異動就嘗試清除tag
                EventCustomizedSettingRepository.DeleteByModel(para);
            }

            return;
        }


        public CRUDResultModel CloneDrawPrize(int userID, EventDrawPrizeCloneRequestModel model)
        {
            model.RuleFor(z => z.EventDrawPrizeID).Rule_NotZeroNotMinus();

            var user = GetCurrentOwnPermissionByCache(userID);

            var queryExist = EventDrawPrizeRepository.GetById(model.EventDrawPrizeID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryItems = EventDrawPrizeItemRepository.GetByFilter(new QueryEventDrawPrizeItemRequestViewModel
            {
                EventDrawPrizeIDs = model.EventDrawPrizeID.ToListCollection()
            });

            string newName = model.DisplayName;

            if (string.IsNullOrWhiteSpace(newName))
                newName = queryExist.DisplayName + " (Copy)";

            DateTime now = DateTimeHelper.GetCurrentTime();

            var copyInsertEntity = new EventDrawPrizeEntity
            {
                MaxReferenceValue = queryExist.MaxReferenceValue,
                Type = queryExist.Type,
                DisplayCurrency = queryExist.DisplayCurrency,

                DisplayName = newName,
                Status = NormalActiveStatus.Enable.ToInteger(),
                Memo = "Copy",
                UpdateTime = now,
                UpdateBy = user.UserName,
                CreateBy = user.UserName,
                CreateTime = now,
            };


            using(var scope = new TransactionScope())
            {
                int newDrawPrizeID = EventDrawPrizeRepository.Create(copyInsertEntity);

                var insertItems = new List<EventDrawPrizeItemEntity>();

                foreach (var cloneSource in queryItems)
                {
                    insertItems.Add(new EventDrawPrizeItemEntity
                    {
                        AmountOrQuantity = cloneSource.AmountOrQuantity,
                        ProbabilityWeights = cloneSource.ProbabilityWeights,
                        UsageQuantity = 0,
                        LimitQuantity = cloneSource.LimitQuantity,
                        EventDrawPrizeID = newDrawPrizeID,
                        UpdateTime = now,
                        UpdateBy = user.UserName
                    });
                }

                EventDrawPrizeItemRepository.CreateMany(insertItems);
                scope.Complete();

                return new CRUDResultModel
                {
                    ResultId = newDrawPrizeID
                };
            }
        }

        public void Delete(int userID, int eventDrawPrizeID)
        {
            var queryExist = EventDrawPrizeRepository.GetById(eventDrawPrizeID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExist.Status = NormalActiveStatus.PermanentDelete.ToInteger();

            EventDrawPrizeRepository.Update(queryExist);
        }


        private void SetExtraProperty(List<EventDrawPrizeViewModel> viewModels)
        {
            List<int> drawPrizeIDs = viewModels.Select(z=> z.EventDrawPrizeID).ToList();
            List<int> eventIDs = viewModels.Where(z => z.EventID.HasValue).Select(z => z.EventID.Value).ToList();

            var queryEvents = EventRepository.GetByIds(eventIDs);

            var queryCount = EventDrawPrizeItemRepository.GetPrizeItemCount(drawPrizeIDs);

            foreach (var item in viewModels)
            {
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)item.Status);
                item.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventMultiplePrizeTypeEnum)item.Type);
                item.EventName = queryEvents.FirstOrDefault(z => z.EventID == item.EventID)?.DisplayName;

                var match = queryCount.FirstOrDefault(z => z.EventDrawPrizeID == item.EventDrawPrizeID);

                if (match != null)
                {
                    item.PrizeCount = match.PrizeCount;
                    item.TotalQuantity = match.TotalQuantity;
                    item.TotalUsageQuantity = match.TotalUsageQuantity;
                }

            }
        }

    }

}
