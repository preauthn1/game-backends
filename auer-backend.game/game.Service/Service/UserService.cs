using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.AdminSys;
using game.Entity.Users;
using game.Infrastructure;

namespace game.Service
{
    public class UserService : IUserService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFatctory;

        public UserService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFatctory = dbFactory;
        }

        public UserInfo GetUserByID(int userID)
        {
            return _repoFactory.CreateRepository<UserInfo>().GetById(userID);
        }
        /// <summary>
        /// 用户详细信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserDetailInfoDTO GetUserDetailInfo(int userId, DateTime? sTimeBegin, DateTime? sTimeEnd)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userId),
                                   new SqlParameter("@dtBeginTime",sTimeBegin),
                                   new SqlParameter("@dtEndTime",sTimeEnd)
                                   };
            var user =
                _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB)
                    .ExecuteStoreProcedureForDataList<UserDetailInfoDTO>("[dbo].[PrWeb_User_GetUserDetailInfo]", paras);
            return user != null ? user.FirstOrDefault() : null;
        }

        public UserInfo GetUserByLoginName(string loginName)
        {
            return _repoFactory.CreateRepository<UserInfo>().Entities.FirstOrDefault(m => m.LoginName == loginName);
        }

        public UserInfo GetUserByNickName(string nickName)
        {
            return _repoFactory.CreateRepository<UserInfo>().Entities.FirstOrDefault(m => m.NickName == nickName);
        }

        public FW.Core.Helper.PagedList<UserInfo> GetUser(int? userID, string loginName, string nickName, DateTime? regBeginTime, DateTime? regEndTime, DateTime? lastlogonBeginTime, DateTime? lastlogonEndTime,
            string regIpAddress, UserType? userType, HDType? hdType, string hdCode, string lastlogonIPAddress, Sex? sex, decimal? convertChargeRateBegin, decimal? convertChargeRateEnd,
            long? chargeAmountBegin, long? chargeAmountEnd, long? convertAmountBegin, long? convertAmountEnd, int? gradeID, string unionpayAccount, string alipayAccount, string realName, int? areaID, int pageIndex, int pageSize)
        {
            Int64 pageCount = 0;
            SqlParameter[] paras = {
                                       new SqlParameter("@intUserID",userID),
                                       new SqlParameter("@chvLoginName",loginName),
                                       new SqlParameter("@dtmRegBeginTime",regBeginTime),
                                       new SqlParameter("@dtmRegEndTime",regEndTime),
                                       new SqlParameter("@dtmLastlogonBeginTime",lastlogonBeginTime),
                                       new SqlParameter("@dtmLastlogonEndTime",lastlogonEndTime),
                                       new SqlParameter("@chvRegIpAddress",regIpAddress),
                                       new SqlParameter("@intUserType",userType==null?null:Convert.ToInt32(userType).ToString()),
                                       new SqlParameter("@intHdType",hdType==null?null:Convert.ToInt32(hdType).ToString()),
                                       new SqlParameter("@chvHdCode",hdCode),
                                       new SqlParameter("@chvLastlogonIPAddress",lastlogonIPAddress),
                                       new SqlParameter("@intSex",sex==null?null:Convert.ToInt32(sex).ToString()),
                                       new SqlParameter("@dmlConvertChargeRateBegin",convertChargeRateBegin),
                                       new SqlParameter("@dmlConvertChargeRateEnd",convertChargeRateEnd),
                                       new SqlParameter("@lngChargeAmountBegin",chargeAmountBegin),
                                       new SqlParameter("@lngChargeAmountEnd",chargeAmountEnd),
                                       new SqlParameter("@lngConvertAmountBegin",convertAmountBegin),
                                       new SqlParameter("@lngConvertAmountEnd",convertAmountEnd),
                                       new SqlParameter("@chvUnionpayAccount",unionpayAccount),
                                       new SqlParameter("@chvAlipayAccount",alipayAccount),
                                       new SqlParameter("@chvRealName",realName),
                                       new SqlParameter("@intAreaID",areaID),
                                       new SqlParameter("@intPageIndex",pageIndex),
                                       new SqlParameter("@intPageSize",pageSize),
                                       new SqlParameter("@intPageCount",pageCount)

                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserInfo>("[dbo].[PrWeb_User_GetUserInfo]", paras).ToList();

            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new FW.Core.Helper.PagedList<UserInfo>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;

        }





        public FW.Core.Helper.PagedList<UserInfo2> GetUser2(int? userID, string loginName, string nickName, DateTime? regBeginTime, DateTime? regEndTime, DateTime? lastlogonBeginTime, DateTime? lastlogonEndTime,
            string regIpAddress, UserType? userType, HDType? hdType, string hdCode, string lastlogonIPAddress, Sex? sex, decimal? convertChargeRateBegin, decimal? convertChargeRateEnd,
            long? chargeAmountBegin, long? chargeAmountEnd, long? convertAmountBegin, long? convertAmountEnd, int? gradeID, string unionpayAccount, string alipayAccount, string realName, int? areaID, int pageIndex, int pageSize)
        {
            Int64 pageCount = 0;
            SqlParameter[] paras = {
                                       new SqlParameter("@intUserID",userID),
                                       new SqlParameter("@chvLoginName",loginName),
                                       new SqlParameter("@dtmRegBeginTime",regBeginTime),
                                       new SqlParameter("@dtmRegEndTime",regEndTime),
                                       new SqlParameter("@dtmLastlogonBeginTime",lastlogonBeginTime),
                                       new SqlParameter("@dtmLastlogonEndTime",lastlogonEndTime),
                                       new SqlParameter("@chvRegIpAddress",regIpAddress),
                                       new SqlParameter("@intUserType",(object)userType??DBNull.Value),
                                       new SqlParameter("@intGrandID",(object)gradeID??DBNull.Value),
                                       new SqlParameter("@intHdType",(object)hdType??DBNull.Value),
                                       new SqlParameter("@chvHdCode",hdCode),
                                       new SqlParameter("@chvLastlogonIPAddress",lastlogonIPAddress),
                                       new SqlParameter("@intSex",(object)sex??DBNull.Value),
                                       new SqlParameter("@dmlConvertChargeRateBegin",convertChargeRateBegin),
                                       new SqlParameter("@dmlConvertChargeRateEnd",convertChargeRateEnd),
                                       new SqlParameter("@lngChargeAmountBegin",chargeAmountBegin),
                                       new SqlParameter("@lngChargeAmountEnd",chargeAmountEnd),
                                       new SqlParameter("@lngConvertAmountBegin",convertAmountBegin),
                                       new SqlParameter("@lngConvertAmountEnd",convertAmountEnd),
                                       new SqlParameter("@chvUnionpayAccount",unionpayAccount),
                                       new SqlParameter("@chvAlipayAccount",alipayAccount),
                                       new SqlParameter("@chvRealName",realName),
                                       new SqlParameter("@intAreaID",areaID),
                                       new SqlParameter("@intPageIndex",pageIndex),
                                       new SqlParameter("@intPageSize",pageSize),
                                       new SqlParameter("@intPageCount",pageCount)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserInfo2>("[dbo].[PrWeb_User_GetUserInfo]", paras).ToList();

            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new PagedList<UserInfo2>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));
            return pageList;
        }

        public PagedList<RegUserInfo> GetRegUser(int? platformid, int? adminId, int? userID = null, int? areaID = null, DateTime? regBeginTime = null, DateTime? regEndTime = null,
          UserType? userType=null, string regIpAddress = null, HDType? hdType = null, int pageIndex = 1, int pageSize = 20, SortName? sortName = SortName.StatTime, SortLifting? sortLifting = SortLifting.DESC)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                       new SqlParameter("@intAdminID",adminId),
                                       new SqlParameter("@intPlatformID",platformid),
                                       new SqlParameter("@intAreaID",areaID),
                                       new SqlParameter("@chvUserName",null),
                                       new SqlParameter("@intUserID",userID),
                                       new SqlParameter("@tnyUserType", userType),
                                       new SqlParameter("@chvIPAddress",regIpAddress),
                                       new SqlParameter("@tnyHDType",hdType),
                                       new SqlParameter("@dtStatTime",regBeginTime),
                                       new SqlParameter("@dtEndTime",regEndTime),
                                       new SqlParameter("@tnyColumns",(int)sortName),
                                       new SqlParameter("@tnySort",(int)sortLifting),
                                       new SqlParameter("@intPageIndex",pageIndex),
                                       new SqlParameter("@intPageSize",pageSize),
                                       new SqlParameter("@intPageCount",pageCount)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<RegUserInfo>("[dbo].[PrWeb_Stat_GetRegUserInfo]", paras).ToList();

            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new PagedList<RegUserInfo>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
        }

        public RegCountLog GetUserRegChargeInfo(int? platformId, int? adminId, DateTime? regBeginTime, DateTime? regEndTime, string platformids, int? areaID = null, UserType? userType=null, HDType? hdType = null)
        {
            var regCount = 0;
            var chargeCount = 0;
            var intRegCount = new SqlParameter
            {
                ParameterName = "@intRegCount",
                Value = regCount,
                Direction = ParameterDirection.Output
            };

            var intChargeCount = new SqlParameter
            {
                ParameterName = "@intChargeCount",
                Value = chargeCount,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                  new SqlParameter("@intAdminID",adminId),
                  new SqlParameter("@intPlatformID",platformId),
                  new SqlParameter("@dtmRegBeginTime",regBeginTime),
                  new SqlParameter("@dtmRegEndTime",regEndTime),
                  new SqlParameter("@intAreaID", areaID),
                  new SqlParameter("@tnyUserType", userType),
                  new SqlParameter("@inthdType",hdType),
                  intRegCount, intChargeCount
            };

            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_User_GetUserRegChargeInfo]", paras);


            var data = new RegCountLog
            {
                ChargeCount = Convert.ToInt32(intChargeCount.Value),
                RegCount = Convert.ToInt32(intRegCount.Value)
            };

            return data;
        }



        public bool ModifyUserPassword(int userID, string newpwd, string ipAddress, ref string message)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@chvNewPwd",newpwd),
                                   new SqlParameter("@chvIPAddress",ipAddress),
                                   DBHelper.GetProcedureErrorMsg()
                                   };
            int returnVal = _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_ModifyPassword", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnVal > 0;
        }

        public bool ModifyUserBankPassword(int userID, string newpwd, string ipAddress, ref string message)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@chvNewPwd",newpwd),
                                   new SqlParameter("@chvIPAddress",ipAddress),
                                   DBHelper.GetProcedureErrorMsg()
                                   };
            int returnVal = _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_ModifyBankPassword", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnVal > 0;
        }


        public string EncrypePwd(string pwd)
        {
            string encryPwd = "";
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@chvPwd",pwd),
                                   new SqlParameter("@chvEncryptPwd",encryPwd)
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            paras.LastOrDefault().SqlDbType = System.Data.SqlDbType.VarChar;
            paras.LastOrDefault().Size = 32;

            int returnVal = _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrPsWeb_User_EncryptPwd", paras);
            encryPwd = [REDACTED];
            return encryPwd;
        }

        public bool ModifyUserLoginName(int userID, string loginName, string ipAddress, ref string message)
        {
            var u = GetUserByID(userID);
            if (u == null)
            {
                message = "操作失败:用户不存在";
                return false;
            }
            if (_repoFactory.CreateRepository<UserInfo>().Entities.FirstOrDefault(p => p.LoginName == loginName) != null)
            {
                message = "操作失败:用户名已被占用";
                return false;
            }


            string oldData = "原用户名:" + u.LoginName;
            string newData = "修改后的用户名:" + loginName;

            u.LoginName = loginName;
            _repoFactory.CreateRepository<UserInfo>().Update(u);

            //写修改用户信息记录
            AddUserInfoChangeLog(u.Id, oldData, newData, "修改用户名", UserInfoChangeType.LoginName, ipAddress);

            message = "操作成功";
            return true;
        }

        public bool ModifyNickName(int userID, string nickName, string ipAddress, ref string message)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@chvNickName",nickName),
                                   new SqlParameter("@chvIPAddress",ipAddress),
                                   DBHelper.GetProcedureErrorMsg()
                                   };
            int returnVal = _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_ModifyNickName", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnVal > 0;

        }

        public string GetContestUserTicketSummary(int userID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("intUserID",userID)
                                   };
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<string>("[dbo].[PrWeb_Event_GetContestUserTicketSummary]", paras).ToList();
            if (list.Count > 0) return list[0];
            return "{}";
        }

        public PagedList<UserInfoChangeLog> GetUserInfoChangeLog(int operatorID, int? userID, UserInfoChangeType? type, string ipAddress, DateTime? createTimeBegin, DateTime? createTimeEnd, int pageIndex, int pageSize)
        {
            var _pageCount = 0;
            SqlParameter[] _paras = {
                                    new SqlParameter("@intAdminID",operatorID),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@intChangeType",type),
                                    new SqlParameter("@dtStartTime",createTimeBegin),
                                    new SqlParameter("@dtEndTime",createTimeEnd),
                                    new SqlParameter("@chvIPAddress",string.IsNullOrEmpty(ipAddress) ? DBNull.Value :(object) ipAddress),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",_pageCount)
                                   };
            _paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFatctory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForDataList<UserInfoChangeLog>("[dbo].[PrWeb_User_GetUserInfoChangeLog]", _paras).ToList();

            _pageCount = Convert.ToInt32(_paras.LastOrDefault().Value);
            if (_pageCount > int.MaxValue) _pageCount = int.MaxValue;

            return new PagedList<UserInfoChangeLog>(_list, pageIndex, pageSize, _pageCount);
        }

        public PagedList<UserTicket> GetUserTicket(int gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                       new SqlParameter("@intGameID",gameID),
                                       new SqlParameter("@intUserID",userID),
                                       new SqlParameter("@dtStartTime",beginTime),
                                       new SqlParameter("@dtEndTime",endTime),
                                       new SqlParameter("@intPageIndex",pageIndex),
                                       new SqlParameter("@intPageSize",pageSize),
                                       new SqlParameter("@intPageCount",pageCount)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserTicket>("dbo.PrWeb_Event_GetContestUserTicket", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<UserTicket>(list, pageIndex, pageSize, pageCount);
        }

        public PagedList<UserRank> GetUserInfoRankLog(int? gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                       new SqlParameter("@intGameID",gameID),
                                       new SqlParameter("@intUserID",userID),
                                       new SqlParameter("@dtStartTime",beginTime),
                                       new SqlParameter("@dtEndTime",endTime),
                                       new SqlParameter("@intPageIndex",pageIndex),
                                       new SqlParameter("@intPageSize",pageSize),
                                       new SqlParameter("@intPageCount",pageCount)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserRank>("dbo.PrWeb_Event_GetContestUserRank", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<UserRank>(list, pageIndex, pageSize, pageCount);
        }


        public bool GetOperatorPermission(int operatorID, int userID, string loginName)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@chvLoginName",string.IsNullOrEmpty(loginName) ? DBNull.Value :(object) loginName),
                                   };
            int returnVal = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_AdminCheck", paras);
            return returnVal > 0;
        }

        /// <summary>
        /// 写用户信息修改日志
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="oldData"></param>
        /// <param name="newData"></param>
        /// <param name="memo"></param>
        /// <param name="type"></param>
        /// <param name="ipAddress"></param>
        public void AddUserInfoChangeLog(int userID, string oldData, string newData, string memo, UserInfoChangeType type, string ipAddress)
        {
            UserInfoChangeLog log = new UserInfoChangeLog();
            log.UserID = userID;
            log.OldData = oldData;
            log.NewData = newData;
            log.Memo = memo;
            log.ChangeType = type;
            log.IPAddress = ipAddress;
            log.WriteTime = DateTime.Now;
            _repoFactory.CreateRepository<UserInfoChangeLog>().Insert(log);
        }

        public List<QuestionLogViewModel> GetQuestionUserName(int adminUserID, int? IncludeUser)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminUserID),
                                    new SqlParameter("@intIncludeUser",IncludeUser),
                                   };
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<QuestionLogViewModel>("dbo.PrWeb_QuestionUserNameList_Get", paras).ToList();
            return list;
        }

        public PagedList<QuestionLog> GetUserQuestionLog(int adminUserID, string platformIDs, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminUserID),
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@intUserID",UserID),
                                    new SqlParameter("@tnyStatus",Status),
                                    new SqlParameter("@chvHandlePerson",handlePerson),
                                    new SqlParameter("@dtBeginTime",StartTime),
                                    new SqlParameter("@dtEndTime",EndTime),
                                    new SqlParameter("@intIncludeUser",IncludeUser),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<QuestionLog>("dbo.PrWeb_UserQuestion_Get", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<QuestionLog>(list, pageIndex, pageSize, pageCount);
        }

        public PagedList<QuestionLog> GetUserQuestionLogAll(int adminUserID, string platformIDs, int? UserID, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int? IncludeUser, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminUserID),
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@intUserID",UserID),
                                    new SqlParameter("@tnyStatus",Status),
                                    new SqlParameter("@chvHandlePerson",handlePerson),
                                    new SqlParameter("@dtBeginTime",StartTime),
                                    new SqlParameter("@dtEndTime",EndTime),
                                    new SqlParameter("@intIncludeUser",IncludeUser),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<QuestionLog>("dbo.PrWeb_UserQuestionAll_Get", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<QuestionLog>(list, pageIndex, pageSize, pageCount);
        }

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
        public FW.Core.Helper.PagedList<QuestionLog> GetUserLogonQuestionLog(string platformIDs, string questionType, QuestionStatus? Status, string handlePerson, DateTime? StartTime, DateTime? EndTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@chvQuestionType",questionType),
                                    new SqlParameter("@tnyStatus",Status),
                                    new SqlParameter("@chvHandlePerson",handlePerson),
                                    new SqlParameter("@dtBeginTime",StartTime),
                                    new SqlParameter("@dtEndTime",EndTime),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<QuestionLog>("dbo.PrWeb_UserLogonQuestion_Get", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new FW.Core.Helper.PagedList<QuestionLog>(list, pageIndex, pageSize, pageCount);

        }
        public bool AddQuestionLog(int userID, string question, string ipAddress, ref string message)
        {
            var user = _repoFactory.CreateRepository<UserInfo>().GetById(userID);
            if (user == null)
            {
                message = "操作失败:游戏账号不存在";
                return false;
            }
            var n = DateTime.Now;
            var d = new DateTime(n.Year, n.Month, n.Day);
            var context = _repoFactory.CreateRepository<QuestionLog>();
            if (context.Entities.Count(m => m.CreateTime >= d && m.UserID == userID) >= 10)
            {
                message = "操作失败:提交问题太频繁,请稍后再试";
                return false;
            }
            var model = new QuestionLog();
            model.UserID = userID;
            model.Question = question;
            model.IPAddress = ipAddress;
            model.Status = QuestionStatus.UnHandled;
            model.CreateTime = n;
            model.HandleTime = n;
            context.Insert(model);
            message = "操作成功";
            return true;
        }
        public bool ModifyQuestionLog(int Id, string handlePerson, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intQuestionID",Id),
                                    new SqlParameter("@tintStatus",QuestionStatus.Handled),
                                    new SqlParameter("@chvHandlePerson",handlePerson),
                                   };
            _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserQuestion_ModifyStatus", paras);
            message = "修改成功";
            return true;
        }


        public bool DeleteQuestionLogById(int Id, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intQuestionID",Id)
                                   };
            _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserQuestion_Delete", paras);

            message = "删除成功";
            return true;
        }

        public bool ReplayMessage(int UserId, int qid, string title, string msg, string handelPerson, ref string message)
        {

            //如果回覆內容是空白,不做任何事
            if (msg.Trim().Length == 0)
            {
                message = "发送成功";
                return true;
            }

            SqlParameter[] paras = {
                                    new SqlParameter("@intQuestionID",qid),
                                    new SqlParameter("@chvHandlePerson",handelPerson),
                                    new SqlParameter("@intUserID",UserId),
                                    new SqlParameter("@chvTitle",title),
                                    new SqlParameter("@chvContent",msg)
                                   };

            int returnVal = _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserQuestion_Reply", paras);
            if (returnVal > 0)
            {
                message = "发送成功";
                return true;
            }
            message = "发送失败";
            return false;
        }

        public void ReplyMessageCancel(int questionID, int msgIndex)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intQuestionID",questionID),
                                    new SqlParameter("@intRow",msgIndex)
                                   };
            _dbFatctory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserQuestion_ReplyCancel", paras);
        }


        public List<QuestionLog> GetUserQuestionLogNearbyById(int userID, int questionID, int count)
        {
            var t = DateTime.Now.AddDays(-30);
            return _repoFactory.CreateRepository<QuestionLog>().Entities.Where(x => x.UserID == userID && x.Id <= questionID && x.CreateTime > t).OrderByDescending(x => x.Id).Take(count).ToList();
        }
        public QuestionLog GetQuestionById(int Id)
        {
            DateTime dt = DateTime.Now.AddMonths(-1);
            return _repoFactory.CreateRepository<QuestionLog>().Entities.FirstOrDefault(m => m.Id == Id && m.CreateTime >= dt);
        }

        public LogonQuestionLog GetLogonQuestionById(int Id)

        {
            DateTime dt = DateTime.Now.AddMonths(-1);
            SqlParameter[] paras = {
                                    new SqlParameter("@Id",Id)
            };

            string sql = @"Select q.*, u.LoginName from Users.QuestionLog q 
                        left join Users.UserInfo u 
                        on q.UserID = u.UserID
                        where q.id = @Id";
            LogonQuestionLog list = _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<LogonQuestionLog>(sql, paras).FirstOrDefault();

            return list;
        }


        public UserInfoToken GetUserInfoByCerCode(string cerCode)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@cerCode",cerCode)
            };

            string sql = @"SELECT U.UserID, U.AreaID, A.PlatformID
                        FROM [CenterDB].[Game].[Certification] AS C
                        INNER JOIN [CenterDB].[Users].[UserInfo] U ON C.UserID = U.UserID
                        INNER JOIN [CenterDB].Area.Area AS A ON A.AreaID = U.AreaID	
                        WHERE [CerCode] = @cerCode";
            UserInfoToken list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<UserInfoToken>(sql, paras).FirstOrDefault();
            return list;
        }


        public List<QuestionLog> GetUserMessageById(int? UserID)
        {
            var list = _repoFactory.CreateRepository<QuestionLog>().Entities.Where(x => x.UserID == UserID).ToList();
            return list;
        }


        public bool CreateQuestion(int? userid, int? platformId, int? areaID, string msg, string IPAddress, string imageUrl, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID",platformId),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@intUserID",userid),
                                    new SqlParameter("@chvMsg",msg),
                                    new SqlParameter("@intQuestionType", "0"),
                                    new SqlParameter("@chvIPAddress",IPAddress),
                                    new SqlParameter("@chvImageUrl",imageUrl),
                                    new SqlParameter("@chvErrMsg",message),
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            int returnVal = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_ContactService", paras);

            message = paras.LastOrDefault().Value.ToString();

            if (returnVal > 0)
            {
                message = "发送成功";
                return true;
            }
            message = "发送失败";
            return false;
        }

        public void ServiceTalkCleanNotifyCount(int userid)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userid),
                                    new SqlParameter("@intCount", (object)0)
                                   };
            _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrNs_ServiceTalkNotifyCount", paras);
        }


        public void ReSetAlipayAccount(int UserID, string ip)
        {
            var user = GetUserByID(UserID);

            string oldData = string.Format("原支付宝姓名:{0},支付宝账号:{1}", user.RealName, user.AlipayAccount);

            user.RealName = "";
            user.AlipayAccount = "";
            _repoFactory.CreateRepository<UserInfo>().Update(user);

            string newData = "重置为空";
            //写修改用户信息记录
            AddUserInfoChangeLog(user.Id, oldData, newData, "重置用户支付宝账号", UserInfoChangeType.Payment, ip);
        }


        public void ReSetUnionpayAccount(int UserID, string ip)
        {
            var user = GetUserByID(UserID);

            string oldData = string.Format("原銀行卡账号:{0}", user.UnionpayAccount);

            user.UnionpayAccount = "";
            user.UnionpayRealName = "";
            _repoFactory.CreateRepository<UserInfo>().Update(user);
            string newData = "重置为空";

            //写修改用户信息记录
            AddUserInfoChangeLog(user.Id, oldData, newData, "重置用户銀行卡账号", UserInfoChangeType.Payment, ip);
        }

        public void ResetPhoneNumber(int UserID, string ip)
        {
            var user = GetUserByID(UserID);

            string oldData = string.Format("原注册手机号:{0}", user.LoginName);

            user.LoginName = "Fast_" + user.LoginName + user.Id;
            user.UserType = UserType.Fast;
            _repoFactory.CreateRepository<UserInfo>().Update(user);
            string newData = "重置为" + user.LoginName;

            //写修改用户信息记录
            AddUserInfoChangeLog(user.Id, oldData, newData, "重置用户注册手机号", UserInfoChangeType.LoginName, ip);
        }

        public int GetUnReplyQuestionCount(string platformIDs, string handlePerson)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@chvHandlePerson",handlePerson),
                                    new SqlParameter("@intReplyCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserQuestion_UnReplyCount", paras);

            return Convert.ToInt32(paras.LastOrDefault().Value);
        }

        public int GetLogonQuestionCount(string platformIDs, int status)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@tintStatus",status),
                                    new SqlParameter("@intCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UserLogonQuestion_Count", paras);

            return Convert.ToInt32(paras.LastOrDefault().Value);
        }

        public PagedList<UserInfoForReport> GetUserInfo(int areaID, int platformID, DateTime startTime, long? totalCharge, int? totalLogin, int pageIndex, int pageSize)
        {
            var _pageCount = 0;
            SqlParameter[] _paras = {
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@dtStatTime",startTime),
                                    new SqlParameter("@StatChargeAmount",totalCharge == null ? (object)DBNull.Value : totalCharge.Value),
                                    new SqlParameter("@StatLogonCount",totalLogin == null ? (object)DBNull.Value : totalLogin.Value),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",_pageCount)
                                   };
            _paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserInfoForReport>("[dbo].[PrWeb_Stat_GetStatReportDetail]", _paras).ToList();

            _pageCount = Convert.ToInt32(_paras.LastOrDefault().Value);
            if (_pageCount > int.MaxValue) _pageCount = int.MaxValue;

            return new PagedList<UserInfoForReport>(_list, pageIndex, pageSize, _pageCount);
        }

        #region VIP用户

        public bool AddFocusUser(FocusUser user, ref string message)
        {
            var u = _repoFactory.CreateRepository<FocusUser>().GetById(user.Id);
            if (u == null)
            {
                _repoFactory.CreateRepository<FocusUser>().Insert(user);
            }

            message = "操作成功";
            return true;
        }

        public bool DeleteFocusUser(int userID, ref string message)
        {
            var u = _repoFactory.CreateRepository<FocusUser>().GetById(userID);
            if (u != null)
            {
                _repoFactory.CreateRepository<FocusUser>().Delete(u);
            }

            message = "操作成功";
            return true;
        }

        public List<VIPUserInfo> GetFocusUser()
        {
            return _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<VIPUserInfo>("dbo.PrWeb_GetFocusUser", new SqlParameter[] { }).ToList();
        }


        public int GetOnlineVipUserCount(int adminId)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
                                    new SqlParameter("@intCount",(object)0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_GetOnlineVIPUser_Count", paras);

            return Convert.ToInt32(paras.LastOrDefault().Value);
        }

        public int GetPlatformDownloadIncorrectCount(int adminId)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
                                    new SqlParameter("@intCount",(object)0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_GetPlatformDownloadIncorrectCount", paras);

            return Convert.ToInt32(paras.LastOrDefault().Value);
        }


        public List<OnlineVIPUserInfo> GetOnlineVipUser(int? UserID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",UserID)
                                   };
            var list = _dbFatctory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<OnlineVIPUserInfo>("dbo.PrWeb_Game_GetOnlineVIPUser", paras).ToList();

            return list;

        }
        #endregion


        //記錄欲註冊的使用者IP
        public string AddPreRegUserIP(int areaid, string hdcode, string userip)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@intAreaID",areaid),
                                    new SqlParameter("@chvHdCode",hdcode),
                                    new SqlParameter("@chvUserIP",userip)
                                   };

            int returnVal = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_AddPreRegInfo", _paras);
            return returnVal.ToString();

        }

        //記錄欲註冊的使用者手機設備
        public string AddPreRegHD(string ip, string hdcode, string hdinfo)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@chvIPAddress",ip),
                                    new SqlParameter("@chvHdCode",hdcode),
                                    new SqlParameter("@chvHDInfo",hdinfo)
            };

            int returnVal = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_AddPreRegHD", _paras);
            return returnVal.ToString();

        }

        public bool CSOnBoard(int operatorID, int status, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOperatorID",operatorID),
                                    new SqlParameter("@intStatus",status),
                                    new SqlParameter("@chvErrMsg",message),
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            int returnVal = _dbFatctory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_User_CSOnBoard", paras);

            message = paras.LastOrDefault().Value.ToString();

            if (returnVal > 0)
            {
                message = "打卡成功";
                return true;
            }
            message = "操作失败";
            return false;
        }

        public CSOnBoard GetCSOnBoard(int operatorID)
        {
            return _repoFactory.CreateRepository<CSOnBoard>().GetById(operatorID);
        }
    }
}
