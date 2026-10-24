using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Payment;
using game.Entity.ViewModels;
using game.Infrastructure;
using game.Service.DAO;

namespace game.Service.Service
{
    public class PaymentPlatformSettingService : IPaymentPlatformSettingService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        private PlatformPayTypeSettingRepository _platformPayTypeSettingRepository;
        private PayTypeCategoryMappingRepository _payTypeCategoryMappingRepository;

        

        public PaymentPlatformSettingService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
            _platformPayTypeSettingRepository = new PlatformPayTypeSettingRepository();
            _payTypeCategoryMappingRepository = new PayTypeCategoryMappingRepository();
        }


        public PlatformPayTypeSetting GetById(int platformSettingId)
        {
            var query = _platformPayTypeSettingRepository.GetById(platformSettingId);
            return query;
        }
        

        public APIResult UpdatePlatformSettingPriority(UpdatePlatformSettingPriorityRequestModel model)
        {
            if (model == null)
                return new APIResult(false, "參數錯誤");

            var allCheckResult = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.PayTypeID , TargetColumnEnum.OtherID),
                ValidateHelper.CheckIsNotZeroOrMinus(model.PlatformID , TargetColumnEnum.PlatformID),
                ValidateHelper.CheckIsNotZeroOrMinus(model.Priority , TargetColumnEnum.Priority)
            };

            var result = ValidateHelper.CheckAllSuccess(allCheckResult);

            if (!result.IsSuccess)
                return new APIResult(false, result.Message);

            _platformPayTypeSettingRepository.UpdatePrioirty(model.PayTypeID , model.PlatformID , model.Priority);
            return new APIResult(true);
        }



        public List<PayTypeCategoryMapping> GetAllPayTypeCategoryMapping()
        {
            var query = _payTypeCategoryMappingRepository.GetAll();
            return query;
        }

    }
}
