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
    public class ConvertGatewayService : BaseService, IConvertGatewayService
    {

        public ConvertGatewayRepository ConvertGatewayRepository { get; set; }


        public ConvertGatewayEntity GetByID(int id)
        {
            var query = ConvertGatewayRepository.GetById(id);
            return query;
        }

        public List<ConvertGatewayEntity> GetByIDs(List<int> ids)
        {
            var query = ConvertGatewayRepository.GetByIds(ids);
            return query;
        }

        public List<ConvertGatewayEntity> GetByFilter(QueryConvertGatewayRequestViewModel filter)
        {
            var query = ConvertGatewayRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<ConvertGatewayEntity> GetPagingByFilter(QueryConvertGatewayRequestViewModel filter)
        {
            var query = ConvertGatewayRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(ConvertGatewayEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.GatewayName , TargetColumnEnum.Name)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            model.CreateTime = DateTime.Now;
            model.UpdareTime = DateTime.Now;
            var resultId = ConvertGatewayRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Edit(ConvertGatewayEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.ID , TargetColumnEnum.Id),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.GatewayName , TargetColumnEnum.Name)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            var queryExsit = ConvertGatewayRepository.GetById(model.ID);

            if(queryExsit == null)
                return new CRUDResultModel("查无资料");

            queryExsit.GatewayName = model.GatewayName;
            queryExsit.ConvertType = model.ConvertType;
            queryExsit.Status = model.Status;
            queryExsit.UpdateBy = model.UpdateBy;
            queryExsit.UpdareTime = DateTime.Now;

            ConvertGatewayRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }





    }

}
