using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public class WarmUpService : BaseService
    {

        public GameListRepository GameListRepository { get; set; }


        /// <summary>
        /// 主機一啟動就馬上觸發一些工具暖機initial
        /// 這些服務特徵就是第一次啟用會很慢
        /// </summary>
        public async Task WarmUp()
        {
            await Task.Run(() =>
            {
                var notifyChannel = TelegramChannelEnum.pgs_common_error;
                DateTime begin = DateTimeHelper.GetCurrentTime();

                try
                {
                    var cacheTime = new TimeSpan(0, 1, 0);
                    var cacheValue = "OK";
                    string key = $"WarmUp_{Guid.NewGuid().ToString()}";

                    var queryGame = GameListRepository.GetById(GameListEnum.DFDC_多福多财.ToInteger());

                    var memoryCache = CacheHelper.InstanceCacheByType(isRemoteCache: false);
                    var remoteCache1 = CacheHelper.InstanceCacheByType(isRemoteCache: true);

                    memoryCache.Set(key, cacheValue, cacheTime);
                    remoteCache1.Set(key, cacheValue, cacheTime);

                    var q1 = memoryCache.Get<string>(key);
                    var q2 = remoteCache1.Get<string>(key);

                    //隨便new一個有繼承 baseService的
                    var serviceInstance = new InternalUtilityService();

                    string test = "";
                }
                catch (Exception ex)
                {
                    string msg = $"WarmUp fail ! , {RuntimeHelper.GetK8sInstanceName()} , {ex.Message}";

                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = msg,
                        TelegramChannelEnum = notifyChannel
                    });
                }

                DateTime end = DateTimeHelper.GetCurrentTime();

                int expiredSeconds = SettingHelper.IsDev ? 15 : 10;

                if((end - begin).TotalSeconds >= expiredSeconds)
                {
                    string msg = $"WarmUp 時間花費有點久。是否Redis或SQL連線出現異常 , {RuntimeHelper.GetK8sInstanceName()} ";

                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = msg,
                        TelegramChannelEnum = notifyChannel
                    });
                }

            });

        }

    }



}
