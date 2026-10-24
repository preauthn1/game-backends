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

namespace target.BL.Interface
{

    public interface IGameListService
    {
        PagedList<GameListViewModel> GetByPagingFilter(QueryGameListApiRequestParameter filter);

        GameListViewModel GetByID(int targetID);

        GameMerchantsMappingInfoViewModel GetGameMerchantsInfo(int gameID);

        void Create(GameListCreateRequestModel model);

        void Update(GameListUpdateRequestModel model);

        void Delete(int targetID);

        GameListCustomizedSettingBatchEditModel GetGameListCutomziedSettings(int userID, int gameID);

        void CreateOrUpdateGameListSetting(int userID, GameListCustomizedSettingItemModel model);

        void DeleteGameListSetting(int userID, GameListCustomizedSettingItemModel model);
    }
}
