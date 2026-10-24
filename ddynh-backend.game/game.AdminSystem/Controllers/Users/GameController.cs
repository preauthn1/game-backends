using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Game;
using game.Entity.PerLog;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Service.DAO;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-游戏查询
    /// </summary>
    public class GameController : BaseController
    {

        public GameServerListRepository GameServerListRepository { get; set; }



        /// <summary>
        /// 3-游戏信息查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GameUserGameInfo(int? userID = null, string loginName = "", int? gameID = null , int p = 1, int pageSize = 20 , bool isExport = false)
        {
            int adminID = GetCurrentUserID();

            var myPlatform = DataService.GetPlatformByAdminId(adminID);

            string platformIDs = DataService.GetPlatformIdsTextByAdminID(adminID);

            if(string.IsNullOrWhiteSpace(platformIDs))
                return View();

            p = isExport ? 1 : p;
            pageSize = isExport ? SystemConst.ExcelMaxExportSize : pageSize;
                        
            string url = $"/Game/GetUserGameInfoPaging?UserIDsText={userID}&GameIDsText={gameID}&loginName={loginName}&PlatformIDsText={platformIDs}&pageSize={pageSize}&pageIndex={p}";
            var list = SiteHelper.GetAPIResult<PagedList<UserGameInfoViewModel>>(url);
            
            if(isExport)
            {
                string fileName = $"Export_GameUserGameInfo.xlsx";
                var stream = ExcelModelConvertHelper.ConvertToGameUserGameInfoExcelStream(list.PageData);
                return File(stream, Infrastructure.SystemConst.Excel_Format_ContentType, fileName);
            }
            
            return View(list);
        }

        /// <summary>
        /// 3-游戏记录查询
        /// </summary>

        public ActionResult GameLog()
        {
            if (Session["userID"] != null)
            {
                ViewBag.userID = int.Parse(Session["userID"].ToString());
            }
            return View();
        }

        /// <summary>
        /// 3-游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="serverID">服务器ID</param>
        /// <param name="gameID">游戏</param>
        /// <param name="ipAddress"></param>
        /// <param name="hDtype"></param>
        /// <param name="hDcode"></param>
        /// <param name="loginBeginTime"></param>
        /// <param name="loginEndTime"></param>
        /// <param name="logoutBeginTime"></param>
        /// <param name="logoutEndTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetGameLog(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "",
            int? serverID = null, int? gameID = null, string ipAddress = null, Entity.Users.HDType? hDtype = null, string hDcode = null,
            DateTime? loginBeginTime = null, DateTime? loginEndTime = null, DateTime? logoutBeginTime = null, DateTime? logoutEndTime = null)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            userID = UserService.GetUserID(userID, loginName, nickName);

            if (userID == null && Session["userID"] != null)
            {
                userID = int.Parse(Session["userID"].ToString());
            }
            else
            {
                Session["userID"] = userID;
            }

            if (userID == null)
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    success = true,
                    data = new { }
                });
            }

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<FW.Core.AdminSys.Provider.APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={userID}");
            if (!isPermission.Result) return Json(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                success = true,
                data = new { }
            });

            if (logoutBeginTime == null) logoutBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var AdminID = Convert.ToInt32(operatorID);
            var api = SiteHelper.GetAPIResult<PagedList<GameLog>>($"/Game/GetGameLog?serverID={serverID}&gameID={gameID}&userID={userID}&ipAddress={ipAddress}&hDtype={hDtype}&hDcode={hDcode}"
                + $"&loginBeginTime={loginBeginTime}&loginEndTime={loginEndTime}&logoutBeginTime={logoutBeginTime}&logoutEndTime={logoutEndTime}&pageIndex={pageIndex}&pageSize={length}&AdminID={AdminID}");

            var query = from pro in api.PageData
                        select new
                        {
                            Platform = pro.User.Area.Platform.Name,
                            UserID = pro.UserID,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            ServerName = pro.GameServer.ServerName,
                            GameID = pro.GameServer.GameID,
                            LoginMoney = AppLogicHelper.ShowCashAmountOrDashText(pro.GameServer.GameID , pro.LoginMoney),
                            Amount = pro.Amount.ToCash(),
                            TaxAmount = pro.TaxAmount.ToCash(),
                            WinCount = pro.WinCount,
                            LoseCount = pro.LoseCount,
                            DrawCount = pro.DrawCount,
                            GameTime = pro.GameTime,
                            IPAddress = pro.IPAddress,
                            HDType = EnumExtensions.GetDisplayName(pro.HDType),
                            HDCode = pro.HDCode,
                            LoginTime = pro.LoginTime.ToString("yyyy/MM/dd hh:mm"),
                            LogoutTime = pro.LogoutTime.ToString("yyyy/MM/dd hh:mm")
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        /// <summary>
        /// 每局游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="dealNo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="p"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult DetailLog(int? userID = null, string loginName = "", string nickName = "", int? gameID = null, int? serverID = null, long? dealNo = null, DateTime? beginTime = null, DateTime? endTime = null , int p = 1, int pageSize = 20 , bool IsExport = false)
        {
            int operatorID = GetCurrentUserID();

            //若user查無此人則不查詢
            var queryUser = UserService.GetUser(null, loginName, nickName);

            if (queryUser != null)
                userID = queryUser.Id;

            DateTime tempBegin = beginTime ?? DateTime.Now;
            DateTime tempEnd = endTime ?? DateTime.Now;

            if (IsExport)
            {
                int maxSize = 2000;
                int maxDays = 7;
                if (!userID.HasValue && (tempEnd - tempBegin).TotalDays >= (maxDays + 1))
                {
                    return Alert($"若选择汇出报表，则起始、结束日期 不得超过{maxDays}日 或 必须填写用户ID。且汇出数据上限 {maxSize} 笔。");
                }
                
                p = 1;
                pageSize = maxSize; //最多2000筆
            }                        

            string url = string.Format("/PerGameLog/GetDetailLog?userID={0}&gameID={1}&serverID={2}&dealNo={3}&beginTime={4}&endTime={5}&pageIndex={6}&pageSize={7}&AdminID={8}"
                , userID, gameID, serverID, dealNo, beginTime, endTime, p, pageSize, operatorID);

            var queryApi = SiteHelper.GetAPIResult<PagedList<DetailLog>>(url);
            
            SetGameIDByServer(queryApi.PageData);


            if (IsExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("Export_PerGameDetailLog", tempBegin, tempEnd);
                var stream = ExcelModelConvertHelper.ConvertToPerGameLogDetailLogExcelStream(queryApi.PageData);
                return File(stream, Infrastructure.SystemConst.Excel_Format_ContentType, fileName);
            }            

            return View(queryApi);            
        }

        public ActionResult GetDetailLogByLogID(string logID, DateTime gametime)
        {
            var data = SiteHelper.GetAPIResult<List<DetailLog>>(string.Format("/PerGameLog/GetDetailLogByLogID?logID={0}&gametime={1}", logID, gametime.ToString("yyyy-MM-dd HH:mm:ss")));

            var queryMyPlatformIDs = DataService.GetMyPlatforms().Select(x=> x.Id).ToList();

            string userIDs = string.Join("," , data.Select(z => z.UserID));

            //查出這些用戶 是否來自其他平台(顯示名字要打碼移除)
            var queryUsers = SiteHelper.GetAPIResult<List<UserInfoBriefModel>>($"/user/GetUserBriefByIDs?userIDsText={userIDs}");
            List<int> maskUserIDs = queryUsers.Where(z => !queryMyPlatformIDs.Contains(z.PlatformID)).Select(x=> x.UserID).ToList();

            var retObj = data.Select(m => new
            {
                m.Id,
                m.LogID,
                m.ServerID,
                m.ServerName,
                m.GameName,
                m.UserID,
                LoginName = MaskTextOrNot(maskUserIDs , m.UserID , m.LoginName),
                NickName = MaskTextOrNot(maskUserIDs, m.UserID, m.NickName),
                RealName = MaskTextOrNot(maskUserIDs, m.UserID, m.RealName),
                UnionpayRealName = m.UnionpayRealName == null ? "--" : m.UnionpayRealName,
                m.GradeID,
                CurrentAmount = m.CurrentAmount.ToCash(),
                UpdateAmount = m.UpdateAmount.ToCash(),
                m.GameTime,
                m.LastLogonIPAddress,
                UserType = Convert.ToString(m.UserType).Replace("1", "正式帐号").Replace("2", "机器人").Replace("3", "游客")
            });

            var dateTimeConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss" };
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(retObj, Newtonsoft.Json.Formatting.Indented, dateTimeConverter));
        }




        private void SetGameIDByServer(List<DetailLog> query)
        {
            if (query.IsEmptyOrNull())
                return;

            var serverIDs = query.Select(x => x.ServerID).Distinct().ToList();

            var queryServers = GameServerListRepository.GetByIds(serverIDs);

            foreach (var item in query)
            {
                var matchServer = queryServers.FirstOrDefault(x => x.ServerID == item.ServerID);

                if (matchServer != null)
                {
                    item.GameID = matchServer.GameID;
                }
            }
        }



        private string MaskTextOrNot(List<int> maskUserIDs , int userID , string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return "--";

            if (maskUserIDs.Contains(userID))
                return "--";

           //return inputText.GetMaskPrivateText(1);
            return inputText;
        }


        public ActionResult GetPerGameLogByLogID(string logID, DateTime gametime)
        {
            var data = SiteHelper.GetAPIResult<PerGameLog>(string.Format("/PerGameLog/GetPerGameLogByLogID?logID={0}&gametime={1}", logID, gametime.ToString("yyyy-MM-dd HH:mm:ss")));
            var retObj = new { data.Id, data.ServerID, data.ServerName, data.GameName, TaxAmount = data.TaxAmount.ToCash(), data.GameData, data.BeginTime, data.EndTime };
            var dateTimeConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss" };
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(retObj, Newtonsoft.Json.Formatting.Indented, dateTimeConverter));
        }
    }
}