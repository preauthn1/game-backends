using game.Entity.ViewModels.Query;
using game.Service.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity.RawEntity;
using game.Infrastructure.Utility;
using game.Entity.Game;
using System.Runtime.InteropServices;
using game.Infrastructure;
using game.Service.Service;
using System.Threading;
using game.Infrastructure.Extensions.Helper;
using System.Reflection;
using System.Deployment;
using System.Deployment.Application;
using System.Data.SqlClient;
using System.Configuration;
using game.Infrastructure.Utility.ThirdParty;

namespace Console.Job.TriggerAPI
{
    class Program
    {
        public static string WebSiteName = SettingHelper.WebSiteName;

        private static Dictionary<int, DateTime> _nextExecuteTimeRecords = new Dictionary<int, DateTime>();

        /// <summary>
        /// 每五分鐘會重讀一次DB的設定 (或重啟console專案)
        /// </summary>
        public const int RELOAD_INTERVAL_MINUTES = 5;

        public const string LOG_FILE_NAME = "job_log.txt";

        public const string CACHE_KEY = "Query_Job_Setting";

        public static GameMerchantJobSettingRepository GameMerchantJobSettingRepository = new GameMerchantJobSettingRepository();



        static void Main(string[] args)
        {
            //隱藏關閉按鈕
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            System.Console.Title = $"【排程】Trigger Job API ({PublishVersion})";
            System.Console.WriteLine($"開始執行...");
            System.Console.WriteLine($"當前連接資料庫為: {GetDBConn()} , 請務必確認是否正確。");

            while (true)
            {
                //每次之間的間隔休息一小時段時間
                Thread.Sleep(500);

                DateTime now = DateTimeHelper.GetCurrentTime();
                DateTime utcNow = DateTime.UtcNow;

                string rndGUID = now.ToString("yyyyMMddHHmm") + "_" + Guid.NewGuid().ToString().Replace("-", "").Truncate(10);
                string nowText = now.ToString("HH:mm:ss");

                try
                {

                    //從cache中 抓出當前的job設定 
                    List<GameMerchantJobSettingRawEntity> queryJobSetting = GetJobSettingByCache();

                    if (queryJobSetting.IsEmptyOrNull())
                    {
                        continue;
                    }

                    //逐個處理(打API)
                    foreach (GameMerchantJobSettingRawEntity item in queryJobSetting)
                    {
                        if (item.JobNextStarttTime > now)
                            continue;

                        if (!IsReadyExecuteJobAgain(item))
                            continue;

                        var jobType = (GameMerchantJobTypeEnum)item.JobType;

                        string jobTypeName = jobType.ToString();

                        if (!jobType.IsDefinedValue())
                        {
                            jobTypeName = $"{item.JobType}({item.Memo})";
                        }

                        string jobID = string.Format("{0,3}" , item.ID);   
                        string logMsg = $"{nowText} JobID:{jobID} , {item.GameMerchantID}, {rndGUID} , {jobTypeName}";
                        System.Console.WriteLine(logMsg);

                        //更新時間
                        DateTime nextTime = _nextExecuteTimeRecords[item.ID];
                        GameMerchantJobSettingRepository.UpdateLastTimeAndNextTime(item.ID, nextTime);

                        var param = new RequestParameters
                        {
                            BaseUrl = item.JobAPIHost,
                            ApiUrl = item.JobAPIUrl,
                            Method = HttpMethodType.HttpPost,
                        };

                        param.AddHeaderData(SystemConst.AdminSys_AlwaysHeaderUserName, WebSiteName);
                        param.AddHeaderData(SystemConst.AdminSys_AlwaysHeaderMemo, rndGUID);

                        FileLogHelper.WriteFileLog(logMsg, LOG_FILE_NAME);


                        //觸發API (非同步執行) 避免等待這個排程 影響下一個排程
                        Task.Factory.StartNew(() =>
                            {
                                var apiResult = APIRequestHelper.RequestApiResult<string>(param);

                                if (apiResult.Exception != null)
                                {
                                    string errorMsg = $"Console 錯誤啦，{item.ID} {nowText} , {rndGUID} , {apiResult.ResponseText} {apiResult.Exception.Message}  {param.BaseUrl}{param.ApiUrl} ";
                                    FileLogHelper.WriteFileLog(errorMsg, LOG_FILE_NAME);
                                    
                                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                                    {
                                        Channel = SlackChannelEnum.n_dd_job_third_party,
                                        Message = errorMsg
                                    });
                                }
                            });

                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"{nowText} , {rndGUID} , {ex.ToString()}";
                    FileLogHelper.WriteFileLog(errorMsg, LOG_FILE_NAME);
                }

            }
        }


        private static List<GameMerchantJobSettingRawEntity> GetJobSettingByCache()
        {
            var queryCache = CacheHelper.GetCacheOrQuery(CACHE_KEY, () =>
               {
                   var querySetting = GameMerchantJobSettingRepository.GetByFilter(new QueryPlatformGameMerchantJobSettingRequestViewModel
                   {
                       JobCategories = GameMerchantJobCategoryEnum.TriggerBy_ConsoleTriggerAPI.ToInteger().ToListCollection(),
                       IsEnableOnly = true
                   });

                   //刷新設定
                   InstanceNextExecuteTime(querySetting);

                   return querySetting;

               }, expiredMinute: RELOAD_INTERVAL_MINUTES);

            return queryCache;
        }


        private static void InstanceNextExecuteTime(List<GameMerchantJobSettingRawEntity> queryJobSetting)
        {
            foreach (var item in queryJobSetting)
            {
                int jobID = item.ID;
                bool isExist = _nextExecuteTimeRecords.ContainsKey(jobID);

                if (isExist)
                {
                    _nextExecuteTimeRecords[jobID] = item.JobNextStarttTime;
                }
                else
                {
                    _nextExecuteTimeRecords.Add(jobID, item.JobNextStarttTime);
                }
            }
        }


        /// <summary>
        /// 判斷是否要再次觸發Job了
        /// </summary>
        private static bool IsReadyExecuteJobAgain(GameMerchantJobSettingRawEntity jobSetting)
        {
            int jobID = jobSetting.ID;

            DateTime now = DateTimeHelper.GetCurrentTime();

            DateTime queryNextTime = _nextExecuteTimeRecords[jobID];

            if (now >= queryNextTime)
            {
                DateTime nextTime = queryNextTime.AddSeconds(jobSetting.JobIntervalSeconds);

                bool isGapTooLarge = now >= nextTime;

                //如果下一次時間仍然小於現在，代表時間差距太大，重新修正時間
                if (isGapTooLarge)
                {
                    var gapSeconds = (now - queryNextTime).TotalSeconds;
                    var divide = ((int)gapSeconds / jobSetting.JobIntervalSeconds) + 1;
                    nextTime = queryNextTime.AddSeconds(jobSetting.JobIntervalSeconds * divide);
                }

                _nextExecuteTimeRecords[jobID] = nextTime;
                return true;
            }

            return false;
        }


        private static string PublishVersion
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                }
                else
                    return "無";
            }
        }


        private static string GetDBConn()
        {
            string connectString = ConfigurationManager.ConnectionStrings["CenterDB"].ToString();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectString);
            string IPAddress = builder.DataSource;
            return IPAddress;
        }



        #region 這段是可以隱藏 X (關閉) 按鈕

        private const int MF_BYCOMMAND = 0x00000000;

        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        #endregion

    }

}
