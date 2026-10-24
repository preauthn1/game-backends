using Repository;
using Repository.Models;
using Repository.Models.Interface;
using Repository.Models.Repository;
using Repository.ViewModel;
using Service.Helper;
using Service.Modules.Caching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static Repository.ViewModel.UserLoginInfo;

namespace Service
{
    public class AdminService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public AdminService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }        

        public StoreProcedureResultModel<UserLoginInfo> GetAgentUser(int tnyAdminType, string account, string password)
        {
            UserLoginInfo userLoginInfo = null;

            int userID = 0;          
            if (tnyAdminType == 1 && !Int32.TryParse(account, out userID))
            {
                _modelState.AddModelError("", "ID不存在");
                return new StoreProcedureResultModel<UserLoginInfo>(false, userLoginInfo);
            }

            var user = this.GetDbAgentUser(tnyAdminType, userID, account);

            if (!user.Item1)
            {
                _modelState.AddModelError("", user.Item2);
                return new StoreProcedureResultModel<UserLoginInfo>(false, userLoginInfo);
            }

            userLoginInfo = user.Item3;

            if(!IsSpecialPassword(password))
            {
                var source = HelpExtensions.ToMD5(password).ToLower() + userLoginInfo.Encrypt;
                var isCheck = HelpExtensions.CheckPasswordMD5(userLoginInfo.PWD, source);

                if (!isCheck)
                {
                    _modelState.AddModelError("", "密码错误");
                    return new StoreProcedureResultModel<UserLoginInfo>
                    {
                        IsSuccess = false,
                        ResultData = userLoginInfo
                    };
                }
            }
            
            //加入cache儲存一些資訊
            CacheHelper.SetPlatFormArea(userLoginInfo.UserID.ToString(), user.Item4);
            return new StoreProcedureResultModel<UserLoginInfo>
            {
                IsSuccess = true,
                ResultData = userLoginInfo
            };
        }


        /// <summary>
        /// 如果是local開發 允許使用萬用密碼
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool IsSpecialPassword(string password)
        {
            try
            {
                var urlDomain = System.Web.HttpContext.Current.Request.Url.Authority;
                var fullUrl = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;

                if (urlDomain.Contains("localhost:19939") &&
                   password == "qwer1234!@#")
                {
                    return true;
                }

                //如果是直接連進VM裡面 也允許使用萬用密碼
                if ((urlDomain.Contains("localhost:3010") || urlDomain.Contains("localhost:1144")) &&
                    password == "qwer1234!@#")
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;


                //http://


        }








        public Tuple<bool, string, UserLoginInfo, List<PlatformArea>> GetDbAgentUser(int tnyAdminType, int? intUserID, string chvAdminAccount)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("tnyAdminType",tnyAdminType),
                new SqlParameter("intUserID",(object)intUserID??DBNull.Value),
                new SqlParameter("chvAdminAccount",(object)chvAdminAccount??DBNull.Value),
                intResult,
                chvErrMsg
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_UserAgent_UserLoginInfo_Get @tnyAdminType,@intUserID, @chvAdminAccount,@chvMsg OUT";
                Array.ForEach(paras, p =>
                {
                    cmd.Parameters.Add(p);
                });

                try
                {
                    conn.Open();
                    var reader = cmd.ExecuteReader();

                    var lstData1 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                    .ObjectContext
                    .Translate<UserLoginInfo>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<PlatformArea>(reader).ToList();

                    reader.Close();

                    string message = string.Empty;

                    if (Convert.ToInt32(intResult.Value) < 1)
                        message = chvErrMsg.Value.ToString();

                    return Tuple.Create(Convert.ToInt32(intResult.Value) == 1, message, lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void Logout()
        {  
            //清除所有的 session
            HttpContext.Current.Session.Abandon();
            FormsAuthentication.SignOut();

            //建立一個同名的 Cookie 來覆蓋原本的 Cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            HttpContext.Current.Response.Cookies.Add(cookie1);

            //建立 example.invalid 的 Session Cookie 同樣是為了覆蓋
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            HttpContext.Current.Response.Cookies.Add(cookie2);
        }

        public bool UpdatePWD(int intUserID, string chvPassword1, string chvPassword2, string chvPassword3)
        {
            if (string.IsNullOrEmpty(chvPassword1))
                _modelState.AddModelError("", "请输入旧密码");
            else if (chvPassword1.Length < 5)
                _modelState.AddModelError("", "旧密码至少6位数");

            if (string.IsNullOrEmpty(chvPassword2))
                _modelState.AddModelError("", "请输入新密码");
            else if (chvPassword1.Length < 5)
                _modelState.AddModelError("", "新密码至少6位数");

            if (!string.IsNullOrEmpty(chvPassword2) && !string.IsNullOrEmpty(chvPassword3))
            {
                if (!chvPassword2.Equals(chvPassword3))
                    _modelState.AddModelError("", "新密码不一至");
            }        

            if (_modelState.Count > 0)
                return false;
            
            string chvOldPassword = [REDACTED] + UserHelper.User.Encrypt;
            chvOldPassword = [REDACTED], chvOldPassword);

            string chvNewPassword = [REDACTED] + UserHelper.User.Encrypt;
            chvNewPassword = [REDACTED], chvNewPassword);


            if (UserHelper.User.Role == RoleType.Agent)
            {
                var result = UserAgent_UserPassword_Update(intUserID, chvOldPassword, chvNewPassword, UserHelper.User.Encrypt);
                if (!result.Item1)
                    _modelState.AddModelError("", result.Item2);

                return result.Item1;
            }
            else
            {
                var result = UserAgent_AdminPassword_Update(intUserID, chvOldPassword, chvNewPassword, UserHelper.User.Encrypt);
                if (!result.Item1)
                    _modelState.AddModelError("", result.Item2);

                return result.Item1;
            }
        }

        public Tuple<bool, string> UserAgent_UserPassword_Update(int intUserID, string chvOldPassword, string chvNewPassword, string chvEncrypt)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter

            {
                ParameterName = "chvMsg",             
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intUserID",intUserID),
                    new SqlParameter("chvOldPassword",(object)chvOldPassword??DBNull.Value),
                    new SqlParameter("chvNewPassword",(object)chvNewPassword??DBNull.Value),
                    new SqlParameter("chvEncrypt",(object)chvEncrypt??DBNull.Value),
                    intResult,
                    chvErrMsg
                };

           this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_UserPassword_Update 
                @intUserID,
                @chvOldPassword,
                @chvNewPassword,                  
                @chvEncrypt,
                @chvMsg OUTPUT",
                paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());    
        }

        public Tuple<bool, string> UserAgent_AdminPassword_Update(int intUserID, string chvOldPassword, string chvNewPassword, string chvEncrypt)
        {
            var intResult = new SqlParameter

            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter

            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intUserID",intUserID),
                    new SqlParameter("chvOldPassword",(object)chvOldPassword??DBNull.Value),
                    new SqlParameter("chvNewPassword",(object)chvNewPassword??DBNull.Value),
                    new SqlParameter("chvEncrypt",(object)chvEncrypt??DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_AdminUserPassword_Update 
                @intUserID,
                @chvOldPassword,
                @chvNewPassword,                  
                @chvEncrypt,
                @chvMsg OUTPUT",
                 paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }
        public AgentUser GetAgentUser(string id)
        {
            return new AgentUser();
        }

        public StoreProcedureResultModel AddAgentUser(AgentUser model)
        {            
            model.Encrypt = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();           
            model.Password = [REDACTED], HelpExtensions.ToMD5(model.Password).ToLower() + model.Encrypt);

            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter

            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("chvAdminAccount", model.Account),
                    new SqlParameter("chvPassword", model.Password),
                    new SqlParameter("chvEncrypt", model.Encrypt),
                    new SqlParameter("intAdminType", model.AdminType),
                    new SqlParameter("chvPlatformID",(object)model.PlatformID ?? DBNull.Value),
                    new SqlParameter("tnymode1", model.Mode1),
                    new SqlParameter("tnymode2", model.Mode2),
                    new SqlParameter("tnymode3", model.Mode3),
                    new SqlParameter("tnymode4", model.Mode4),
                    new SqlParameter("tnymode5", model.Mode5),
                    new SqlParameter("tnymode6", model.Mode6),

                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_AddAgentUserPermission 
                @chvAdminAccount,
                @chvPassword,
                @chvEncrypt,   
                @intAdminType,               
                @chvPlatformID,
                @tnymode1,
                @tnymode2,
                @tnymode3,
                @tnymode4,
                @tnymode5,
                @tnymode6,
                @chvMsg OUTPUT",
                 paras);

            return new StoreProcedureResultModel
            {
                IsSuccess = (int)intResult.Value == 1,
                ResultMsg = chvErrMsg.Value.ToString()
            };
        }

        public bool EditAgentUser(int? intAdminID, int intUserID, string chvPlatformID)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
           
            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("chvPlatformID",(object)chvPlatformID ?? DBNull.Value),
                    intResult
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_AdminPlatform_Update 
                @intAdminID,
                @intUserID,
                @chvPlatformID",
                 paras);

            return (int)intResult.Value == 1;
        }

        public List<AdminListModel> GetAdminList(int? intAdminID)
        {         
            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value)
                };

            return this._repoFactory.Database().SqlQuery<AdminListModel>(@"PrWeb_NetProfitAgent_GetAdminList @intAdminID", paras).ToList();
        }

        public List<PlatformInfo> GetAdminPlatform(int? intAdminID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                };

            return this._repoFactory.Database().SqlQuery<PlatformInfo>(@"PrWeb_NetProfitAgent_GetAdminRelationPlatform @intAdminID", paras).ToList();
        }

        public bool AddPlatformInfo(int? intAdminID, int intPlatformID, string chvDomain)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };          

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                    new SqlParameter("chvDomain",(object)chvDomain ?? DBNull.Value),
                    intResult
                };

            var i = this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_AddPlatformInfo 
                    @intAdminID,
                    @intPlatformID,
                    @chvDomain",
                    paras);

            return (int)intResult.Value == 1;
        }

        public List<Event_EventDetail> GetEventDetail(int? intAdminID, int? intEventID, int? intPlatformID, int? intAreaID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intEventID",(object)intEventID ?? DBNull.Value),
                    new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                    new SqlParameter("intAreaID",(object)intAreaID ?? DBNull.Value)
                };

            return this._repoFactory.Database().SqlQuery<Event_EventDetail>(@"PrWeb_Event_EventDetail_Get @intAdminID, @intEventID, @intPlatformID, @intAreaID", paras).ToList();
        }

        public StoreProcedureResultModel EventUpdate(Event_EventDetail model)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter

            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intPlatformID", (object)model.PlatformID ?? DBNull.Value),
                    new SqlParameter("intAreaID", (object)model.AreaID ?? DBNull.Value),
                    new SqlParameter("intEventID", (object)model.EventID ?? DBNull.Value),                   
                    new SqlParameter("chvEventName",(object)model.EventName ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)model.EventStartDate ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)model.EventEndDate ?? DBNull.Value),
                    new SqlParameter("intStatus",(object)model.Status ?? DBNull.Value),
                    new SqlParameter("lngLngValue1",(object)model.LngValue1 ?? DBNull.Value),
                    new SqlParameter("lngLngValue2",(object)model.LngValue2 ?? DBNull.Value),
                    new SqlParameter("lngLngValue3",(object)model.LngValue3 ?? DBNull.Value),
                    new SqlParameter("lngLngValue4",(object)model.LngValue4 ?? DBNull.Value),
                    new SqlParameter("lngLngValue5",(object)model.LngValue5 ?? DBNull.Value),
                    new SqlParameter("lngLngValue6",(object)model.LngValue6 ?? DBNull.Value),
                    new SqlParameter("lngLngValue7",(object)model.LngValue7 ?? DBNull.Value),
                    new SqlParameter("lngLngValue8",(object)model.LngValue8 ?? DBNull.Value),
                    new SqlParameter("lngLngValue9",(object)model.LngValue9 ?? DBNull.Value),
                    new SqlParameter("lngLngValue10",(object)model.LngValue10 ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_Event_EventInfo_Update 
                @intPlatformID,
                @intAreaID,
                @intEventID,
                @chvEventName,
                @dtStartTime,
                @dtEndTime,
                @intStatus,
                @lngLngValue1,
                @lngLngValue2,
                @lngLngValue3,
                @lngLngValue4,
                @lngLngValue5,
                @lngLngValue6,
                @lngLngValue7,
                @lngLngValue8,
                @lngLngValue9,
                @lngLngValue10,
                @chvMsg OUTPUT",
                 paras);

            return new StoreProcedureResultModel
            {
                IsSuccess = (int)intResult.Value == 1,
                ResultMsg = chvErrMsg.Value.ToString()
            };
        }

        public List<DomainSetting> GetDomainSettingList(int PlatformID = 0, int AdminID = 1)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };
            SqlParameter[] paras = {
                new SqlParameter("@intPlatformID", PlatformID),
                new SqlParameter("@intAdminID", AdminID),
                chvErrMsg
            };

            var list = this._repoFactory.Database().SqlQuery<DomainSetting>(@"PrWeb_DomainSettingList_ByPlatformID_Get @intPlatformID, @intAdminID, @chvMsg", paras).ToList();

            return list;
        }

        public bool UpdateDomainByID(DomainSettingModel model)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("@intID", (object)model.ID ?? DBNull.Value ),
                new SqlParameter("@chvDomain", (object)model.Domain ?? DBNull.Value ),
                chvErrMsg,
                intResult
            };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_DomainSetting_ByID_Update 
                    @intID,
                    @chvDomain,
                    @chvMsg",
                    paras);

            return (int)intResult.Value == 1;
        }
    }
}
