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
using game.Entity.Payment;

namespace game.Service.Service
{
    public class PayGatewayConvertTypeMappingService : BaseService, IPayGatewayConvertTypeMappingService
    {

        public PayGatewayConvertTypeMappingRepository PayGatewayConvertTypeMappingRepository { get; set; }


        public PayGatewayConvertTypeMappingEntity GetByID(int id)
        {
            var query = PayGatewayConvertTypeMappingRepository.GetById(id);
            return query;
        }

        public List<PayGatewayConvertTypeMappingEntity> GetByIDs(List<int> ids)
        {
            var query = PayGatewayConvertTypeMappingRepository.GetByIds(ids);
            return query;
        }

        public List<PayGatewayConvertTypeMappingEntity> GetByFilter(QueryPayGatewayConvertTypeMappingRequestViewModel filter)
        {
            var query = PayGatewayConvertTypeMappingRepository.GetByFilter(filter);
            return query;
        }

        public CRUDResultModel EditAndDelete(PayGatewayConvertTypeMappingUpdateRequestModel model)
        {
            foreach(var item in model.Items)
            {
                List<CheckResultModel> valids = new List<CheckResultModel>
                {
                    ValidateHelper.CheckIsNotZeroOrMinus(item.ConvertType , TargetColumnEnum.Type),
                    ValidateHelper.CheckIsNotZeroOrMinus(model.GWID , TargetColumnEnum.GWID),
                };

                var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

                if (!result.IsSuccess)
                    return result;
            }

            PayGatewayConvertTypeMappingRepository.DeleteAndAddNew(model.GWID , model.Items);
            return new CRUDResultModel(true);
        }





    }

}
