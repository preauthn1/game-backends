using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;

namespace game.Service.Service
{
    public class ConvertGatewayBankSettingService : BaseService, IConvertGatewayBankSettingService
    {

        public ConvertGatewayBankSettingRepository ConvertGatewayBankSettingRepository { get; set; }


        public ConvertGatewayBankSettingEntity GetByID(int id)
        {
            var query = ConvertGatewayBankSettingRepository.GetById(id);
            return query;
        }

        public List<ConvertGatewayBankSettingEntity> GetByIDs(List<int> ids)
        {
            var query = ConvertGatewayBankSettingRepository.GetByIds(ids);
            return query;
        }

        public List<ConvertGatewayBankSettingEntity> GetByFilter(QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            var query = ConvertGatewayBankSettingRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<ConvertGatewayBankSettingEntity> GetPagingByFilter(QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            var query = ConvertGatewayBankSettingRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Edit(ConvertGatewayBankSettingViewModel model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.GWID , TargetColumnEnum.Id)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            ConvertGatewayBankSettingRepository.DeleteAndAddNew(model.GWID , model.SettingItems);
            return new CRUDResultModel(true);
        }



    }

}
