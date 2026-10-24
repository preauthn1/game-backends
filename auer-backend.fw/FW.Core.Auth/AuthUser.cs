using FW.Core.Auth.Token;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Text;
using System.Xml.Serialization;


namespace FW.Core.Auth
{
    public class AuthUser
    {
        /// <summary>
        /// 访客
        /// </summary>
        public static AuthUser Guest = new AuthUser();

        /// <summary>
        /// ctor
        /// </summary>
        public AuthUser()
        {
            Roles = new List<string>();
            Rights = new List<string>();
        }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string LoginName
        {
            get;
            set;
        }

        public DateTime LogonTime
        {
            get;
            set;
        }

        /// <summary>
        /// 使用者類別
        /// </summary>
        public byte UserType
        {
            get;
            set;
        }

        /// <summary>
        /// 用户所拥有的角色
        /// </summary>
        public List<string> Roles
        {
            get;
            set;
        }

        /// <summary>
        /// 用户所拥有的权限
        /// </summary>
        public List<string> Rights
        {
            get;
            set;
        }

        /// <summary>
        /// 是否访客
        /// </summary>
        public bool IsGuest
        {
            get
            {
                return String.IsNullOrWhiteSpace(UserID) || UserID == "0";
            }
        }

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string ExtendInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 获取JSON格式扩展信息
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject GetJSONExtendInfo<TObject>()
        {
            if (String.IsNullOrWhiteSpace(ExtendInfo))
            {
                return default(TObject);
            }

            return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<TObject>(ExtendInfo);
        }

        /// <summary>
        /// 获取XML格式扩展信息
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject GetXMLExtendInfo<TObject>()
        {
            if (String.IsNullOrWhiteSpace(ExtendInfo))
            {
                return default(TObject);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(TObject));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ExtendInfo)))
            {
                return (TObject)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// 金鑰
        /// </summary>
        public int SecretKey { get; set; }
    }
}
