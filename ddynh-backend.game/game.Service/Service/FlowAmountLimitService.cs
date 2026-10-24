using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;

namespace game.Service.Service
{
    public class FlowAmountLimitService : BaseService , IFlowAmountLimitService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        private FlowAmountLimitLogRepository _flowAmountLimitLogRepository { get; set; }


        public FlowAmountLimitService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }
        

        public CRUDResultModel UpdateStatus(CommonUpdateStatusRequestModel model)
        {
            model = model ?? new CommonUpdateStatusRequestModel();

            var validResults = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.TargetId , TargetColumnEnum.Id),
                ValidateHelper.CheckIsNotZeroOrMinus(model.TargetStatus , TargetColumnEnum.Status),
                ValidateHelper.CheckTextLength_IsValid(model.Memo , TargetColumnEnum.Memo , 100),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(validResults);

            if (!result.IsSuccess)
                return result;
            
            FlowAmountLimitLogEntity queryExist = _flowAmountLimitLogRepository.GetById(model.TargetId);

            if (queryExist == null)
                return InstanceFailModel(CommonErrorTypeEnum.NotFoundData);
            
            queryExist.Status = (int)model.TargetStatus;
            queryExist.Memo = model.Memo;
            queryExist.UpdateBy = model.UpdateBy;
            queryExist.UpdateTime = DateTime.Now;

            _flowAmountLimitLogRepository.Update(queryExist);

            string msg = "更新成功";

            if (model.TargetStatus == (int)FlowLimitStatus.Delete)
                msg = "刪除稽核成功";

            return new CRUDResultModel(true , msg);
        }


        public PagedList<UserFlowLimit> GetFlowAmountLimitLog(QueryFlowAmountLimitRequestViewModel filter)
        {
            var query = _flowAmountLimitLogRepository.GetFlowAmountLimitLog(filter);
            return query;
        }

    }
}
