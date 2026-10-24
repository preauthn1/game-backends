using System;
using System.Collections.Generic;
using System.Web.Http;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class UserController : BaseAPIController
    {
        private IUserService _uService;
        private IMoneyService _moneyService;
        private IForbidChatService _forbidChatService;
        private IForbidUserService _forbidUserService;
        private IGameService _gameService;
        private IPayService _payService;
        private IFlowAmountLimitService _flowAmountLimitService;


        public UserController(
            IUserService uService, 
            IMoneyService moneyService, 
            IForbidChatService forbidChatService, 
            IForbidUserService forbidUserService, 
            IGameService gameService, 
            IPayService payService,
            IFlowAmountLimitService flowAmountLimitService)
        {
            _uService = uService;
            _moneyService = moneyService;
            _forbidChatService = forbidChatService;
            _forbidUserService = forbidUserService;
            _gameService = gameService;
            _payService = payService;
            _flowAmountLimitService = flowAmountLimitService;
        }


        [HttpGet]
        public UserInfo GetUserByID(int userID)
        {
            return _uService.GetUserByID(userID);
        }


        [HttpGet]
        public List<UserInfoBriefModel> GetUserBriefByIDs(string userIDsText)
        {
            var query = _uService.GetUserBriefByIDs(userIDsText);
            return query;
        }


        [HttpGet]
        public UserInfo GetUserByLoginName(string loginName)
        {
            return _uService.GetUserByLoginName(loginName);
        }

        [HttpGet]
        public UserInfo GetUserByNickName(string nickName)
        {
            return _uService.GetUserByNickName(nickName);
        }

        [HttpGet]
        public PagedList<UserInfo> GetUser(int? userID = null, int? areaID = null, string loginName = null, string nickName = null, DateTime? regBeginTime = null, DateTime? regEndTime = null, DateTime? lastlogonBeginTime = null, DateTime? lastlogonEndTime = null,
            string regIpAddress = null, UserType? userType = null, HDType? hdType = null, string hdCode = null, string lastlogonIPAddress = null, Sex? sex = null, decimal? convertChargeRateBegin = null, decimal? convertChargeRateEnd = null,
            long? chargeAmountBegin = null, long? chargeAmountEnd = null, long? convertAmountBegin = null, long? convertAmountEnd = null, int? gradeID = null, string unionpayAccount = null, string alipayAccount = null, string realName = null, int pageIndex = 1, int pageSize = 20)
        {
            return _uService.GetUser(userID, loginName, nickName, regBeginTime, regEndTime, lastlogonBeginTime, lastlogonEndTime, regIpAddress, userType, hdType, hdCode, lastlogonIPAddress, sex, convertChargeRateBegin, convertChargeRateEnd, chargeAmountBegin, chargeAmountEnd, convertAmountBegin, convertAmountEnd, gradeID, unionpayAccount, alipayAccount, realName, areaID, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<UserInfo2> GetUser2([FromUri]QueryAllUsersSPRequestViewModel filter)
        {
            var query = _uService.GetAllUserByFilter(filter);
            return query;
        }


        [HttpGet]
        public PagedList<RegUserInfo> GetRegUser(int? platformid, int? adminId, int? userID = null, int? areaID = null, DateTime? regBeginTime = null, DateTime? regEndTime = null,
          string regIpAddress = null, HDType? hdType = null, int pageIndex = 1, int pageSize = 20, SortName? sortName = SortName.StatTime, SortLifting? sortLifting = SortLifting.DESC)
        {
            return _uService.GetRegUser(platformid, adminId, userID, areaID, regBeginTime, regEndTime, regIpAddress, hdType, pageIndex, pageSize, sortName, sortLifting);
        }

        [HttpGet]
        public APIResult ModifyUserPassword(int userID, string newpwd, string ipAddress)
        {
            string message = "";
            bool result = _uService.ModifyUserPassword(userID, newpwd, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyUserBankPassword(int userID, string newpwd, string ipAddress)
        {
            string message = "";
            bool result = _uService.ModifyUserBankPassword(userID, newpwd, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public string EncrypePwd(string pwd)
        {
            return _uService.EncrypePwd(pwd);
        }

        [HttpGet]
        public APIResult ModifyUserLoginName(int userID, string loginName, string ipAddress)
        {
            string message = "";
            bool result = _uService.ModifyUserLoginName(userID, loginName, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyNickName(int userID, string nickName, string ipAddress)
        {
            string message = "";
            bool result = _uService.ModifyNickName(userID, nickName, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public string GetContestUserTicketSummary(int userID)
        {
            return _uService.GetContestUserTicketSummary(userID);
        }

        [HttpGet]
        public APIResult GetOperatorPermission(int operatorID, int userID, string loginName = "")
        {
            string message = "";
            bool result = _uService.GetOperatorPermission(operatorID, userID, loginName);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<UserInfoChangeLog> GetUserInfoChangeLog(int operatorID, int? userID = null, UserInfoChangeType? type = null, string ipAddress = null, DateTime? createTimeBegin = null, DateTime? createTimeEnd = null,
            int pageIndex = 1, int pageSize = 20)
        {
            return _uService.GetUserInfoChangeLog(operatorID, userID, type, ipAddress, createTimeBegin, createTimeEnd, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<UserTicket> GetUserTicket(int gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _uService.GetUserTicket(gameID, userID, beginTime, endTime, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<UserRank> GetUserInfoRankLog(int? gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _uService.GetUserInfoRankLog(gameID, userID, beginTime, endTime, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<UserFlowLimit> GetFlowAmountLimitLog([FromUri]QueryFlowAmountLimitRequestViewModel filter)
        {
            return _flowAmountLimitService.GetFlowAmountLimitLog(filter);
        }

        [HttpPost , HttpPut]
        public CRUDResultModel UpdateFlowAmountLimitLogStatus([FromBody]CommonUpdateStatusRequestModel model)
        {
            var result = _flowAmountLimitService.UpdateStatus(model);
            return result;
        }
        
        /// <summary>
        /// 获取用户详细统计信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet]
        public UserDetailInfoDTO GetUserDetailInfo(int userID, DateTime? sTimeBegin = null, DateTime? sTimeEnd = null)
        {
            return _uService.GetUserDetailInfo(userID, sTimeBegin, sTimeEnd);
        }

        /// <summary>
        /// 获取用户详细数据
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet]
        public UserInfoDetail GetUserInfoDetail(int userID, DateTime? beginTime, DateTime? endTime)
        {
            var user = _uService.GetUserByID(userID);
            if (user == null) return null;
            var cash = _moneyService.GetUserCash(userID) ?? new Cash();
            var bank = _moneyService.GetUserBank(userID) ?? new Bank();
            UserInfoDetail detail = new UserInfoDetail();
            
            detail.UserID = user.Id;
            detail.FaceID = user.FaceID;
            detail.LoginName = user.LoginName;
            detail.NickName = user.NickName;
            detail.Sex = user.Sex;
            detail.HDType = user.HDType;
            detail.HDCode = user.HDCode;
            detail.UserType = user.UserType;
            detail.CreateTime = user.CreateTime;
            detail.CreateIPAddress = user.CreateIPAddress;
            detail.LastLogonTime = user.LastLogonTime;
            detail.LastLogonIPAddress = user.LastLogonIPAddress;
            detail.LastLogonHDCode = user.LastLogonHDCode;
            detail.RealName = user.RealName;
            detail.AlipayAccount = user.AlipayAccount;
            detail.UnionpayAccount = user.UnionpayAccount;
            detail.UnionpayRealName = user.UnionpayRealName;
            detail.ExperienceCash = user.ExperienceCash;
            detail.SumCharge = user.SumCharge;
            detail.SumAgentCharge = user.SumAgentCharge;
            detail.SumChargeTotal = user.SumCharge + user.SumAgentCharge + (user.SumFreeMoney == null ? 0 : user.SumFreeMoney.Value);
            detail.SumLoginTimes = user.SumLoginTimes;
            detail.SumTax = user.SumTax;
            detail.SumConvert = user.SumConvert;
            detail.SumConvertTotal = user.SumConvert + user.SumConvertTax;
            detail.SumConvertTimes = user.SumConvertTimes;
            detail.SumConvertTax = user.SumConvertTax;
            detail.SumFreeMoney = user.SumFreeMoney == null ? 0 : user.SumFreeMoney.Value;
            detail.SumAdminMoney = user.SumAdminMoney == null ? 0 : user.SumAdminMoney.Value;
            detail.QuestionCount = user.QuestionCount;
            detail.GradeID = user.GradeID;
            detail.AreaID = user.AreaID;
            detail.AreaName = user.Area.AreaName;
            detail.PlatformID = user.Area.PlatformID;
            detail.PlatformName = user.Area.Platform.Name;
            detail.CashAmount = cash.Amount;
            detail.BankAmount = bank.Amount;
            detail.StatInfo = _uService.GetUserDetailInfo(userID, beginTime, endTime);
            detail.StatGameInfo = _gameService.GetStatUserGameInfo(userID, beginTime, endTime);
            detail.SumFlowAmount = user.SumFlowAmount;

            if (_forbidUserService.GetForbidUserByID(userID) == null)
            {
                detail.ForbidUser = "未封号";
            }
            else
            {
                detail.ForbidUser = "已被封号";
            }

            if (!String.IsNullOrEmpty(user.AlipayAccount) && _forbidUserService.GetForbidAccount(user.AlipayAccount, null, null, null).Find(x => x.IdType == 2) == null)
                detail.ForbidAlipay = "未封号";
            else
                detail.ForbidAlipay = "已被封号";

            if (!String.IsNullOrEmpty(user.UnionpayAccount) && _forbidUserService.GetForbidAccount(user.UnionpayAccount, null, null, null).Find(x => x.IdType == 3) == null)
                detail.ForbidUnionpay = "未封号";
            else
                detail.ForbidUnionpay = "已被封号";


            if (_forbidChatService.GetForbidChatUserByID(userID) == null)
            {
                detail.ForbidChat = "允许";
            }
            else
            {
                detail.ForbidChat = "不允许";
            }

            detail.GameInfo = _gameService.GetUserGameInfo(userID, null);
            return detail;

        }

        public List<QuestionLogViewModel> GetQuestionUserName(int adminUserID, int? IncludeUser)
        {
            return _uService.GetQuestionUserName(adminUserID, IncludeUser);
        }
        
        public PagedList<QuestionLog> GetUserQuestionLog(int adminUserID, string platformIds, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize)
        {
            return _uService.GetUserQuestionLog(adminUserID, platformIds, UserID, Status, string.IsNullOrWhiteSpace(handlePerson) ? null : handlePerson, StartTime, EndTime, IncludeUser, pageIndex, pageSize);
        }

        public PagedList<QuestionLog> GetUserQuestionLogAll(int adminUserID, string platformIds, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize)
        {
            return _uService.GetUserQuestionLogAll(adminUserID, platformIds, UserID, Status, string.IsNullOrWhiteSpace(handlePerson) ? null : handlePerson, StartTime, EndTime, IncludeUser, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取用户登入階段投诉问题
        /// </summary>
        /// <param name="platformIds"></param>
        /// <param name="Status"></param>
        /// <param name="handlePerson"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public FW.Core.Helper.PagedList<QuestionLog> GetUserLogonQuestionLog(int? AdminID, int? platformIds, string questionType, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int pageIndex, int pageSize)
        {

            return _uService.GetUserLogonQuestionLog(AdminID, platformIds,
                string.IsNullOrWhiteSpace(questionType) ? null : questionType,
                Status,
                string.IsNullOrWhiteSpace(handlePerson) ? null : handlePerson,
                StartTime, EndTime, pageIndex, pageSize);

        }
        public int GetUnReplyQuestionCount(string platformIDs, string handlePerson)
        {
            return _uService.GetUnReplyQuestionCount(platformIDs, handlePerson);
        }

        /// <summary>
        /// 获取用户登入階段投诉问题未處理的筆數
        /// </summary>
        /// <param name="platformIDs"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int GetLogonQuestionCount(string platformIDs, int status)
        {
            return _uService.GetLogonQuestionCount(platformIDs, status);
        }
        [HttpGet]
        public APIResult AddQuestionLog(int userID, string question, string ipAddress)
        {
            var message = "";
            var result = _uService.AddQuestionLog(userID, question, ipAddress, ref message);
            return new APIResult(result, message);
        }
        [HttpGet]
        public APIResult ModifyQuestionLog(int Id, string handlePerson)
        {
            var message = "";
            var result = _uService.ModifyQuestionLog(Id, handlePerson, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteQuestionLogById(int Id)
        {
            var message = "";
            var result = _uService.DeleteQuestionLogById(Id, ref message);
            return new APIResult(result, message);
        }
        /// <summary>
        /// 回复用户邮件
        /// </summary>
        /// <param name="UserId">用户Id</param>
        /// <param name="msg">回复消息</param>
        /// <param name="title">标题</param>
        /// <param name="qid">问题ID</param>
        /// <returns></returns>
        [HttpGet]
        public APIResult ReplayMessage(int userid, string title, string msg, int qid, string handelPerson)
        {
            var message = "";
            var result = _uService.ReplayMessage(userid, qid, title, msg, handelPerson, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public void ReplyMessageCancel(int questionID, int msgIndex)
        {
            _uService.ReplyMessageCancel(questionID, msgIndex);
        }

        [HttpGet]
        public List<QuestionLog> GetUserQuestionLogNearbyById(int userID, int questionID, int count)
        {
            return _uService.GetUserQuestionLogNearbyById(userID, questionID, count);
        }
        [HttpGet]
        public QuestionLog GetQuestionById(int Id)
        {
            return _uService.GetQuestionById(Id);
        }
        [HttpGet]
        public void ResetAlipay(int userid, string ip)
        {
            _uService.ReSetAlipayAccount(userid, ip);
        }

        [HttpGet]
        public LogonQuestionLog GetLogonQuestionById(int Id)
        {
            return _uService.GetLogonQuestionById(Id);
        }


        [HttpGet]
        public void ResetUnionpay(int userid, string ip)
        {
            _uService.ReSetUnionpayAccount(userid, ip);
        }

        [HttpGet]
        public void ResetPhoneNumber(int userid, string ip)
        {
            _uService.ResetPhoneNumber(userid, ip);
        }


        #region VIP用户

        [HttpPost]
        public APIResult AddFocusUser(FocusUser user)
        {
            var message = "";
            var result = _uService.AddFocusUser(user, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteFocusUser(int userID)
        {
            var message = "";
            var result = _uService.DeleteFocusUser(userID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<VIPUserInfo> GetFocusUser()
        {
            return _uService.GetFocusUser();
        }

        [HttpGet]
        public int GetOnlineVipUserCount(int adminId)
        {
            return _uService.GetOnlineVipUserCount(adminId);
        }

        [HttpGet]
        public int GetPlatformDownloadIncorrectCount(int adminId)
        {
            return _uService.GetPlatformDownloadIncorrectCount(adminId);
        }

        [HttpGet]
        public List<OnlineVIPUserInfo> GetOnlineVipUser(int? userID = null)
        {
            return _uService.GetOnlineVipUser(userID);
        }



        #endregion


        [HttpGet]
        public MoenyLogAddSumFlowAmountReport GetMoneyLog([FromUri]MoneyLogQueryFilterModel filter)
        {
            return _moneyService.GetMoneyLog(filter);
        }

        [HttpGet]
        public PagedList<BankLog> GetBankLog(int operatorID, int? userID = null, Int16? sourceType = null, string ipAddress = null, string memo = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _moneyService.GetBankLog(operatorID, userID, sourceType, ipAddress, memo, beginTime, endTime, pageIndex, pageSize);
        }


        [HttpGet]
        public PagedList<GameLog> GetGameLog(int? serverID = null, int? gameID = null, int? userID = null, string ipAddress = null, HDType? hDtype = null, string hDcode = null,
           DateTime? loginBeginTime = null, DateTime? loginEndTime = null, DateTime? logoutBeginTime = null, DateTime? logoutEndTime = null, int? areaID = null, int pageIndex = 1, int pageSize = 20, int? AdminID = null)
        {
            return _gameService.GetGameLog(serverID, gameID, userID, ipAddress, hDtype, hDcode, loginBeginTime, loginEndTime, logoutBeginTime, logoutEndTime, areaID, pageIndex, pageSize, AdminID);
        }


        [HttpGet]
        public PagedList<OrderListDTO> GetPayOrder(int? platformId = null, int? orderID = null, int? adminId = null, int? userID = null, string ipAddress = null, int? GWID = null, int? payTypeID = null, CDOrderStatus? status = null, DateTime? createBeginTime = null, DateTime? createEndTime = null, int? areaID = null, int pageIndex = 1, int pageSize = 20)
        {
            return _payService.CDGetPayOrder(platformId, orderID, adminId, userID, ipAddress, GWID, payTypeID, status, 0, createBeginTime, createEndTime, areaID, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<ConvertLog> GetConvertLog(int? platformId, int? adminId, int? userID = null, ConvertLogStatus? status = null, int? convertType = null, string realName = null, string ipAddress = null, DateTime? beginTime = null, DateTime? endTime = null, string operatorName = null, int? gwid = null, decimal? minAmount = null, decimal? maxAmount = null, int pageIndex = 1, int pageSize = 20)
        {
            return _moneyService.GetConvertLog(platformId, adminId, userID, status, convertType, realName, ipAddress, beginTime, endTime, operatorName, gwid, minAmount, maxAmount, pageIndex, pageSize);
        }

        [HttpGet]
        public RegCountLog GetUserRegChargeInfo(int? platformId, int? adminId, DateTime? regBeginTime, DateTime? regEndTime, string platformids, int? areaID = null, HDType? hdType = null)
        {
            return _uService.GetUserRegChargeInfo(platformId, adminId, regBeginTime, regEndTime, platformids, areaID, hdType);
        }

        [HttpGet]
        public PagedList<UserInfoForReport> GetUserInfo(int areaID, int platformID, string startTime, long? totalCharge, int? totalLogin, int pageIndex, int pageSize)
        {
            return _uService.GetUserInfo(areaID, platformID, Convert.ToDateTime(startTime), totalCharge, totalLogin, pageIndex, pageSize);
        }

        [HttpGet]
        public string AddPreRegUserIP(int areaid, string hdcode, string userip)
        {
            return _uService.AddPreRegUserIP(areaid, hdcode, userip);
        }

        [HttpGet]
        public string AddPreRegHD(string ip, string hdcode, string hdinfo)
        {
            return _uService.AddPreRegHD(ip, hdcode, hdinfo);
        }

        [HttpGet]
        public UserInfoToken GetUserInfoByCerCode(string cerCode)
        {
            var result = _uService.GetUserInfoByCerCode(cerCode);
            return result;
        }

        [HttpGet]
        public APIResult CreateQuestion(int? userid, int? platformId, int? areaID, string msg, string IPAddress = "", string imageUrl = "")
        {
            var message = "";
            var result = _uService.CreateQuestion(userid, platformId, areaID, msg, IPAddress, imageUrl, ref message);
            return new APIResult(result, message);
        }


        [HttpGet]
        public void ServiceTalkCleanNotifyCount(int userid)
        {
            _uService.ServiceTalkCleanNotifyCount(userid);
        }


        public List<QuestionLog> GetUserMessageById(int UserID)
        {
            return _uService.GetUserMessageById(UserID);
        }

        /// <summary>
        /// 取得輸贏排行榜資訊
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet]
        public List<WinLostLeaderboard> GetWinLostLeaderboard(int operatorID, int? userID, DateTime? startTime, DateTime? endTime, int sortName, int sortLifting)
        {
            return _gameService.GetWinLostLeaderboard(operatorID, userID, startTime, endTime, sortName, sortLifting);
        }

        [HttpGet]
        public APIResult CSOnBoard(int operatorID, int status)
        {
            var message = "";
            var result = _uService.CSOnBoard(operatorID, status, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public CSOnBoard GetCSOnBoard(int operatorID)
        {           
            return _uService.GetCSOnBoard(operatorID);
        }

        [HttpGet]
        public FlowAmountFromLastConvertToNow GetFlowAmountFromLastConvertToNow(int UserID)
        {
            return _uService.GetFlowAmountFromLastConvertToNow(UserID);
        }
    }
}