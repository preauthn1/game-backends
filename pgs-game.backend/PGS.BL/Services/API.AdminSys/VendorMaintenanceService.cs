using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace target.BL.Services
{
    public class VendorMaintenanceService : BaseAdminSysService, IVendorMaintenanceService
    {

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }


        public MerchantSelectTemplateRepository MerchantSelectTemplateRepository { get; set; }


        public PagedList<VendorMaintenanceViewModel> GetByPagingFilter(int userID, QueryVendorMaintenanceApiRequestParameterModel filter)
        {
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            var query = VendorMaintenanceRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<VendorMaintenanceEntity, VendorMaintenanceViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        private void SetExtraProperty(List<VendorMaintenanceViewModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            var queryTemplate = MerchantSelectTemplateRepository.GetByFilter(new QueryMerchantSelectTemplateRequestViewModel
            {
                CategoryIDs = DataCategoryEnum.MerchantSelect_GameVendorMaintain.ToInteger().ToListCollection(),
            });

            foreach (var item in items)
            {
                if (item.ExcludeMerchantTemplateID.HasValue)
                {
                    item.ExcludeMerchantTemplateName = queryTemplate.FirstOrDefault(x => x.TemplateID == item.ExcludeMerchantTemplateID)?.DisplayName;
                }

                if (item.IncludeMerchantTemplateID.HasValue)
                {
                    item.IncludeMerchantTemplateName = queryTemplate.FirstOrDefault(x => x.TemplateID == item.IncludeMerchantTemplateID)?.DisplayName;
                }
            }

            var now = DateTimeHelper.GetCurrentTime();

            var inOnlyOneActive = items.Where(x => !x.IsExpired).Count() == 1;

            if (inOnlyOneActive)
            {
                foreach (var item in items)
                {
                    if (!item.IsExpired)
                    {
                        var msgGenerator = GenerateSingleGameVendorMaintainMessage(item.StartTime, item.EndTime, (GameVendorTypeEnum)item.VendorID);
                        item.MaintainMessage = msgGenerator.ConcateAllMessage;
                        item.TgApiTemplateMessage = msgGenerator.TgApiTemplateMessage;
                    }
                }
            }

        }



        public CRUDResultModel Create(int userID, VendorMaintenanceCreateViewModel model)
        {
            model.RuleFor(x => x.VendorID).Rule_NotMinus();
            model.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);

            DateTime now = DateTimeHelper.GetCurrentTime();
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<int> targetVendors = new List<int>();
            targetVendors.Add(model.VendorID);

            if (model.VendorID == SystemConstant.MaintainAllVendors)
            {
                targetVendors = AppHelper.GetReportAvailableGameVendors()
                    .Select(z => z.ToInteger())
                    .ToList();
            }

            int resultID = 0;
            foreach (var vendorID in targetVendors)
            {
                var newEntity = model.ReturnInjecteObject<VendorMaintenanceEntity>();

                newEntity.VendorID = vendorID;
                newEntity.StartTime = SetToZeroSeconds(newEntity.StartTime);
                newEntity.EndTime = SetToZeroSeconds(newEntity.EndTime);
                newEntity.Status = NormalActiveStatus.Enable.ToInteger();
                newEntity.CreateTime = now;
                newEntity.CreateBy = currentUser.UserName;
                newEntity.UpdateTime = now;
                newEntity.UpdateBy = currentUser.UserName;

                resultID = VendorMaintenanceRepository.Create(newEntity);

                //清除cache
                CacheRemoveHelper.RemoveCache_WhenVendorMaintainSettingUpdate(vendorID);
            }

            return new CRUDResultModel
            {
                ResultId = resultID,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public void UpdateExcludeMerchantTemplate(int userID, int id, int templateID)
        {
            var queryExist = VendorMaintenanceRepository.GetById(id);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (templateID <= 0)
            {
                queryExist.ExcludeMerchantTemplateID = null;
            }
            else
            {
                queryExist.ExcludeMerchantTemplateID = templateID;
            }

            //強制清空另一邊的配置
            queryExist.IncludeMerchantTemplateID = null;

            VendorMaintenanceRepository.Update(queryExist);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenVendorMaintainSettingUpdate(queryExist.VendorID);
        }

        public void UpdateIncludeMerchantTemplate(int userID, int id, int templateID)
        {
            var queryExist = VendorMaintenanceRepository.GetById(id);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (templateID <= 0)
            {
                queryExist.IncludeMerchantTemplateID = null;
            }
            else
            {
                queryExist.IncludeMerchantTemplateID = templateID;
            }

            //強制清空另一邊的配置
            queryExist.ExcludeMerchantTemplateID = null;

            VendorMaintenanceRepository.Update(queryExist);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenVendorMaintainSettingUpdate(queryExist.VendorID);
        }



        public void RemoveAllMaintain(int userID)
        {
            var queryActiveMaintain = GetByPagingFilter(userID, new QueryVendorMaintenanceApiRequestParameterModel
            {
                PageSize = 30,
            });

            var mustDeleteItems = queryActiveMaintain.Items
                .Where(x => !x.IsExpired)
                .ToList();

            if (mustDeleteItems.IsEmptyOrNull())
                return;

            foreach (var item in mustDeleteItems)
            {
                VendorMaintenanceRepository.Delete(item.Id);

                CacheRemoveHelper.RemoveCache_WhenVendorMaintainSettingUpdate(item.VendorID);
            }
        }


        public CRUDResultModel Delete(int userID, int targetID)
        {
            var queryExsit = VendorMaintenanceRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.Status = NormalActiveStatus.PermanentDelete.ToInteger();

            VendorMaintenanceRepository.Delete(targetID);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenVendorMaintainSettingUpdate(queryExsit.VendorID);

            return new CRUDResultModel(true);
        }


        private VendorMaintenanceViewModel ConvertToViewModel(VendorMaintenanceEntity query)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<VendorMaintenanceViewModel>();
            convert.VendorName = ((GameVendorTypeEnum)query.VendorID).ToString();
            convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)query.Status);
            convert.IsExpired = now > query.EndTime;
            convert.TotalMinutes = (int)(query.EndTime - query.StartTime).TotalMinutes;

            var msgGenerator = GenerateMaintainMessage(query.StartTime, query.EndTime);
            convert.MaintainMessage = msgGenerator.ConcateAllMessage;
            convert.TgApiTemplateMessage = msgGenerator.TgApiTemplateMsg;

            if (convert.StartTime > now)
            {
                int minutes = ((int)(convert.StartTime - now).TotalMinutes);
                convert.ExtraHintMsg = DateTimeHelper.GetDiffDisplayTime(minutes, isIncludeParentheses: true, "後");
                convert.IsUpComming = true;
            }

            if (convert.StartTime < now && convert.EndTime > now)
            {
                convert.IsProgressing = true;
            }

            return convert;
        }


        private DateTime SetToZeroSeconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }


        private string BeginTimeEndTimeText(DateTime beginUTC, DateTime endTimeUTC, double timeZone)
        {
            DateTime beginTimeZone = beginUTC.AddHours(timeZone);
            DateTime endTimeZone = endTimeUTC.AddHours(timeZone);

            var text = $"{beginTimeZone.ToString("HH:mm")}:~{endTimeZone.ToString("HH:mm")} (GMT+{((int)timeZone)})";
            return text;
        }

        private string GetDayOfWeekTextEN(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case 0:
                    return "Sun.";
                case 1:
                    return "Mon.";
                case 2:
                    return "Tues.";
                case 3:
                    return "Wed.";
                case 4:
                    return "Thur.";
                case 5:
                    return "Fri.";
                case 6:
                    return "Sat.";
                default:
                    return "";
            }
        }

        private string GetDayOfWeekTextCN(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case 0:
                    return "日";
                case 1:
                    return "一";
                case 2:
                    return "二";
                case 3:
                    return "三";
                case 4:
                    return "四";
                case 5:
                    return "五";
                case 6:
                    return "六";
                default:
                    return "";
            }
        }


        private string GenerateENMessage(DateTime beginUTC, DateTime endTimeUTC, double timeZone)
        {
            DateTime beginTime_timezone = beginUTC.AddHours(timeZone);
            string beginTimeText = BeginTimeEndTimeText(beginUTC, endTimeUTC, timeZone);

            string dateText = MultiLanguageTextHelper.GetDateTextEN(beginUTC, timeZone);
            string dayOfWeek = GetDayOfWeekTextEN((int)beginTime_timezone.DayOfWeek);

            string dynamicMsg = "a temporary maintenance";

            if (beginUTC.AddHours(8).DayOfWeek == DayOfWeek.Tuesday)
            {
                dynamicMsg = "an upgrade maintenance";
            }

            string brandName = GetDisplayBrandName();
            string newLine = Environment.NewLine;

            string msgEN = $"【{brandName} System maintenance notice】" + newLine;
            msgEN += "Dear partners:" + newLine;
            msgEN += $"We will perform {dynamicMsg}, which will bring you inconvenience please understand. " + newLine;
            msgEN += $"Date：{dateText} ({dayOfWeek})" + newLine;
            msgEN += $"Time：{beginTimeText}" + newLine;
            msgEN += $"Affected Service：All Games" + newLine;
            msgEN += $"Environment：Production & Staging" + newLine;

            return msgEN;
        }


        private string GenerateCNMessage(DateTime beginUTC, DateTime endTimeUTC, double timeZone)
        {
            DateTime beginTime_timezone = beginUTC.AddHours(timeZone);
            DateTime endTime_timezone = endTimeUTC.AddHours(timeZone);

            string hourText = $"{beginTime_timezone.ToString("HH:mm")}~{endTime_timezone.ToString("HH:mm")} (GMT+{(int)timeZone})";
            string dayOfWeek = GetDayOfWeekTextCN((int)beginTime_timezone.DayOfWeek);

            string dynamicMsg = "临时维护作业";

            if (beginUTC.AddHours(8).DayOfWeek == DayOfWeek.Tuesday)
            {
                dynamicMsg = "维护作业";
            }

            string brandName = GetDisplayBrandName();
            string newLine = Environment.NewLine;
            string msg = $"【{brandName} 系统维护通知】" + newLine;
            msg += "致尊敬的合作伙伴：" + newLine;
            msg += $"系统预计在以下时间进行{dynamicMsg}，给您带来不便敬请谅解。{newLine}";
            msg += $"日期：{beginTime_timezone.Year}年{beginTime_timezone.Month}月{beginTime_timezone.Day}日 ({dayOfWeek})" + newLine;
            msg += $"时间：{hourText}" + newLine;
            msg += $"影响服务：全游戏" + newLine;
            msg += $"影响环境：Production & Staging" + newLine;

            return msg;
        }

        private string GetDisplayBrandName()
        {
            return SettingHelper.PromoteBrandCodeName;
        }



        private string GenerateIDRMessage(DateTime beginUTC, DateTime endTimeUTC, double timeZone)
        {
            DateTime beginTime_timezone = beginUTC.AddHours(timeZone);
            string beginTimeText = BeginTimeEndTimeText(beginUTC, endTimeUTC, timeZone);

            string dateText = MultiLanguageTextHelper.GetDateTextEN(beginUTC, timeZone);
            string dayOfWeek = GetDayOfWeekTextEN((int)beginTime_timezone.DayOfWeek);

            string bradingName = "ION Slots";
            string newLine = Environment.NewLine;
            string msgEN = $"【PENGUMUMAN maintenance sistem {bradingName}】" + newLine;
            msgEN += "Dear partners:" + newLine;
            msgEN += $"Akan ada Maintenance Reguler pada. " + newLine;
            msgEN += $"Tanggal：{dateText} ({dayOfWeek})" + newLine;
            msgEN += $"Waktu：{beginTimeText}" + newLine;
            msgEN += $"Service yang terpengaruh：Semua Games" + newLine;
            msgEN += $"Mohon maaf atas ketidaknyamanannya, Mohon dimaklumi." + newLine;

            return msgEN;
        }



        private (string ConcateAllMessage, string TgApiTemplateMsg) GenerateMaintainMessage(DateTime beginUTC, DateTime endUTC)
        {
            if ((endUTC - beginUTC).TotalHours >= 24)
            {
                return ("(*複製失敗!) 時間區間超過24小時的維護 無法產生複製訊息，請自行擬定文案", "");
            }

            string newLine = Environment.NewLine;

            StringBuilder sb = new StringBuilder();

            string msgEN_8 = GenerateENMessage(beginUTC, endUTC, 8) + newLine + newLine;
            string msgCN_8 = GenerateCNMessage(beginUTC, endUTC, 8) + newLine + newLine;

            sb.Append(msgEN_8);
            sb.Append(msgCN_8);

            sb.Append(GenerateENMessage(beginUTC, endUTC, 7) + newLine + newLine);
            sb.Append(GenerateCNMessage(beginUTC, endUTC, 7) + newLine + newLine);
            sb.Append(GenerateIDRMessage(beginUTC, endUTC, 7) + newLine + newLine);

            sb.Append(newLine + "請幫忙通知客戶 謝謝。(Skype)" + newLine
                + "公告訊息(大量發送訊息)請使用電腦裝置PC來操作、避免文字排版亂掉 " + newLine
                + "有部分客戶需要 @全部 請不要忘記 " + newLine
                + "並於維護結束15分鐘後與我方確認維護是否結束。" + newLine);

            return (sb.ToString(), msgEN_8 + msgCN_8);
        }


        private (string ConcateAllMessage, string TgApiTemplateMessage) GenerateSingleGameVendorMaintainMessage(DateTime beginUTC, DateTime endTimeUTC, GameVendorTypeEnum gameVendor)
        {
            if ((endTimeUTC - beginUTC).TotalHours >= 24)
            {
                return ("(*複製失敗!) 時間區間超過24小時的維護 無法產生複製訊息，請自行擬定文案", "");
            }

            string newLine = Environment.NewLine;
            string msg = string.Empty;

            double timeZone = 8;

            DateTime beginTime_timezone = beginUTC.AddHours(timeZone);
            DateTime endTime_timezone = endTimeUTC.AddHours(timeZone);

            //若為當日維護 視為臨時停機 (文案稍微改變)
            bool isTodayMaintain = beginTime_timezone.Date == DateTimeHelper.GetCurrentTime(8).Date;

            string beginTimeText = BeginTimeEndTimeText(beginUTC, endTimeUTC, timeZone);
            string dateText = MultiLanguageTextHelper.GetDateTextEN(beginUTC, timeZone);
            string dayOfWeekEN = GetDayOfWeekTextEN((int)beginTime_timezone.DayOfWeek);
            string dayOfWeekCN = GetDayOfWeekTextCN((int)beginTime_timezone.DayOfWeek);

            string bradingName = "target/ION";
            string bradingNameCN = "target/ION";
            string affectGames = "All Games";

            int gameID_min = gameVendor.ToInteger() + 1;
            int gameID_max = gameVendor.ToInteger() + 999;

            //各遊戲商略有文案差異
            if (gameVendor == GameVendorTypeEnum.Galaxy)
            {
                bradingName = "Live casino game (Tiger9)";
                bradingNameCN = "视讯游戏 (Tiger9 Live) ";
                affectGames = $"Tiger9 Live (gameID: {gameID_min}~{gameID_max})";
            }
            else if (gameVendor == GameVendorTypeEnum.AGG)
            {
                affectGames = $"Slots games (gameID: {gameID_min}~{gameID_max})";
            }
            else if (gameVendor == GameVendorTypeEnum.AGG)
            {
                affectGames = $"Live casino (gameID: {gameID_min}~{gameID_max})";
            }
            else if (gameVendor == GameVendorTypeEnum.Jili)
            {
                bradingName = "Jili game";
                bradingNameCN = "Jili game (吉利電子)";
                affectGames = $"Jili game (gameID: {gameID_min}~{gameID_max})";
            }
            else if (gameVendor == GameVendorTypeEnum.target)
            {
                affectGames = $"GameID: 1000~9999";
            }
            else
            {
                affectGames = $"GameID: {gameID_min}~{gameID_max}";
            }

            string describeTextEN = isTodayMaintain ? "temporary" : "upgrade"; 
            string describeTextCN = isTodayMaintain ? "临时" : ""; 

            string msgEN = $"【{bradingName} Maintenance Notice (Specific Games Only)】" + newLine;
            msgEN += "Dear partners:" + newLine;
            msgEN += $"We will perform a {describeTextEN} maintenance. Only affect specific games, other games are not affected. (Refer the gameIDs) " + newLine;
            msgEN += $"Date：{dateText} ({dayOfWeekEN})" + newLine;
            msgEN += $"Time：{beginTimeText}" + newLine;
            msgEN += $"Affected Service：{affectGames}" + newLine;
            msgEN += $"Environment：Production & Staging" + newLine;


            string hourText = $"{beginTime_timezone.ToString("HH:mm")}~{endTime_timezone.ToString("HH:mm")} (GMT+{(int)timeZone})";
            string msgCN = $"【{bradingNameCN} 系统维护通知】" + newLine;
            msgCN += "致尊敬的合作伙伴：" + newLine;
            msgCN += $"系统预计对特定幾款游戏进行{describeTextCN}维护作业，本次维护仅针对「特定游戏」。其他游戏将不受影响 (请参考gameID)。{newLine}";
            msgCN += $"日期：{beginTime_timezone.Year}年{beginTime_timezone.Month}月{beginTime_timezone.Day}日 ({dayOfWeekCN})" + newLine;
            msgCN += $"时间：{hourText}" + newLine;
            msgCN += $"影响服务：{affectGames}" + newLine;
            msgCN += $"影响环境：Production & Staging" + newLine;

            msg = msg + msgEN + newLine + newLine;
            msg = msg + msgCN + newLine + newLine;

            msg = msg + newLine + "請幫忙通知客戶 謝謝。(TG/Skype)" + newLine
                + "本次維護公告(*可能)只需要發送特定客戶" + newLine
                + "公告訊息(大量發送訊息)請使用電腦裝置PC來操作、避免文字排版亂掉 " + newLine;

            return (msg, msgEN + newLine + msgCN + newLine);
        }

    }

}
