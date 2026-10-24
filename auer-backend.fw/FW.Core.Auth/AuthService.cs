using System;
using KCKJ.Core.Cache;


namespace KCKJ.Core.Auth
{
    public class AuthService : IAuthenService
    {
        private ICacheManager _cacheManager;
        private ISessionIDGenerator _sessionIDGenerator;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cacheManager"></param>
        /// <param name="sessionIDGenerator"></param>
        public AuthService(ICacheManager cacheManager, ISessionIDGenerator sessionIDGenerator)
        {
            KCKJ.Core.Helper.Check.NotEmpty<ICacheManager>(cacheManager, "cacheManager");
            KCKJ.Core.Helper.Check.NotEmpty<ISessionIDGenerator>(sessionIDGenerator, "sessionIDGenerator");

            _cacheManager = cacheManager;
            _sessionIDGenerator = sessionIDGenerator;
        }


        public string Signin(string userID, int persistTime)
        {
            try
            {
                string sessionID = _sessionIDGenerator.GenerateSessionID();
                _cacheManager.Add(sessionID, 
                    new LogonInfo() {
                        UserID = userID, 
                        PersistTime = persistTime, 
                        LogonTime = DateTime.Now 
                    }, persistTime);

                return sessionID;
            }
            catch (System.Exception ex)
            {
                throw new AuthException("执行登陆操作错误," + ex.Message, ex);
            }
        }

        public void Signout(string sessionID)
        {
            try
            {
                _cacheManager.Remove(sessionID);
            }
            catch (System.Exception ex)
            {
                throw new AuthException("执行登出错误," + ex.Message, ex);
            }
        }

        public string CheckUser(string sessionID)
        {
            try
            {
                var logonInfo = _cacheManager.GetObject<LogonInfo>(sessionID);
                if (logonInfo == null)
                {
                    return String.Empty;
                }

                //失效时间采用滑动方式
                if (((TimeSpan)(logonInfo.LogonTime.AddMilliseconds(logonInfo.PersistTime) - DateTime.Now)).TotalMilliseconds < logonInfo.PersistTime * 2 / 3)
                {
                    logonInfo.LogonTime = DateTime.Now;
                    _cacheManager.Add(sessionID, logonInfo, logonInfo.PersistTime);
                }

                return logonInfo.UserID;
            }
            catch (System.Exception ex)
            {
                throw new AuthException("获取用户凭据错误," + ex.Message, ex);
            }
        }


        private class LogonInfo
        {
            public string UserID
            {
                get;
                set;
            }

            public int PersistTime
            {
                get;
                set;
            }

            public DateTime LogonTime
            {
                get;
                set;
            }
        }
    }
}
