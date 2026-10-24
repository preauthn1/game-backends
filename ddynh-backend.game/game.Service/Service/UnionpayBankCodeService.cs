using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Infrastructure;
using game.Service.DAO;

namespace game.Service.Service
{
    public class UnionpayBankCodeService : BaseService, IUnionpayBankCodeService
    {

        public UnionpayBankCodeRepository UnionpayBankCodeRepository { get; set; }


        public UnionpayBankCodeEntity GetByID(int id)
        {
            var query = UnionpayBankCodeRepository.GetById(id);
            return query;
        }

        public List<UnionpayBankCodeEntity> GetByIDs(List<int> ids)
        {
            var query = UnionpayBankCodeRepository.GetByIds(ids);
            return query;
        }

        public List<UnionpayBankCodeEntity> GetByFilter(QueryUnionpayBankCodeRequestViewModel filter)
        {
            var query = UnionpayBankCodeRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<UnionpayBankCodeEntity> GetPagingByFilter(QueryUnionpayBankCodeRequestViewModel filter)
        {
            var query = UnionpayBankCodeRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(UnionpayBankCodeEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.BankName , TargetColumnEnum.Name)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;

            var resultId = UnionpayBankCodeRepository.Create(model);
            return new CRUDResultModel
            {
                IsSuccess = true,
                ResultId = resultId,
                Message = "成功",
            };
        }
                
        public CRUDResultModel Edit(UnionpayBankCodeEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.Id , TargetColumnEnum.Content),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.BankName , TargetColumnEnum.Name)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
                return result;

            var queryExsit = UnionpayBankCodeRepository.GetById(model.Id);
            queryExsit.BankName = model.BankName;
            queryExsit.BankID = model.BankID;
            queryExsit.Show = model.Show;
            queryExsit.UpdateTime = DateTime.Now;
            queryExsit.UpdateBy = model.UpdateBy;


            UnionpayBankCodeRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }

    }

}
