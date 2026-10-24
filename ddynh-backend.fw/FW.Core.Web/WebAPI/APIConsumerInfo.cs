using System;


namespace FW.Core.Web.WebAPI
{
    public class APIConsumerInfo
    {
        /// <summary>
        /// 匿名API调用客户端身份
        /// </summary>
        public static APIConsumerInfo Guest = new APIConsumerInfo();

        /// <summary>
        /// ctor
        /// </summary>
        public APIConsumerInfo()
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="key"></param>
        public APIConsumerInfo(string cid, string key)
        {
            CID = cid;
            Key = key;
        }

        /// <summary>
        /// API调用方标识
        /// </summary>
        public string CID
        {
            get;
            set;
        }

        /// <summary>
        /// 调用秘钥
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// 权限标识
        /// </summary>
        internal string RightToken
        {
            get;
            set;
        }

        /// <summary>
        /// 当前用户是否已经登录
        /// </summary>
        internal bool IsLogon
        {
            get
            {
                return !String.IsNullOrWhiteSpace(RightToken);
            }
        }

        public override string ToString()
        {
            return String.Format("cid={0}&key={1}", CID, Key);
        }

        internal string GetLogonUrl(string logonUrl)
        {
            return String.Format("{0}?{1}", logonUrl, this.ToString());
        }

    }
}
