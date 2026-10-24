using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels.Query;
using game.Entity.Game;
using game.Entity;
using game.Infrastructure.Helper;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Entity.RawEntity;
using game.Infrastructure.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Service.Job.ServiceImplement.Services.ReferenceService.Services;

namespace game.Service.Service
{
    public abstract class BaseThirdPartyGameJobService : BaseService
    {

        /// <summary>
        /// 以下這些要求強制實作之
        /// </summary>
        public abstract int GameMerchantID { get; }

        public abstract void FetchBetRecordsSinglePlatform(JobTriggerFetchBetRecordParameterModel model);

        public abstract void CheckTargetDataIsCorrect(int platformID, DateTime targetDate_8TimeZone);





        public GameMerchantPlatformConfigRepository GameMerchantPlatformConfigRepository { get; set; }

        public UserInfoRepository UserInfoRepository { get; set; }



        /// <summary>
        /// 以台灣時區(+8) 當基準，重抓昨日的數據
        /// </summary>
        public void FetchByTaiwanYesterday(int platformID)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            DateTime yesDay = now.AddDays(-1).To_00_00_00();

            FetchBetRecordsSinglePlatform(new JobTriggerFetchBetRecordParameterModel
            {
                BeginTimeUTC = yesDay.AddHours(-1 * SystemConst.TIMEZONE_TAIWAN),
                EndTimeUTC = yesDay.AddDays(1).AddHours(-1 * SystemConst.TIMEZONE_TAIWAN),
                PlatformID = platformID
            });
        }


        /// <summary>
        /// 以台灣時區(+8) 當基準，重抓某幾天的數據
        /// </summary>
        public void FetchByTaiwanTimeFromDateToDate(DateTime begin, DateTime end, int platformID)
        {
            DateTime beginUTC = begin.AddHours(-8);
            DateTime endUTC = end.AddHours(-8);

            FetchBetRecordsSinglePlatform(new JobTriggerFetchBetRecordParameterModel
            {
                BeginTimeUTC = beginUTC,
                EndTimeUTC = endUTC,
                PlatformID = platformID
            });
        }


        /// <summary>
        /// 以台灣時區(+8) 當基準，重抓某一天的數據
        /// </summary>
        public void FetchByTaiwanTargetDate(DateTime twDate, int platformID)
        {
            twDate = twDate.Date;
            FetchBetRecordsSinglePlatform(new JobTriggerFetchBetRecordParameterModel
            {
                BeginTimeUTC = twDate.AddHours(-1 * SystemConst.TIMEZONE_TAIWAN),
                EndTimeUTC = twDate.AddDays(1).AddHours(-1 * SystemConst.TIMEZONE_TAIWAN),
                PlatformID = platformID
            });
        }




        public void CheckYesterdayDataIsCorrect(int platformID)
        {
            DateTime targetDate_Taiwan = DateTimeHelper.GetCurrentTime().AddDays(-1).Date;
            CheckTargetDataIsCorrect(platformID, targetDate_Taiwan);
        }



        protected List<int> GetSettingEqualPlatformIDs(int platformID)
        {
            var queryConfigSetting = GameMerchantPlatformConfigRepository.GetByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
            {
                GameMerchantIDs = GameMerchantID.ToListCollection()
            });

            var matchThisPlatform = queryConfigSetting.FirstOrDefault(x => x.PlatformID == platformID);

            //理論上一組密鑰 只會提供給一家使用 但測試環境就...
            List<int> queryAllEqualSettings = queryConfigSetting
                .Where(x => x.MerchantIdentity == matchThisPlatform.MerchantIdentity)
                .Select(x => x.PlatformID)
                .ToList();

            return queryAllEqualSettings;
        }


        protected int GetConfigSetting_PreviousSeconds()
        {
            try
            {
                return SettingHelper.JobAPI.DataBeginTime_PreviousSeconds;
            }
            catch
            {

            }

            return 600;
        }


    }

}
