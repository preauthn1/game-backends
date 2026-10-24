using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Models.Enums;
using System.Linq;
using target.Models.ViewModels;
using target.Models.Table;

namespace target.Utility.Helper
{
    public class EnumsExtraConfigHelper
    {
        /// <summary>
        /// 當複製商戶時。指定的配置不會同步 
        /// 僅針對『正式商戶』
        /// </summary>
        public static List<int> MerchantCustomizedSettingKey_NotSyncWhenCopyMerchant_Production()
        {
            List<int> ignoreSetting = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.DefaultWalletBalance.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.Common_IsCredentialProvideCompleted.ToInteger(), //經評估後 還是不要copy這個比較好
                        MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.IsAllowCreateTestAccount.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.Common_MerchantCopySource.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.Common_AgentMemo.ToInteger(),
                    };

            return ignoreSetting;
        }

        /// <summary>
        /// 當複製商戶時。指定的配置不會同步 
        /// 針對 *所有測試或正式商戶
        /// </summary>
        public static List<int> MerchantCustomizedSettingKey_NotSyncWhenCopyMerchant_AllEnv()
        {
            List<int> ignoreSetting = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.GameSetting_IDGNameSpace_CreateTime.ToInteger(),
                    };

            return ignoreSetting;
        }

    }
}
