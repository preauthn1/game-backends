using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Users;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Service.DAO;

namespace game.Service.Service
{
    public class OrganizationService : IOrganizationService
    {
        public OrganizationRepository OrganizationRepository;

        public OrganizationPlatformMappingRepository OrganizationPlatformMappingRepository;

        private IDatabaseFactory _dbFactory;


        public OrganizationService(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
            OrganizationRepository = new OrganizationRepository();
            OrganizationPlatformMappingRepository = new OrganizationPlatformMappingRepository();
        }


        public Organization GetOrganizationById(int orgId)
        {
            var query = OrganizationRepository.GetById(orgId);
            return query;
        }


        public List<Organization> GetOrganizationByIds(List<int> ids)
        {
            if (ids.IsEmptyOrNull())
                return OrganizationRepository.GetAll();

            var query = OrganizationRepository.GetByIds(ids);
            return query;
        }

        public int CreateOrganization(Organization org)
        {   
            //欄位必填檢查
            var checkLsit = new List<CheckResultModel>
            {
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(org.Name , TargetColumnEnum.Name),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(org.ShortName, TargetColumnEnum.ShortName),
            };

            ValidateHelper.CheckAllSuccessOrException(checkLsit);

            org.CreateTime = DateTime.Now;
            var resultId = OrganizationRepository.Create(org);
            return resultId;
        }


        public APIResult UpdateOrganization(OrganizationrUpdateRequestViewModel updateModel)
        {
            //欄位必填檢查
            var checkLsit = new List<CheckResultModel>
            {
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(updateModel.Name , TargetColumnEnum.Name),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(updateModel.ShortName, TargetColumnEnum.ShortName),
                ValidateHelper.CheckIsNotZeroOrMinus(updateModel.OrgID, TargetColumnEnum.Id)
            };

            var finalResult = ValidateHelper.CheckAllSuccess(checkLsit);

            if (!finalResult.IsSuccess)
                return new APIResult(false, finalResult.Message);          
            

            var queryExist = OrganizationRepository.GetById(updateModel.OrgID);
            queryExist.Name = updateModel.Name;
            queryExist.Memo = updateModel.Memo;
            queryExist.ShortName = updateModel.ShortName;


            var resultId = OrganizationRepository.Update(queryExist);
            return new APIResult(true);
        }


        public List<OrganizationPlatformMapping> OrganizationPlatformMappingByOrgIds(int orgId)
        {
            var query = OrganizationPlatformMappingRepository.GetByOrgId(orgId);
            return query;
        }


        public List<EachOrganizationPlatformCountSummaryViewModel> GetEachOrganizationCountSummaryInfo()
        {
            string spName = "PrWeb_GetEachOrganizationCountSummary";

            var query = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_AdminSys).ExecuteStoreProcedureForDataList<EachOrganizationPlatformCountSummaryViewModel>(spName);
            return query.ToList();
        }

        public List<EachPlatformOrganizationCountViewModel> GetEachPlatformOrganizationCount()
        {
            string spName = "PrWeb_GetEachPlatformOrganizationCount";

            var query = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_AdminSys).ExecuteStoreProcedureForDataList<EachPlatformOrganizationCountViewModel>(spName);
            return query.ToList();
        }


        public void UpdateOrganizationPlatformMapping(UpdateOrganizationPlatformRequestViewModel model)
        {
            if (model.OrgId <= 0)
                throw new Exception($"invalid OrgId");

            OrganizationPlatformMappingRepository.UpdateOrganizationPlatform(model.OrgId, model.PlatformIds);
            return;
        }



    }
}
