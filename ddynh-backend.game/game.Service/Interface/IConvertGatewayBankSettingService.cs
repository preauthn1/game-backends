using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IConvertGatewayBankSettingService
    {

        ConvertGatewayBankSettingEntity GetByID(int id);


        List<ConvertGatewayBankSettingEntity> GetByIDs(List<int> ids);


        List<ConvertGatewayBankSettingEntity> GetByFilter(QueryConvertGatewayBankSettingRequestViewModel filter);


        PagedList<ConvertGatewayBankSettingEntity> GetPagingByFilter(QueryConvertGatewayBankSettingRequestViewModel filter);


        CRUDResultModel Edit(ConvertGatewayBankSettingViewModel model);

    }
}
