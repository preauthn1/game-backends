using FW.Core.Helper;
using FW.Core.Helper.Reflection;
using System;
using example.invalid;
using example.invalid;
using System.Text;


namespace FW.Core.Web.WebAPI
{
    public  class APICaller
    {
        #region private fields
        /// <summary>
        /// 默认编码格式
        /// </summary>
        public Encoding _defaultEncoding = Encoding.UTF8;
        private APIConsumerInfo _consumerInfo;
        private string _logonUrl;
        private string _baseUrl = "";
        private bool _logonFirst = true;
        #endregion

        #region ctor
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="consumerInfo"></param>
        /// <param name="logonUrl"></param>
        /// <param name="baseUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="logonFirst"></param>
        public APICaller(APIConsumerInfo consumerInfo, Encoding encoding, string baseUrl = "", string logonUrl = "Logon", bool logonFirst = true)
        {
            _consumerInfo = consumerInfo;
            _logonUrl = logonUrl;
            _defaultEncoding = encoding;
            _logonFirst = logonFirst;

            if (!String.IsNullOrWhiteSpace(baseUrl))
            {
                _baseUrl = baseUrl;
            }
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="key"></param>
        /// <param name="logonUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="baseUrl"></param>
        /// <param name="logonFirst"></param>
        public APICaller(string cid, string key, Encoding encoding, string baseUrl = "", string logonUrl = "Logon", bool logonFirst = true)
            :this(new APIConsumerInfo(cid, key), encoding, baseUrl,logonUrl, logonFirst)
        {  }
        #endregion

        #region private methods
        /// <summary>
        /// 构造Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string buildUrl(string url)
        {
            if (String.IsNullOrWhiteSpace(_baseUrl) || url.StartsWith("http://"))
            {
                return url;
            }

            return _baseUrl + url;
        }

        /// <summary>
        /// 检查登陆
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ensureLogon(ref string message)
        {
            try
            {
                //如果不需要登陆,或者已经登陆
                if (_consumerInfo.IsLogon || !_logonFirst)
                {
                    return true;
                }

                var ret = getPageData(_consumerInfo.GetLogonUrl(_logonUrl), "GET");
                var logonRet = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<LogonRet>(ret.Item2);
                if (logonRet.IsSuccess)
                {
                    _consumerInfo.RightToken = [REDACTED];
                    return true;
                }
                else
                {
                    message = logonRet.Message;
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取网页内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        private  Pair<HttpStatusCode,string> getUrlInternal(string url,string httpMethod)
        {
           string message = "";
            if (!ensureLogon(ref message))
            {
                Pair<HttpStatusCode, string> ret = new Pair<HttpStatusCode, string>();
                ret.Item1 = HttpStatusCode.Unauthorized;
                ret.Item2 = message;
                return ret;
            }

            return getPageData(url, httpMethod);
        }

        /// <summary>
        /// 获取网页内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        private Pair<HttpStatusCode, string> getPageData(string url, string httpMethod)
        {
            HttpWebRequest webReq = WebRequest.Create(buildUrl(url)) as HttpWebRequest;
            webReq.Proxy = null;
            webReq.Method = httpMethod;
            webReq.ContentType = "text/plain";

            if (!String.IsNullOrWhiteSpace(_consumerInfo.RightToken))
            {
                webReq.Headers.Add(Auth.Authen.HttpRequestAuthenService.HTTPREQUEST_TOKENKEY, _consumerInfo.RightToken);
            }

            try
            {
                HttpWebResponse webRes = webReq.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(webRes.GetResponseStream(), _defaultEncoding);
                string content = reader.ReadToEnd();
                reader.Close();

                return new Pair<HttpStatusCode, string>()
                {
                    Item1 = webRes.StatusCode,
                    Item2 = content
                };
            }
            catch (System.Exception ex)
            {
                return new Pair<HttpStatusCode, string>()
                {
                    Item1 = HttpStatusCode.BadRequest,
                    Item2 = ex.Message
                };
            }
        }

        /// <summary>
        /// 向网页提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Pair<HttpStatusCode, string> postUrlInternal(string url, string httpMethod, string data)
        {
            string message = "";
            if (!ensureLogon(ref message))
            {
                Pair<HttpStatusCode, string> ret = new Pair<HttpStatusCode, string>();
                ret.Item1 = HttpStatusCode.Unauthorized;
                ret.Item2 = message;
                return ret;
            }

            return postPageData(url, httpMethod, data);
        }

        /// <summary>
        /// 向网页提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Pair<HttpStatusCode, string> postPageData(string url, string httpMethod, string data)
        {
            HttpWebRequest webReq = WebRequest.Create(buildUrl(url)) as HttpWebRequest;
            webReq.Proxy = null;
            webReq.Method = httpMethod;
            webReq.ContentType = "application/x-www-form-urlencoded";
            byte[] postData = this._defaultEncoding.GetBytes(data);

            if (!String.IsNullOrWhiteSpace(_consumerInfo.RightToken))
            {
                webReq.Headers.Add(Auth.Authen.HttpRequestAuthenService.HTTPREQUEST_TOKENKEY, _consumerInfo.RightToken);
            }

            try
            {
                webReq.ContentLength = postData.Length;
                Stream outputStream = webReq.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                HttpWebResponse webRes = webReq.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(webRes.GetResponseStream(), _defaultEncoding);
                string content = reader.ReadToEnd();
                reader.Close();

                return new Pair<HttpStatusCode, string>()
                {
                    Item1 = webRes.StatusCode,
                    Item2 = content
                };
            }
            catch (System.Exception ex)
            {
                return new Pair<HttpStatusCode, string>()
                {
                    Item1 = HttpStatusCode.BadRequest,
                    Item2 = ex.Message
                };
            }
        }
        #endregion

        public Pair<HttpStatusCode, string> Get(string url)
        {
            return getUrlInternal(url, "GET");
        }

        public string GetRaw(string url)
        {
            return getUrlInternal(url, "GET").Item2;
        }

        public Pair<HttpStatusCode, string> Delete(string url)
        {
            return getUrlInternal(url, "DELETE");
        }

        public Pair<HttpStatusCode, string> Post(string url, string data)
        {
            return postUrlInternal(url, "POST", data);
        }

        public Pair<HttpStatusCode, string> Put(string url, string data)
        {
            return postUrlInternal(url, "PUT", data);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public TObject GetEntity<TObject>(string url)
        {
            var data = Get(url);
            if (data.Item1 == HttpStatusCode.OK)
            {
                if (TypeHelper.IsSimpleType<TObject>())
                {
                    return (TObject)((object)data.Item2.Replace("\"", ""));
                }

                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<TObject>(data.Item2);
            }

            return default(TObject);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteEntity(string url,ref string message)
        {
            var data = Delete(url);
            message = data.Item2;
            if (data.Item1 == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool DeleteEntity(string url)
        {
            string message = "";
            return DeleteEntity(url, ref message);
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public TObject PostEntity<TObject>(string url, string data)
        {
            var ret = Post(url, data);
            

            if (ret.Item1 == HttpStatusCode.OK)
            {
                if (TypeHelper.IsSimpleType<TObject>())
                {
                    return (TObject)((object)ret.Item2.Replace("\"", ""));
                }

                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<TObject>(ret.Item2);
            }

            return default(TObject);
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool PostEntity(string url, string data,ref string message)
        {
            var ret = Post(url, data);
            message = ret.Item2;
            if (ret.Item1 == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool PostEntity(string url, string data)
        {
            string message = "";
            return PostEntity(url, data, ref message);
        }

        /// <summary>
        /// 新增数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public TObject PutEntity<TObject>(string url, string data)
        {
            var ret = Put(url, data);
            

            if (ret.Item1 == HttpStatusCode.OK)
            {
                if (TypeHelper.IsSimpleType<TObject>())
                {
                    return (TObject)((object)ret.Item2.Replace("\"", ""));
                }

                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<TObject>(ret.Item2);
            }

            return default(TObject);
        }


        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool PutEntity(string url, string data, ref string message)
        {
            var ret = Put(url, data);
            message = ret.Item2;

            if (ret.Item1 == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public bool PutEntity(string url, string data)
        {
            string message = "";
            return PutEntity(url, data, ref message);
        }

    }
}
