using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Utility.Helper;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility.Helper.HttpContext;
using target.Models;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;
using System.Transactions;

namespace target.BL.Services
{

    public partial class GameVendorSpecialCaseService : BaseService, IGameVendorSpecialCaseService
    {
        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }



        /// <summary>
        /// 使用情境: 玩家退房時 強制結算AGG注單
        /// </summary>
        public void ForcePendingRecordTriggerSettle(int playerID)
        {
            var query = PlayerRepository.GetBriefByCache(playerID);

            if (query == null)
                return;

            var isAutoSettleSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(query.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsAutoSettleWhenGameLeave);

            if (!isAutoSettleSetting.SettingValueOfBoolean)
                return;

            string selectColumns = $" {nameof(BetRecordAGGEntity.BetRecordNoPGS)} ";

            //取N分鐘內的注單 (時間不要拉太長 避免效能很悲劇)
            var queryPendingRecords = BetRecordAGGRepository.GetByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PartitionTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 120),
                PartitionTimeEnd = DateTimeHelper.GetCurrentTime(),
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                PlayerIDs = playerID.ToListCollection(),
                TimeoutSeconds = 90

            }, selectColumns);

            if (queryPendingRecords.IsEmptyOrNull())
                return ;

            foreach (var item in queryPendingRecords)
            {
                //API 請求對方重新發出結算通知 (然後externalAPI會再次收到請求)
                TriggerAGGApi(item.BetRecordNoPGS);
                Thread.Sleep(100);
            }

            string msg = $"共 {queryPendingRecords.Count()} 筆資料";
        }


        protected virtual string TriggerAGGApi(string betRecordNoPGS)
        {
            var AGGInternalAPIService = ServiceInstanceHelper.InstanceSlotsGameVendorService(GameVendorTypeEnum.AGG);

            //API 請求對方重新發出結算通知 (然後externalAPI會再次收到請求)
            var result = AGGInternalAPIService.TriggerSettlementRequestAgain(new AGGForceTriggerSettlementRequestReqeustParameter
            {
                BetRecordNoPGS = betRecordNoPGS,
                DebugReferenceUUID = betRecordNoPGS
            });

            string responseText = result.responseText;
            return responseText;
        }


    }
}
