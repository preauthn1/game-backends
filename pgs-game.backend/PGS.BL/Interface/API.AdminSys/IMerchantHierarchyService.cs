using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IMerchantHierarchyService
    {
        MerchantHierarchyTreeviewViewModel GetHierarchyTreeView(int userID, MerchantHierarchyTreeviewRequestModel filter);

        void MerchantHierarchyMigrate(int userID, MerchantHierarchyMigrateRequestModel model);

        void CreateAsRootMerchantHierarchy(int userID, int merchantID);

        void CreateMerchantHierarchy(int userID, MerchantHierarchyCreateRequestModel model);

        CRUDResultModel CreateMerchantHierarchyWithTryCatch(int userID, MerchantHierarchyCreateRequestModel model);

        void DeleteMerchantHierarchy(int userID, MerchantHierarchyDeleteRequestModel model);

        MerchantHierarchyEntity ValidParentHierarchyBeforeCreateMerchantHierarchy(int parentMerchantID);

        MerchantEntity ValidParentBasicInfoBeforeCreateMerchant(MerchantParentAgentValidateBeforeCreateParameter model);

        void ValidParentAndEnableGames(int parentMerchantID, List<int> enableGameIDs);

        void GenerateHierarchyChildTreeData(int rootMerchantID);
    }
}
