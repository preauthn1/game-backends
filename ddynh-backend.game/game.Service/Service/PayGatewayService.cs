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
    public class PayGatewayService : BaseService, IPayGatewayService
    {

        public PayGatewayRepository PayGatewayRepository { get; set; }


        public PayGatewayEntity GetByID(int id)
        {
            var query = PayGatewayRepository.GetById(id);
            return query;
        }

        public List<PayGatewayEntity> GetByIDs(List<int> ids)
        {
            var query = PayGatewayRepository.GetByIds(ids);
            return query;
        }

        public List<PayGatewayEntity> GetByFilter(QueryPayGatewayRequestViewModel filter)
        {
            var query = PayGatewayRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<PayGatewayEntity> GetPagingByFilter(QueryPayGatewayRequestViewModel filter)
        {
            var query = PayGatewayRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(PayGatewayEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {           
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.GatewayCode , TargetColumnEnum.Code),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.GatewayName , TargetColumnEnum.Name),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            var resultId = PayGatewayRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Edit(PayGatewayEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.ID , TargetColumnEnum.Id),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.GatewayName , TargetColumnEnum.Name),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            var queryExsit = PayGatewayRepository.GetById(model.ID);

            if (queryExsit == null)
                return new CRUDResultModel("查无资料");

            queryExsit.Status = model.Status;
            queryExsit.SupportPayin = model.SupportPayin;
            queryExsit.SupportPayout = model.SupportPayout;
            queryExsit.GatewayName = model.GatewayName;
            queryExsit.UpdateBy = model.UpdateBy;

            PayGatewayRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }


    }

}
