using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Users;

namespace game.Service
{
    /// <summary>
    /// 用户业务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 根据用户ID获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        UserInfo GetUserByID(int userID);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        UserInfo GetUserByLoginName(string loginName);
        /// <summary>
        /// 存储过程获取用户的详细信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserDetailInfoDTO GetUserDetailInfo(int userId, DateTime? createTimeBegin, DateTime? createTimeEnd);

        /// <summary>
        /// 根据昵称获取用户
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        UserInfo GetUserByNickName(string nickName);

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="regBeginTime"></param>
        /// <param name="regEndTime"></param>
        /// <param name="lastlogonBeginTime"></param>
        /// <param name="lastlogonEndTime"></param>
        /// <param name="regIpAddress"></param>
        /// <param name="userType"></param>
        /// <param name="hdType"></param>
        /// <param name="hdCode"></param>
        /// <param name="lastlogonIPAddress"></param>
        /// <param name="sex"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<UserInfo> GetUser(int? userID, string loginName, string nickName, DateTime? regBeginTime, DateTime? regEndTime, DateTime? lastlogonBeginTime, DateTime? lastlogonEndTime,
            string regIpAddress, UserType? userType, HDType? hdType, string hdCode, string lastlogonIPAddress, Sex? sex, decimal? convertChargeRateBegin, decimal? convertChargeRateEnd,
            long? chargeAmountBegin, long? chargeAmountEnd, long? convertAmountBegin, long? convertAmountEnd, int? gradeID, string unionpayAccount, string alipayAccount, string realName,
            int? areaID, int pageIndex, int pageSize);

        PagedList<UserInfo2> GetUser2(int? userID, string loginName, string nickName, DateTime? regBeginTime, DateTime? regEndTime, DateTime? lastlogonBeginTime, DateTime? lastlogonEndTime,
            string regIpAddress, UserType? userType, HDType? hdType, string hdCode, string lastlogonIPAddress, Sex? sex, decimal? convertChargeRateBegin, decimal? convertChargeRateEnd,
            long? chargeAmountBegin, long? chargeAmountEnd, long? convertAmountBegin, long? convertAmountEnd, int? gradeID, string unionpayAccount, string alipayAccount, string realName,
            int? areaID, int pageIndex, int pageSize);



        PagedList<RegUserInfo> GetRegUser(int? platformid, int? adminId, int? userID = null, int? areaID = null, DateTime? regBeginTime = null, DateTime? regEndTime = null,
          UserType? userType=null, string regIpAddress = null, HDType? hdType = null, int pageIndex = 1, int pageSize = 20, SortName? sortName = SortName.StatTime, SortLifting? sortLifting = SortLifting.DESC);



        RegCountLog GetUserRegChargeInfo(int? platformId, int? adminId, DateTime? regBeginTime, DateTime? regEndTime, string platformids, int? areaID = null, UserType? userType = null, HDType? hdType = null);
        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="newpwd"></param>
        /// <param name="ipAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserPassword(int userID, string newpwd, string ipAddress, ref string message);

        /// <summary>
        /// 修改用户保险箱密码
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserBankPassword(int userID, string password, string ipAddres, ref string message);

        /// <summary>
        /// 判断操作者权限
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        bool GetOperatorPermission(int operatorID, int userID, string loginName);

        /// <summary>
        /// 加密密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        string EncrypePwd(string pwd);

        /// <summary>
        /// 修改用户名
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserLoginName(int userID, string loginName, string ipAddress, ref string message);

        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="nickName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyNickName(int userID, string nickName, string ipAddress, ref string message);

        /// <summary>
        /// 取得所有免费券
        /// </summary>
        /// <param name="userID"></param>
        string GetContestUserTicketSummary(int userID);


        /// <summary>
        /// 获取用户信息变更记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="type"></param>
        /// <param name="ipAddress"></param>
        /// <param name="createTimeBegin"></param>
        /// <param name="createTimeEnd"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<UserInfoChangeLog> GetUserInfoChangeLog(int operatorID, int? userID, UserInfoChangeType? type, string ipAddress, DateTime? createTimeBegin, DateTime? createTimeEnd,
            int pageIndex, int pageSize);

        /// <summary>
        /// 获取用户免费券纪录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="userID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<UserTicket> GetUserTicket(int gameID, int userID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 获取比赛排名纪录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="userID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<UserRank> GetUserInfoRankLog(int? gameID, int userID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 添加用户修改信息记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="oldData">旧数据</param>
        /// <param name="newData">新数据</param>
        /// <param name="memo">描述</param>
        /// <param name="type">修改类型</param>
        /// <param name="ipAddress">IP地址</param>
        void AddUserInfoChangeLog(int userID, string oldData, string newData, string memo, UserInfoChangeType type, string ipAddress);

        /// <summary>
        /// 重置用户支付宝账号
        /// </summary>
        /// <param name="UserID">用户ID</param>
        /// <param name="ip">操作IP</param>
        void ReSetAlipayAccount(int UserID, string ip);

        /// <summary>
        /// 重置用户銀行账号 
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="ip"></param>
        void ReSetUnionpayAccount(int UserID, string ip);

        /// <summary>
        /// 重置手機註冊
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="nnumber"></param>
        void ResetPhoneNumber(int UserID, string ip);

        PagedList<UserInfoForReport> GetUserInfo(int areaID, int platformID, DateTime startTime, long? totalCharge, int? totalLogin, int pageIndex, int pageSize);

        #region VIP用户

        /// <summary>
        /// 添加VIP用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddFocusUser(FocusUser user, ref string message);

        /// <summary>
        /// 删除VIP用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteFocusUser(int userID, ref string message);

        /// <summary>
        /// 获取VIP用户
        /// </summary>
        /// <returns></returns>
        List<VIPUserInfo> GetFocusUser();

        /// <summary>
        /// 獲取線上且充值超過10000的使用者數量
        /// </summary>
        /// <returns></returns>
        int GetOnlineVipUserCount(int adminId);

        /// <summary>
        /// 取得渠道包是否有不正確資訊
        /// </summary>
        /// <returns></returns>
        int GetPlatformDownloadIncorrectCount(int adminId);

        /// <summary>
        /// 取得線上且超過10000的使用者
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        List<OnlineVIPUserInfo> GetOnlineVipUser(int? UserID);

        #endregion




        #region 用户问题反馈处理
        List<QuestionLogViewModel> GetQuestionUserName(int adminUserID, int? IncludeUser);

        /// <summary>
        /// 获取用户的问题列表
        /// </summary>
        /// <param name="UserID">用户id</param>
        /// <param name="Status">状态</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns></returns>
        PagedList<QuestionLog> GetUserQuestionLog(int adminUserID, string platformIDs, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize);

        /// <summary>
        /// 获取用户的最新问题列表
        /// </summary>
        /// <param name="UserID">用户id</param>
        /// <param name="Status">状态</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns></returns>
        PagedList<QuestionLog> GetUserQuestionLogAll(int adminUserID, string platformIDs, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize);
        
        /// <summary>
        /// 获取用户登入階段投诉问题
        /// </summary>
        /// <param name="platformIDs"></param>
        /// <param name="UserID"></param>
        /// <param name="Status"></param>
        /// <param name="handlePerson"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<QuestionLog> GetUserLogonQuestionLog(string platformIDs, string questionType, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int pageIndex, int pageSize);

        /// <summary>
        /// 获取未处理问题数(只取往后推24个小时未处理的问题数)
        /// </summary>
        /// <returns></returns>
        int GetUnReplyQuestionCount(string platformIDs, string handlePerson);

        /// <summary>
        /// 获取用户登入階段投诉问题尚未處理筆數(只取往后推24个小时未处理的问题数)
        /// </summary>
        /// <returns></returns>
        int GetLogonQuestionCount(string platformIDs, int status);

        /// <summary>
        /// 获取指定用户指定问题编号之前的n条问题
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="questionID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<QuestionLog> GetUserQuestionLogNearbyById(int userID, int questionID, int count);
        /// <summary>
        /// 添加用户问题
        /// </summary>
        /// <param name="userID">用户编号</param>
        /// <param name="question">问题内容</param>
        /// <param name="ipAddress">ip地址</param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool AddQuestionLog(int userID, string question, string ipAddress, ref string message);
        /// <summary>
        /// 问题记录修改
        /// </summary>
        /// <param name="log">问题实体</param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool ModifyQuestionLog(int Id, string handlePerson, ref string message);
        /// <summary>
        /// 根据id删除问题
        /// </summary>
        /// <param name="Id">问题id</param>
        /// <returns></returns>
        bool DeleteQuestionLogById(int Id, ref string message);
        /// <summary>
        /// 回复用户邮件
        /// </summary>
        /// <param name="UserId">用户Id</param>
        /// <param name="msg">回复消息</param>
        /// <param name="title">标题</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        bool ReplayMessage(int UserId, int qid, string title, string msg, string handelPerson, ref string message);


        /// <summary>
        /// 通过问题id获取用问题
        /// </summary>
        /// <param name="Id">问题Id</param>
        /// <returns></returns>
        QuestionLog GetQuestionById(int Id);

        LogonQuestionLog GetLogonQuestionById(int Id);

        UserInfoToken GetUserInfoByCerCode(string cerCode);

        List<QuestionLog> GetUserMessageById(int? UserID);

        bool CreateQuestion(int? userid, int? platformId, int? areaID, string msg, string IPAddress, string imageUrl, ref string message);

        void ServiceTalkCleanNotifyCount(int userid);

        void ReplyMessageCancel(int intQuestionID, int msgIndex);
        #endregion


        string AddPreRegUserIP(int areaid, string hdcode, string userip);
        string AddPreRegHD(string ip, string hdcode, string hdinfo);

        bool CSOnBoard(int operatorID, int status, ref string message);

        CSOnBoard GetCSOnBoard(int operatorID);
    }
}
