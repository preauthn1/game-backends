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
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class ServerListService : BaseAdminSysService, IServerListService
    {
        public ServerListRepository ServerListRepository { get; set; }

        public ServerMerchantMappingRepository ServerMerchantMappingRepository { get; set; }

        public PlayerCurrentServerRepository PlayerCurrentServerRepository { get; set; }


        public PagedList<ServerListViewModel> GetByPagingFilter(QueryServerListApiRequestParameterModel filter)
        {
            filter.GameIDs = filter.GameID.HasValue ? filter.GameID.Value.ToListCollection() : filter.GameIDs;
            filter.MerchantIDs = filter.AssignMerchantID.ToIntegerListCollection();

            var query = ServerListRepository.GetPagingByFilter(filter);

            var queryMappings = ServerMerchantMappingRepository.GetAll();

            var viewModels = ConvertToNewTypePagingObject<ServerListEntity, ServerListViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items , queryMappings);
            return viewModels;
        }


        public ServerListViewModel GetByID(int serverID)
        {
            var queryServerList = ServerListRepository.GetById(serverID);

            if (queryServerList == null)
                throw new TerproException(ApiErrorCode.NotFoundServer);

            var queryMappings = ServerMerchantMappingRepository.GetByServerID(serverID);
            var convert = ConvertToViewModels(queryServerList);

            var viewModels = new List<ServerListViewModel>
            {
                convert
            };

            SetExtraProperty(viewModels, queryMappings);
            return convert;
        }


        public void Create(int userID , ServerListCreateRequestModel model)
        {
            model.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ServerName).Rule_NotEmptyText();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.MinAvailableMoney).Rule_NotMinus();
            model.RuleFor(x => x.Status).Rule_NotMinus();
            model.RuleFor(x => x.FlagLevel).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Currency).Rule_NotEmptyText();

            bool isExist = ServerListRepository.IsExist(model.ServerID);

            if (isExist)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var querySameName = ServerListRepository.GetByFilter(new QueryServerListRequestViewModel
            {
                ServerNameFullMatch = model.ServerName,
                GameIDs = model.GameID.ToListCollection()
            });

            if(querySameName.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            ////請實作參數檢查
            var newEntity = model.ReturnInjecteObject<ServerListEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateBy = currentUser.UserName;

            var newServerListID = ServerListRepository.Create(newEntity);
        }


        public void Update(int userID, ServerListUpdateRequestModel model)
        {
            model.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ServerName).Rule_NotEmptyText();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.MinAvailableMoney).Rule_NotMinus();
            model.RuleFor(x => x.Status).Rule_NotMinus();
            model.RuleFor(x => x.FlagLevel).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Currency).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryExist = ServerListRepository.GetById(model.ServerID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundServer);

            queryExist.ServerName = model.ServerName;
            queryExist.GameID = model.GameID;
            queryExist.FlagLevel = model.FlagLevel;
            queryExist.ClientAddr = model.ClientAddr;
            queryExist.HttpAddr = model.HttpAddr;
            queryExist.NotifyAddr = model.NotifyAddr;
            queryExist.Status = model.Status;
            queryExist.MinAvailableMoney = model.MinAvailableMoney;
            queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryExist.UpdateBy = currentUser.UserName;
            queryExist.Currency = model.Currency;
            queryExist.RoomFlagLevel = model.RoomFlagLevel;

            ServerListRepository.Update(queryExist);

            CacheRemoveHelper.RemoveCache_WhenUpdateServer(model.ServerID);
        }


        public void Delete(int serverID)
        {
            var queryExist = ServerListRepository.GetById(serverID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundServer);

            var now = DateTimeHelper.GetCurrentTime();

            if ((now - queryExist.UpdateTime).TotalSeconds >= 600)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_NotAllowDeleteData_ModifyRecently);

            var queryCurretPlayer = PlayerCurrentServerRepository.GetByFilter(new QueryPlayerCurrentServerRequestViewModel
            {
                ServerIDs = serverID.ToListCollection()
            });

            if (queryCurretPlayer.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_NotAllowDeleteData_CurrentServer);

            ServerListRepository.Delete(serverID);
            ServerListRepository.DeleteRelationDataWhenServerDelete(serverID);


            CacheRemoveHelper.RemoveCache_WhenUpdateServer(serverID);
        }


        private ServerListViewModel ConvertToViewModels(ServerListEntity queryServerList )
        {
            var convert = queryServerList.ReturnInjecteObject<ServerListViewModel>();
            convert.RoomFlagLevelText = MultiLanguageTextHelper.GetEnumsCultureText((GameRoomFlagLevelEnum)queryServerList.RoomFlagLevel);
            convert.CurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(convert.Currency);

            return convert;
        }


        private void SetExtraProperty(List<ServerListViewModel> viewModels , List<ServerMerchantMappingEntity> mappings)
        {
            List<int> gameIDs = viewModels.Select(x => x.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            var mappingCountDic = mappings.GroupBy(x => x.ServerID).ToDictionary(z => z.Key, z => z.Count());

            foreach (var item in viewModels)
            {
                item.GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID);

                if(mappingCountDic.ContainsKey(item.ServerID))
                {
                    item.AssignMerchantTotalCount = mappingCountDic[item.ServerID];
                }
            }
        }

    }
}
