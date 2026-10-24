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
using target.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Entity.Game;
using System.Threading;
using target.Utility;

namespace target.BL.Services
{

    public class SystemMonitorJobService : BaseService , ISystemMonitorService
    {     
    
        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public StatControlTableRepository StatControlTableRepository { get; set; }

        public int WarningDiskSize
        {
            get
            {
                if(SettingHelper.IsBWG)
                {
                    return 75;
                }

                return 100;
            }
        }

        public string CheckReportJobStatusHealthy()
        {
            var query = StatControlTableRepository.GetAll()
                .Where(x => x.StatStartTime.Year != 1970) //設為1970 = 已經停止的排程
                .ToList();
            
            var min = query.Select(z => z.StatStartTime).Min();
            var max = query.Select(z => z.StatStartTime).Max();
            
            if((max - min).TotalMinutes != 0 && !SettingHelper.IsMSTest)
            {
                string msg = "報表計算排程 StatControlTable 似乎出問題了 (建議重跑統計排程、並重新update資料表的起始結束時間)";
                SendMessageToMultipleChannel(msg);
            }

            return $"{min} ~ {max}";
        }


        public string CheckDatabaseDiskSize()
        {
            string resultMsg = string.Empty;
            var rep = new SystemSPRepository();

            List<Dictionary<string , object>> queryDiskInfo = rep.GetMasterDbDiskRemaningSize();
            resultMsg += CheckDiskSizeByStoreProcedureResult(queryDiskInfo , "MasterDB");

            //LogDB 已經沒有使用
            //List<Dictionary<string, object>> queryDiskInfoLogDB = rep.GetLogDBDiskRemaningSize();
            //resultMsg += CheckDiskSizeByStoreProcedureResult(queryDiskInfoLogDB, "LogDB");

            return resultMsg;
        }



        public string CheckMasterDBAlive()
        {
            var query = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.SystemMaintain_IsSwtichToMaintain);

            string msg = "OK";

            if (query == null || string.IsNullOrWhiteSpace(query.SettingValue))
            {
                msg = "CheckMasterDBAlive 查無物件 (db正常嗎?)";

                SendMessageToMultipleChannel(msg);
            }

            return "";
        }


        private string CheckDiskSizeByStoreProcedureResult(List<Dictionary<string, object>> queryDiskInfo , string dbName)
        {
            foreach (var item in queryDiskInfo)
            {
                var hasColumn = item.ContainsKey("drive");

                if (!hasColumn)
                {
                    string msg = " CheckDatabaseDisckSize , 查無欄位 drive";
                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = msg,
                        TelegramChannelEnum = TelegramChannelEnum.pgs_job_api
                    });

                    return msg;
                }


                var diskName = item["drive"].ToString();

                //D槽
                if (diskName == "D")
                {
                    var otherColumn = item.Where(z => z.Key != "drive").FirstOrDefault();

                    //MB單位 轉成 GB
                    decimal remaningSizeGB = Int64.Parse(otherColumn.Value.ToString()) / 1000;

                    if (!SettingHelper.IsMSTest && remaningSizeGB < WarningDiskSize)
                    {
                        string msg = $"{dbName} D槽空間不足 請注意 , 剩餘空間: {remaningSizeGB} GB";

                        SendMessageToMultipleChannel(msg);
                    }

                    return " 剩餘空間:" + remaningSizeGB + " GB。";
                }

            }

            return " Not Found";
        }



        private void SendMessageToMultipleChannel(string msg)
        {
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_job_api
            });

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = SlackChannelEnum.n_pgs_job_api
            });
        }







    }
}
