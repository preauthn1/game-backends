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
    public class UnionpayBankPlatformMappingService : BaseService, IUnionpayBankPlatformMappingService
    {

        public UnionpayBankPlatformMappingRepository UnionpayBankPlatformMappingRepository { get; set; }


        public UnionpayBankPlatformMappingEntity GetByID(int id)
        {
            var query = UnionpayBankPlatformMappingRepository.GetById(id);
            return query;
        }

        public List<UnionpayBankPlatformMappingEntity> GetByIDs(List<int> ids)
        {
            var query = UnionpayBankPlatformMappingRepository.GetByIds(ids);
            return query;
        }

        public List<UnionpayBankPlatformMappingEntity> GetByFilter(QueryUnionpayBankPlatformMappingRequestViewModel filter)
        {
            var query = UnionpayBankPlatformMappingRepository.GetByFilter(filter);
            return query;
        }


        public CRUDResultModel Edit(UnionpayBankPlatformUpdateViewModel model)
        {
            model = model ?? new UnionpayBankPlatformUpdateViewModel();

            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.PlatformID , TargetColumnEnum.Id),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;
            
            UnionpayBankPlatformMappingRepository.DeleteAndAddNew(model.PlatformID, model.Items);
            return new CRUDResultModel(true);
        }





    }

}
