using FW.Core.Engine;
using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using FW.Core.Auth.Authen;
using example.invalid;


namespace FW.Core.Web.WebAPI
{
    public class ApiLogonHandler : IHttpHandler
    {
        private APIConsumerCollection _apiColls;

        /// <summary>
        /// ctor
        /// </summary>
        public ApiLogonHandler()
        {
            _apiColls = EngineContext.Current.Resolve<IAPIConsumerProvider>().GetApiConsumers();
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string cid = context.Request.QueryString["cid"];
            string key = context.Request.QueryString["key"];

            var ret = new LogonRet();
            
            if (String.IsNullOrWhiteSpace(cid) || String.IsNullOrWhiteSpace(key))
            {
                ret.IsSuccess = false;
                ret.Message = "未设置身份信息";
            }
            else if (_apiColls.CheckApi(cid, key))
            {
                ret.IsSuccess = true;
                ret.Token = [REDACTED] Auth.AuthUser() { UserID = cid });
            }
            else
            {
                ret.IsSuccess = false;
                ret.Message = "身份验证失败";
            }

            context.Response.Write(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ret));
            context.Response.End();
        }

        
    }

    /// <summary>
    /// 登陆结果
    /// </summary>
    internal class LogonRet
    {
        /// <summary>
        /// ctor
        /// </summary>
        public LogonRet()
        {
            IsSuccess = false;
            Token = [REDACTED];
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get;
            set;
        }

        public string Token
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
