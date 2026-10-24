using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using example.invalid;
using System.Text;
using System.Configuration;
using MaxMind.GeoIP2;
using example.invalid;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using example.invalid.Http;
using MaxMind.Db;
using MaxMind.GeoIP2.Responses;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace game.Transform
{
    /*
    public class TransformServices
    {


        #region 參數屬性
        static string cacheKey = "BackupNodeIPList";
        static string _filelog = "tf.log";
        static string _appleFileLog = "apple_tf.log";

        /// <summary>
        /// Log id
        /// </summary>
        private string LOGID { get; set; }
        /// <summary>
        /// log 內容列表
        /// </summary>
        private List<string> LOGBAG { get; } = new List<string>();

        /// <summary>
        /// 資料庫連線
        /// </summary>
        private static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }

        /// <summary>
        /// 是否透過轉發 
        ///    0否, 直連大廳
        ///    1是, 透過節點防護位置取得節點
        ///    2是, 從設定的備用節點取得節點
        /// </summary>
        private static string IsTransfer
        {
            get
            {
                return ConfigurationManager.AppSettings["IsTransfer"].ToString();
            }
        }

        /// <summary>
        /// 是否上报userIP给大厅
        /// </summary>
        private static string GiveUserIP
        {
            get
            {
                return ConfigurationManager.AppSettings["GiveUserIP"].ToString();
            }
        }

        /// <summary>
        /// 是否緩存User取得的節點位置  1是 0否
        /// </summary>
        private static bool CacheUserNodeIP
        {
            get {
                return ConfigurationManager.AppSettings["CacheUserNodeIP"] != null ?
                    ConfigurationManager.AppSettings["CacheUserNodeIP"] == "1" ? true : false
                     : false;
            }
        }
        /// <summary>
        /// 緩存使用者取得的節點位置持續時間(單位秒)
        /// </summary>
        private static int CacheUserNodeIPDuration
        {
            get
            {
                return ConfigurationManager.AppSettings["CacheUserNodeIPDuration"] != null ?
                    Convert.ToInt32(ConfigurationManager.AppSettings["CacheUserNodeIPDuration"])
                     : 30;
            }
        }

        /// <summary>
        /// 直連大廳位置
        /// </summary>
        private static string DirectHallAddr
        {
            get
            {
                return ConfigurationManager.AppSettings["DirectHallAddr"].ToString();
            }
        }

        /// <summary>
        /// 提審服大廳位置
        /// </summary>
        private static string AppleHallAddr
        {
            get
            {
                return ConfigurationManager.AppSettings["AppleHallAddr"].ToString();
            }
        }

        /// <summary>
        /// 節點防護位置
        /// </summary>
        private static string ProtectNodeServer
        {
            get
            {
                return ConfigurationManager.AppSettings["ProtectNodeServer"].ToString();
            }
        }

        /// <summary>
        /// web.config設定的备用节点清单
        /// </summary>
        private static string BackupNodeAddrList
        {
            get
            {
                return ConfigurationManager.AppSettings["BackupNodeAddr"];
            }
        }

        /// <summary>
        /// 取得節點後轉發的port
        /// </summary>
        private static string TransferPort
        {
            get
            {
                return ConfigurationManager.AppSettings["TransferPort"].ToString();
            }
        }


        /// <summary>
        /// 取得IP資料庫
        /// </summary>
        /// <returns></returns>
        private static string GeoIPFile() { return System.Web.HttpContext.Current.Server.MapPath("/GeoLite2-Country.mmdb"); }

        /// <summary>
        /// 是否紀錄filelog 0否 1是
        /// </summary>
        private static string LogEnable
        {
            get
            {
                return ConfigurationManager.AppSettings["LogEnable"].ToString();
            }
        }

        /// <summary>
        /// AES key
        /// </summary>
        public static string SecKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SecretKey"]?.ToString();
            }
        }

        /// <summary>
        /// AES IV
        /// </summary>
        public static string SecIV
        {
            get
            {
                return ConfigurationManager.AppSettings["SecretIV"]?.ToString();
            }
        }

        /// <summary>
        /// A/B服測試 0不啟用 1啟用
        /// </summary>
        private static bool ABTest
        {
            get
            {
                return ConfigurationManager.AppSettings["ABTest"] != null ?
                    ConfigurationManager.AppSettings["ABTest"] == "1" ? true : false
                     : false;
            }
        }
        /// <summary>
        /// A/B服轉發port
        /// </summary>
        private static int ABTestTransferPort
        {
            get
            {
                return ConfigurationManager.AppSettings["ABTestTransferPort"] != null ?
                    Convert.ToInt32(ConfigurationManager.AppSettings["ABTestTransferPort"])
                     : 10000;
            }
        }

        #endregion


        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="logid"></param>
        public TransformServices(string logid)
        {
            LOGID = logid;
        }

        #region 正常轉發遊戲服

        /// <summary>
        /// 取得hallserverIP
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public  async Task<string> GetAddrAsync(string gui, string channel, int simulation, int? userid = null, int? levelid = 0, string key = null)
        {
            //取得使用者IP國家
            string from = "0";
            string addr;

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;
            //string userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];

            //log request params
            pushLog($"Request gui={gui} channel={channel} simulation={simulation} user_id={userid} userip={userIP} levelid {levelid}");


            from = FromCountry(userIP);
            if (from == "-1")
                return "IP_Database_not_found";


            if (gui == "" || channel == "" || userid == null)
                return "Invalid_Parameter";



            if (IsTransfer == "2")
            {
                //取得隨機備用節點
                pushLog($"BackupNode");
                addr = await GetBackupNodeAddrAsync();
            }
            else if (IsTransfer == "0")
            {
                //不轉發,走直連
                pushLog($"DirectHallServer");
                addr = DirectHallAddr;
            }
            else
            {
                //走轉發
                pushLog($"Transfer");
                addr = await GetNodeAsyc(gui, (int)levelid, (int)userid, channel, simulation, userIP);
            }

            string node = $"{addr}:{TransferPort}|{GiveUserIP}|{from}";
            pushLog($"GetAddr {node}");

            var t = WriteLogAsync(_filelog);

            return node;
        }

        /// <summary>
        /// 取得hallserverIP
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> TransferAsync(string gui, string channel, int simulation, int? userid = null, int? levelid = 0, string key = null)
        {
            

            if (gui == "" || channel == "" || userid == null)
                return  "Invalid_Parameter";

            //返回的節點IP位置
            string addr;
            //使用者IP
            string userIP;
            //使用者IP對應國家
            string from;

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;
            //string userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];


            //log request params
            pushLog($"Request gui={gui} channel={channel} simulation={simulation} user_id={userid} userip={userIP} levelid {levelid}");


            //取得使用者IP國家
            from = FromCountry(userIP);
            if (from == "-1")
            {
                return "IP_Database_not_found";
            }
            
            if (IsTransfer == "2")
            {
                //取得隨機備用節點
                pushLog($"BackupNode");
                addr = await GetBackupNodeAddrAsync();
            }
            else if (IsTransfer == "0")
            {
                //不轉發,走直連
                pushLog($"DirectHallServer");
                addr = DirectHallAddr;
            }
            else
            {
                //走轉發
                pushLog($"Transfer");
                addr = await GetNodeAsyc(gui, (int)levelid, (int)userid, channel, simulation, userIP);
            }

            //是否AB測試
            if (userIP == "0.0.0.0" && ABTest)
            {
                string ab = $"{addr}:{ABTestTransferPort}|{GiveUserIP}|{from}|{userIP}";
                pushLog($"Node {ab}");
                var w = WriteLogAsync(_filelog);
                return ab;
            }

            string node = $"{addr}:{TransferPort}|{GiveUserIP}|{from}|{userIP}";
            pushLog($"Node {node}");

            var t = WriteLogAsync(_filelog);

            return node;

        }

        /// <summary>
        /// 取得節點IP
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="levelid"></param>
        /// <param name="userid"></param>
        /// <param name="channel"></param>
        /// <param name="simulation"></param>
        /// <param name="userIP"></param>
        /// <param name="logid"></param>
        /// <returns></returns>
        private async Task<string> GetNodeAsyc(string gui, int levelid, int userid, string channel, int simulation, string userIP)
        {
            
            string nodeIP = ""; 
            string url = ProtectNodeServer;
            string querystring = $"gui={gui}&vip={levelid}&user_id={userid}&channel={channel}&simulation={simulation}&ip={userIP}&login=ok";
            string targetUrl = url + "/?" + querystring;


            //如果cache存在, 優先從cache取得
            string cache_userNodeIP = userid.ToString() + "_nodeip";
            if (CacheUserNodeIP)
            {
                var cache = GetCache(cache_userNodeIP);
                if (cache != null)
                {
                    nodeIP = cache.ToString();
                    pushLog($"Get cache node {nodeIP} userid {userid} userip {userIP}");
                    return nodeIP;
                }
            }

            //從節點防護取得節點IP
            try
            {
                pushLog($"Get {targetUrl}");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 2);
                    nodeIP = await client.GetStringAsync(targetUrl);
                }

            }
            catch (Exception e)
            {
                pushLog($"GetNodeAsyc exception error: {e.Message}");
            }

            //如果取得失敗, 從備用IP取得
            if (nodeIP.Length == 0 || nodeIP == "0" || !CheckIP(nodeIP))
            {
                nodeIP = await GetBackupNodeAddrAsync();
                //SendAlert("方舟节点防护异常, 使用备用节点");
                pushLog($"Get backup node {nodeIP} userid {userid} userip {userIP}");
            }
            else
            {
                //寫入到cache,  預設30後一律過期
                if (CacheUserNodeIP)
                    SetCache(cache_userNodeIP, nodeIP, DateTime.Now.AddSeconds(CacheUserNodeIPDuration));
            }

            return nodeIP;
        }

        /// <summary>
        /// 如果主要節點IP發生問題, 改用備用IP
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetBackupNodeAddrAsync()
        {
            string iplist = "";
            string[] BackupIPList = null;

            //如果cache存在, 優先從cache取得
            var cache = GetCache(cacheKey);
            if (cache != null)
            {
                BackupIPList = cache.ToString().Split(',');
                if (BackupIPList != null && BackupIPList.Length > 0)
                {
                    return GetRandomAddr(BackupIPList);
                }
            }


            //優先從web.config的設定取得, 如果設定值不存在, 則從DB取得備用節點
            if (BackupNodeAddrList != null && BackupNodeAddrList.Length > 0)
            {
                iplist = Regex.Replace(BackupNodeAddrList,@"\s",string.Empty);
            }
            //從DB取得備援節點資訊
            else
            {
                using (SqlConnection conn = new SqlConnection(dbConnection))
                {
                    conn.Open();
                    SqlDataReader rd = null;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            string sql = "Select IP from Serverinfo.ProtectNodeBackupIP with (nolock)";
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;
                            rd = await cmd.ExecuteReaderAsync();

                            if (rd.Read())
                            {
                                iplist = Regex.Replace(rd["IP"].ToString(), @"\s", string.Empty);
                            }

                            rd.Close();
                        }
                        finally
                        {
                            if (rd != null && !rd.IsClosed)
                                rd.Close();
                        }
                    }
                    conn.Close();
                }
            }

            
            if (iplist.Length > 0)
            {
                BackupIPList = iplist.Split(',');
            }

            if (BackupIPList != null && BackupIPList.Length > 0)
            {
                //寫入到cache,  24hr後一律過期
                //SetCache(cacheKey, iplist, DateTime.Now.AddHours(24));

                //寫入到cache,  1小時後沒被使用過期
                SetCache(cacheKey, iplist, TimeSpan.FromHours(1));

                return GetRandomAddr(BackupIPList);
            }

            return "";
        }
        private string GetRandomAddr(string[] list)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, list.Length);
            return list[randomNumber].Trim();
        }
        #endregion

        #region 提審服功能

        public async Task<string> GetAddrInAppleReviewAsync(int? userid = null, int? areaid = null, int? levelid = null, string HDcode = "", string key = "")
        {
            List<string> logbag = new List<string>();

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;
            //string userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];

            
            //取得使用者IP國家
            string from = FromCountry(userIP);
            //連線字串
            string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            
            //apple審核員
            AppleReviewUser user = null;
            //審核中的AreaID
            int ReviewAreaID = 0;
            //審核階段紀錄的IP
            string ReviewIP = "";

            //紀錄request參數
            pushLog($"Request: userid {userid} areaID {areaid} levelID {levelid} HDCode {HDcode} UserIP {userIP}");

            // 檢查登入者, 如果是送審階段, 紀錄連線使用者(可能是審核員)
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                String strSQL = $"exec [CenterDB].[dbo].[PrTF_UserLogon] {areaid},{userid},'{HDcode}','{userIP}'";

                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {

                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "Review");

                        //user data
                        DataTable UserTable = ds.Tables[0];

                        if (UserTable.Rows.Count == 1)
                        {
                            user = new AppleReviewUser();
                            user.UserID = Convert.ToInt32(UserTable.Rows[0]["UserID"]);
                            user.HDCode = UserTable.Rows[0]["HDCode"].ToString();
                            user.LastLogonTime = Convert.ToDateTime(UserTable.Rows[0]["LastLogonTime"]);
                            user.LastLogonIPAddress = UserTable.Rows[0]["LastLogonIPAddress"].ToString();
                            user.UserFlag = Convert.ToInt16(UserTable.Rows[0]["UserFlag"]); //0: 美服資格 1:國服資格
                        }

                        //Review AreaID
                        DataTable ReviewAreaTable = ds.Tables[1];
                        if (ReviewAreaTable.Rows.Count == 1)
                            ReviewAreaID = Convert.ToInt32(ReviewAreaTable.Rows[0]["ReviewAreaID"]);

                        //取得曾經紀錄的IP
                        DataTable ReviewIPTable = ds.Tables[2];
                        if (ReviewIPTable.Rows.Count == 1)
                            ReviewIP = ReviewIPTable.Rows[0]["IP"].ToString();


                    }
                    catch (Exception ex)
                    {
                        //return ex.Message;
                    }

                }
            }

            if (userid == null) userid = 0;
            if (levelid == null) levelid = 0;

            //檢查是否走審核服
            bool goReview = goReviewServer(user, ReviewIP, from);

            if (goReview)
            {
                string ret = $"{AppleHallAddr}:{TransferPort}|1|{from}|{userIP}";
                pushLog(ret);

                var t = WriteLogAsync(_appleFileLog);

                return ret;
            }
            else
            {
                string addr;
                //transfer=2, 強制走備援節點
                if (IsTransfer == "2")
                {
                    addr = await GetBackupNodeAddrAsync(); //GetBackupNodeAddrFromConfig();
                }
                else
                //走一般節點
                {
                    addr = await GetNodeAsyc(HDcode, (int)levelid, (int)userid, "0", 0, userIP);
                }

                string ret = $"{addr}:{TransferPort}|1|{from}|{userIP}";
                pushLog(ret);

                var t = WriteLogAsync(_appleFileLog);

                return ret;
            }

        }

        private bool goReviewServer(AppleReviewUser user, string reviewIP, string from)
        {
            //1例外者 0審核員
            if (user != null)
            {

                if (user.UserFlag == 1)
                {
                    pushLog("special user");
                    return false;
                }
                else
                {
                    pushLog("match reviewer");
                    return true;
                }
            }

            //吻合審核階段紀錄IP
            if (reviewIP != "")
            {
                pushLog("match reviewer ip");
                return true;
            }

            //非中港澳台一律走美服
            if (from == "CN" || from == "TW" || from == "HK" || from == "LA")
            {
                pushLog("CN TW HK LA user");
                return false;
            }
            else
            {
                pushLog("non CN TW HK LA user");
                return true;
            }

        }

        #endregion


        /// <summary>
        /// log紀錄清單
        /// </summary>
        /// <param name="data"></param>
        private void pushLog(string data)
        {
            LOGBAG.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} {LOGID} {data}{Environment.NewLine}");
        }

        private static object logSyncObject = new object();

        /// <summary>
        /// 寫log
        /// </summary>
        /// <param name="fileLogName"></param>
        /// <returns></returns>
        private async Task WriteLogAsync(string fileLogName)
        {
            if (LogEnable == "1")
            {
                string data = LOGBAG.Aggregate((a, b) => a + b);
                var path = HttpContext.Current.Server.MapPath($"/logcustom/{DateTime.Now.ToString("yyyyMMdd")}/");
                await FileLogAsync(path, data, fileLogName);
            }

        }
        /// <summary>
        /// 寫log
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private async Task FileLogAsync(string filePath, string message, string filename)
        {
            if (!Directory.Exists(filePath))
            {
                lock (logSyncObject)
                {
                    Directory.CreateDirectory(filePath);
                }
            }

            var path = filePath + filename;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            Int32 offset = 0;
            Int32 sizeOfBuffer = 4096;

            using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, bufferSize: sizeOfBuffer, useAsync: true))
            {
                await fileStream.WriteAsync(buffer, offset, buffer.Length); //.ConfigureAwait(false);
            }

        }

        /// <summary>
        /// 檢查 IP 是否合法
        /// </summary>
        private static bool CheckIP(string strPattern)
        {
            Regex regex = new Regex("^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$");
            Match m = regex.Match(strPattern);
            return m.Success;
        }
        /// <summary>
        /// 從IP取得地區
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string FromCountry(string ip)
        {
            IPAddress ipAddress;
            CountryResponse countryResponse;
            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipAddress))
            {
                return "0";
            }

            try
            {
                using (var reader = new DatabaseReader(GeoIPFile(), FileAccessMode.MemoryMapped))
                {
                    if (!reader.TryCountry(ipAddress, out countryResponse))
                    {
                        return "0";
                    }
                    return countryResponse.Country.IsoCode;
                }
            }
            catch (FileNotFoundException e)
            {
                return "-1";
            }

        }

        /// <summary>
        /// 設定快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="absoluteExpiration">時間一到就過期</param>
        private static void SetCache(string CacheKey, object objObject, DateTime absoluteExpiration)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, absoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration);
        }

        /// <summary>
        /// 設定快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="slidingExpiration">最後訪問超過某個時間段後就過</param>
        private static void SetCache(string CacheKey, object objObject, TimeSpan slidingExpiration)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, System.Web.Caching.Cache.NoAbsoluteExpiration, slidingExpiration);
        }
        /// <summary>
        /// 讀取快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        private static object GetCache(string CacheKey)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            return objCache[CacheKey];
        }
        /// <summary>
        /// 清除快取
        /// </summary>
        public static void ResetCache()
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Remove(cacheKey);
        }

        /// <summary>
        /// 建立一個TFserver驗證的token
        /// </summary>
        /// <returns></returns>
        private static string GetSecToken()
        {
            //double timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            string token = [REDACTED], 5);
            return Convert.ToBase64String(AESEncryption.AESEncrypt($"{token}:{timeStamp.ToString()}",SecKey));
        }
    }
    */
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    public class TransferServices
    {


        #region 參數屬性
        static string cacheKey = "BackupNodeIPList";
        static string _filelog = "tf.log";
        static string _reviewFileLog = "reviewtf.log";

        /// <summary>
        /// Log id
        /// </summary>
        private string LOGID { get; set; }
        /// <summary>
        /// log 內容列表
        /// </summary>
        private List<string> LOGBAG { get; } = new List<string>();

        /// <summary>
        /// 紀錄從反攻機取得節點時, 錯誤的資訊
        /// </summary>
        private string ERRMSG_GetNode = "";

        /// <summary>
        /// 資料庫連線
        /// </summary>
        private static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }

        /// <summary>
        /// 是否透過轉發 
        ///    0否, 直連大廳
        ///    1是, 透過節點防護位置取得節點
        ///    2是, 從設定的備用節點取得節點
        /// </summary>
        private static string IsTransfer
        {
            get
            {
                return ConfigurationManager.AppSettings["IsTransfer"].ToString();
            }
        }

        /// <summary>
        /// 是否上报userIP给大厅
        /// </summary>
        private static int GiveUserIP
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GiveUserIP"]);
            }
        }

        /// <summary>
        /// 是否緩存User取得的節點位置  1是 0否
        /// </summary>
        private static bool CacheUserNodeIP
        {
            get
            {
                return ConfigurationManager.AppSettings["CacheUserNodeIP"] != null ?
                    ConfigurationManager.AppSettings["CacheUserNodeIP"] == "1" ? true : false
                     : false;
            }
        }
        /// <summary>
        /// 緩存使用者取得的節點位置持續時間(單位秒)
        /// </summary>
        private static int CacheUserNodeIPDuration
        {
            get
            {
                return ConfigurationManager.AppSettings["CacheUserNodeIPDuration"] != null ?
                    Convert.ToInt32(ConfigurationManager.AppSettings["CacheUserNodeIPDuration"])
                     : 30;
            }
        }

        /// <summary>
        /// 直連大廳位置
        /// </summary>
        private static string DirectHallAddr
        {
            get
            {
                return ConfigurationManager.AppSettings["DirectHallAddr"].ToString();
            }
        }

        /// <summary>
        /// 提審服大廳位置
        /// </summary>
        private static string ReviewHallAddr
        {
            get
            {
                return ConfigurationManager.AppSettings["AppleHallAddr"].ToString();
            }
        }
        /// <summary>
        /// 提審服大廳位置 (UI3.0)
        /// </summary>
        private static string ReviewHallAddr2
        {
            get
            {
                return ConfigurationManager.AppSettings["AppleHallAddr2"].ToString();
            }
        }
        /// <summary>
        /// 提審服大廳位置 (UI3.1)
        /// </summary>
        private static string ReviewHallAddr3_1
        {
            get
            {
                return ConfigurationManager.AppSettings["AppleHallAddr3.1"].ToString();
            }
        }
        /// <summary>
        /// 節點防護位置
        /// </summary>
        private static string ProtectNodeServer
        {
            get
            {
                return ConfigurationManager.AppSettings["ProtectNodeServer"].ToString();
            }
        }

        /// <summary>
        /// web.config設定的备用节点清单
        /// </summary>
        private static string BackupNodeAddrList
        {
            get
            {
                return ConfigurationManager.AppSettings["BackupNodeAddr"];
            }
        }

        /// <summary>
        /// 取得節點後轉發的port
        /// </summary>
        private static int TransferPort
        {
            get
            {
                return ConfigurationManager.AppSettings["TransferPort"] != null ?
                    Convert.ToInt32(ConfigurationManager.AppSettings["TransferPort"])
                     : 10000;
            }
        }


        /// <summary>
        /// 取得IP資料庫
        /// </summary>
        /// <returns></returns>
        private static string GeoIPFile() { return System.Web.HttpContext.Current.Server.MapPath("/GeoLite2-Country.mmdb"); }

        /// <summary>
        /// 是否紀錄filelog 0否 1是
        /// </summary>
        private static string LogEnable
        {
            get
            {
                return ConfigurationManager.AppSettings["LogEnable"].ToString();
            }
        }

        /// <summary>
        /// AES key
        /// </summary>
        public static string SecKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SecretKey"]?.ToString();
            }
        }

        /// <summary>
        /// AES IV
        /// </summary>
        public static string SecIV
        {
            get
            {
                return ConfigurationManager.AppSettings["SecretIV"]?.ToString();
            }
        }

        /// <summary>
        /// AES key for Token
        /// </summary>
        public static string SecTokenKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SecretTokenKey"]?.ToString();
            }
        }


        /// <summary>
        /// A/B服測試 0不啟用 1啟用
        /// </summary>
        private static bool ABTest
        {
            get
            {
                return ConfigurationManager.AppSettings["ABTest"] != null ?
                    ConfigurationManager.AppSettings["ABTest"] == "1" ? true : false
                     : false;
            }
        }
        /// <summary>
        /// A/B服轉發port
        /// </summary>
        private static int ABTestTransferPort
        {
            get
            {
                return ConfigurationManager.AppSettings["ABTestTransferPort"] != null ?
                    Convert.ToInt32(ConfigurationManager.AppSettings["ABTestTransferPort"])
                     : 10000;
            }
        }


        private static bool NodeLog2DB
        {
            get
            {
                return ConfigurationManager.AppSettings["NodeLog2DB"] != null ?
                    ConfigurationManager.AppSettings["NodeLog2DB"] == "1" ? true : false
                     : false;
            }
        }


        private static string HostName
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["TFName"] != null ? ConfigurationManager.AppSettings["TFName"] : Dns.GetHostName();
                    //return Dns.GetHostName();
                    //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    //IPAddress ipAddress = ipHostInfo.AddressList[0];
                    //return ipAddress.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }


        #endregion


        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="logid"></param>
        public TransferServices(string logid)
        {
            LOGID = logid;
        }

        #region 正常轉發遊戲服

        /// <summary>
        /// 取得hallserverIP
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<NodeResult> GetAddrAsync(string gui, string channel, int simulation, int userid, int vip)
        {
            //取得使用者IP國家
            string from;
            string addr;

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;
            string userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];

            //log request params
            pushLog($"Request gui={gui} channel={channel} simulation={simulation} user_id={userid} userip={userIP} levelid {vip}");


            from = FromCountry(userIP);
            if (from == "-1")
            {
                return new NodeResult() { data = null, message = "IP_Database_not_found", result = 0 };
            }

            if (gui.Length == 0 || channel.Length == 0)
                return new NodeResult() { data = null, message = "Invalid_Parameter", result = 0 };

            if (IsTransfer == "2")
            {
                //取得隨機備用節點
                pushLog($"BackupNode");
                addr = await GetBackupNodeAddrAsync();
            }
            else if (IsTransfer == "0")
            {
                //不轉發,走直連
                pushLog($"DirectHallServer");
                addr = DirectHallAddr;
            }
            else
            {
                //走轉發
                pushLog($"Transfer");
                addr = await GetNodeAsyc(gui, vip, userid, channel, simulation, userIP);
            }

            //create token
            Token tk = new Token();
            string token = [REDACTED], SecTokenKey)).Replace("-", "");

            pushLog($"GetAddr {addr}:{TransferPort} giveUserIP {GiveUserIP} from {from} userIP {userIP} token {tk.value}");
            var t = WriteLogAsync(_filelog);

            return new NodeResult() { token= [REDACTED], data = new NodeData() { Addr = addr, From = from, GiveUserIP = GiveUserIP, TransferPort = TransferPort, UserIP = userIP }, message = "", result = 1 };

        }

        /// <summary>
        /// 取得hallserverIP
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<NodeResult> TransferAsync(string gui, string channel, int simulation, int userid, int vip, int action)
        {

            if (gui.Length == 0 || channel.Length == 0)
                return new NodeResult() { data = null, message = "Invalid_Parameter", result = 0 };

            //返回的節點IP位置
            string addr;
            //使用者IP
            string userIP;
            //使用者IP對應國家
            string from;

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;
            userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];


            //log request params
            pushLog($"Request gui={gui} channel={channel} simulation={simulation} user_id={userid} userip={userIP} vip {vip}");


            //取得使用者IP國家
            from = FromCountry(userIP);
            if (from == "-1")
            {
                return new NodeResult() { data = null, message = "IP_Database_not_found", result = 0 };
            }



            string log_backupNodeIP = "";
            string log_normalNodeIP = "";
            if (IsTransfer == "2" || action == 2)
            {
                //取得隨機備用節點
                pushLog($"BackupNode");
                addr = await GetBackupNodeAddrAsync();
                log_backupNodeIP = addr;
            }
            else if (IsTransfer == "0")
            {
                //不轉發,走直連
                pushLog($"DirectHallServer");
                addr = DirectHallAddr;
                log_normalNodeIP = addr;
            }
            else
            {
                //走轉發
                pushLog($"Transfer");
                addr = await GetNodeAsyc(gui, vip, userid, channel, simulation, userIP);
                log_normalNodeIP = addr;
            }

            //create token
            Token tk= new Token();
            string token = [REDACTED], SecTokenKey)).Replace("-", "");


            //是否AB測試
            if (userIP == "0.0.0.0" && ABTest)
            {
                pushLog($"Node {addr}:{ABTestTransferPort} giveUserIP {GiveUserIP} from {from} userIP {userIP}");
                var w = WriteLogAsync(_filelog);
                return new NodeResult() { token = [REDACTED], data = new NodeData() { Addr = addr, From=from, GiveUserIP=GiveUserIP, TransferPort = ABTestTransferPort, UserIP=userIP }, message = "", result = 1 };
            }

            //写节点获取记录到数据库
            if (NodeLog2DB)
            {
                //var dbresult = WriteProtectNodeLogAsync(tk.value, userid, action, log_backupNodeIP, log_normalNodeIP, userIP, from, gui);
                 WriteProtectNodeLog(tk.value, userid, action, log_backupNodeIP, log_normalNodeIP, userIP, from, gui, ERRMSG_GetNode);
            }

            pushLog($"Node {addr}:{TransferPort} giveUserIP {GiveUserIP} from {from} userIP {userIP} trasferType {IsTransfer} token {tk.value} action {action} host {HostName}");
            var t = WriteLogAsync(_filelog);


            //返回节点结果
            return new NodeResult() { token = [REDACTED], data = new NodeData() { Addr = addr, From = from, GiveUserIP = GiveUserIP, TransferPort = TransferPort, UserIP = userIP }, message = "", result = 1 };

        }

        /// <summary>
        /// 取得節點IP
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="levelid"></param>
        /// <param name="userid"></param>
        /// <param name="channel"></param>
        /// <param name="simulation"></param>
        /// <param name="userIP"></param>
        /// <param name="logid"></param>
        /// <returns></returns>
        private async Task<string> GetNodeAsyc(string gui, int vip, int userid, string channel, int simulation, string userIP)
        {

            string nodeIP = "";
            string url = ProtectNodeServer;
            string querystring = $"gui={gui}&vip={vip}&user_id={userid}&channel={channel}&simulation={simulation}&ip={userIP}&login=ok";
            string targetUrl = url + "/?" + querystring;


            //如果cache存在, 優先從cache取得
            string cache_userNodeIP = userid.ToString() + "_nodeip";
            if (CacheUserNodeIP)
            {
                var cache = GetCache(cache_userNodeIP);
                if (cache != null)
                {
                    nodeIP = cache.ToString();
                    pushLog($"Get cache node {nodeIP} userid {userid} userip {userIP}");
                    return nodeIP;
                }
            }

            //從節點防護取得節點IP
            try
            {
                pushLog($"Get {targetUrl}");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 2);
                    nodeIP = await client.GetStringAsync(targetUrl);
                }

            }
            catch (Exception e)
            {
                pushLog($"GetNodeAsyc exception error: {e.Message}");
                ERRMSG_GetNode = e.Message;
            }

            //如果取得失敗, 從備用IP取得
            if (nodeIP.Length == 0 || nodeIP == "0" || !CheckIP(nodeIP))
            {
                nodeIP = await GetBackupNodeAddrAsync();
                //SendAlert("方舟节点防护异常, 使用备用节点");
                pushLog($"Get backup node {nodeIP} userid {userid} userip {userIP}");
            }
            else
            {
                //寫入到cache,  預設30後一律過期
                if (CacheUserNodeIP)
                    SetCache(cache_userNodeIP, nodeIP, DateTime.Now.AddSeconds(CacheUserNodeIPDuration));
            }

            return nodeIP;
        }

        /// <summary>
        /// 如果主要節點IP發生問題, 改用備用IP
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetBackupNodeAddrAsync()
        {
            string iplist = "";
            string[] BackupIPList = null;

            //如果cache存在, 優先從cache取得
            var cache = GetCache(cacheKey);
            if (cache != null)
            {
                BackupIPList = cache.ToString().Split(',');
                if (BackupIPList != null && BackupIPList.Length > 0)
                {
                    return GetRandomAddr(BackupIPList);
                }
            }


            //優先從web.config的設定取得, 如果設定值不存在, 則從DB取得備用節點
            if (BackupNodeAddrList != null && BackupNodeAddrList.Length > 0)
            {
                iplist = Regex.Replace(BackupNodeAddrList, @"\s", string.Empty);
            }
            //從DB取得備援節點資訊
            else
            {
                using (SqlConnection conn = new SqlConnection(dbConnection))
                {
                    conn.Open();
                    SqlDataReader rd = null;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            string sql = "Select IP from Serverinfo.ProtectNodeBackupIP with (nolock)";
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;
                            rd = await cmd.ExecuteReaderAsync();

                            if (rd.Read())
                            {
                                iplist = Regex.Replace(rd["IP"].ToString(), @"\s", string.Empty);
                            }

                            rd.Close();
                        }
                        finally
                        {
                            if (rd != null && !rd.IsClosed)
                                rd.Close();
                        }
                    }
                    conn.Close();
                }
            }


            if (iplist.Length > 0)
            {
                BackupIPList = iplist.Split(',');
            }

            if (BackupIPList != null && BackupIPList.Length > 0)
            {
                //寫入到cache,  24hr後一律過期
                //SetCache(cacheKey, iplist, DateTime.Now.AddHours(24));

                //寫入到cache,  1小時後沒被使用過期
                SetCache(cacheKey, iplist, TimeSpan.FromHours(1));

                return GetRandomAddr(BackupIPList);
            }

            return "";
        }
        private string GetRandomAddr(string[] list)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, list.Length);
            return list[randomNumber].Trim();
        }
        #endregion

        #region 提審服功能

        public async Task<NodeResult> AppReviewAsync(int userid, int areaid, int vip, string gui, string ver)
        {
            List<string> logbag = new List<string>();

            //X-forwarded IP位置
            var r = HttpContext.Current.Request;

            string userIP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(userIP)) userIP = r.ServerVariables["REMOTE_ADDR"];



            //取得使用者IP國家
            string from = FromCountry(userIP);
            //連線字串
            string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;

            //apple審核員
            AppleReviewUser user = null;
            //審核中的AreaID
            int ReviewAreaID = 0;
            //審核階段紀錄的IP
            string ReviewIP = "";

            //紀錄request參數
            pushLog($"Request: userid {userid} areaID {areaid} levelID {vip} HDCode {gui} UserIP {userIP}");

            // 檢查登入者, 如果是送審階段, 紀錄連線使用者(可能是審核員)
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                String strSQL = $"exec [CenterDB].[dbo].[PrTF_UserLogon] {areaid},{userid},'{gui}','{userIP}'";

                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {

                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "Review");

                        //user data
                        DataTable UserTable = ds.Tables[0];

                        if (UserTable.Rows.Count == 1)
                        {
                            user = new AppleReviewUser();
                            user.UserID = Convert.ToInt32(UserTable.Rows[0]["UserID"]);
                            user.HDCode = UserTable.Rows[0]["HDCode"].ToString();
                            user.LastLogonTime = Convert.ToDateTime(UserTable.Rows[0]["LastLogonTime"]);
                            user.LastLogonIPAddress = UserTable.Rows[0]["LastLogonIPAddress"].ToString();
                            user.UserFlag = Convert.ToInt16(UserTable.Rows[0]["UserFlag"]); //0: 美服資格 1:國服資格
                        }

                        //Review AreaID
                        DataTable ReviewAreaTable = ds.Tables[1];
                        if (ReviewAreaTable.Rows.Count == 1)
                            ReviewAreaID = Convert.ToInt32(ReviewAreaTable.Rows[0]["ReviewAreaID"]);

                        //取得曾經紀錄的IP
                        DataTable ReviewIPTable = ds.Tables[2];
                        if (ReviewIPTable.Rows.Count == 1)
                            ReviewIP = ReviewIPTable.Rows[0]["IP"].ToString();


                    }
                    catch (Exception ex)
                    {
                        return new NodeResult() { data = null, message = ex.Message, result = -1 };
                    }

                }
            }

            //create token
            string token = [REDACTED] Token()), SecTokenKey)).Replace("-", "");

            //檢查是否走審核服
            bool goReview = goReviewServer(user, ReviewIP, from);

            if (goReview)
            {

                string Hallserver = "";

                if (ver == "3.1")
                {
                    //UI3.1
                    Hallserver = ReviewHallAddr3_1;
                }
                else if (ver=="3")
                {
                    //UI3.0
                    Hallserver = ReviewHallAddr2;
                }
                else
                {
                    //UI2.0
                    Hallserver = ReviewHallAddr;
                }
                    
                pushLog($"Node {Hallserver}:{TransferPort} giveUserIP {GiveUserIP} from {from} userIP {userIP}");
                var t = WriteLogAsync(_reviewFileLog);

                return new NodeResult() { token = [REDACTED], data = new NodeData() { Addr = Hallserver, From = from, GiveUserIP = GiveUserIP, TransferPort = TransferPort, UserIP = userIP }, message = "", result = 1 };

            }
            else
            {
                string addr;
                //transfer=2, 強制走備援節點
                if (IsTransfer == "2")
                {
                    addr = await GetBackupNodeAddrAsync();
                }
                else
                //走一般節點
                {
                    addr = await GetNodeAsyc(gui, vip, userid, "0", 0, userIP);
                }

                pushLog($"Node {addr}:{TransferPort} giveUserIP {GiveUserIP} from {from} userIP {userIP}");
                var t = WriteLogAsync(_reviewFileLog);

                return new NodeResult() { token = [REDACTED], data = new NodeData() { Addr = addr, From = from, GiveUserIP = GiveUserIP, TransferPort = TransferPort, UserIP = userIP }, message = "", result = 1 };

            }

        }

        private bool goReviewServer(AppleReviewUser user, string reviewIP, string from)
        {
            //1例外者 0審核員
            if (user != null)
            {

                if (user.UserFlag == 1)
                {
                    pushLog("special user");
                    return false;
                }
                else
                {
                    pushLog("match reviewer");
                    return true;
                }
            }

            //吻合審核階段紀錄IP
            if (reviewIP != "")
            {
                pushLog("match reviewer ip");
                return true;
            }

            //非中港澳台一律走美服
            if (from == "CN" || from == "TW" || from == "HK" || from == "LA")
            {
                pushLog("CN TW HK LA user");
                return false;
            }
            else
            {
                pushLog("non CN TW HK LA user");
                return true;
            }

        }

        #endregion


        /// <summary>
        /// log紀錄清單
        /// </summary>
        /// <param name="data"></param>
        public void pushLog(string data)
        {
            LOGBAG.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} {LOGID} {data}{Environment.NewLine}");
        }

        private static object logSyncObject = new object();

        /// <summary>
        /// 寫log
        /// </summary>
        /// <param name="fileLogName"></param>
        /// <returns></returns>
        private async Task WriteLogAsync(string fileLogName)
        {
            if (LogEnable == "1")
            {
                string data = LOGBAG.Aggregate((a, b) => a + b);
                var path = HttpContext.Current.Server.MapPath($"/logcustom/{DateTime.Now.ToString("yyyyMMdd")}/");
                await FileLogAsync(path, data, fileLogName);
            }

        }
        /// <summary>
        /// 寫log
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private async Task FileLogAsync(string filePath, string message, string filename)
        {
            if (!Directory.Exists(filePath))
            {
                lock (logSyncObject)
                {
                    Directory.CreateDirectory(filePath);
                }
            }

            var path = filePath + filename;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            Int32 offset = 0;
            Int32 sizeOfBuffer = 4096;

            using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, bufferSize: sizeOfBuffer, useAsync: true))
            {
                await fileStream.WriteAsync(buffer, offset, buffer.Length); //.ConfigureAwait(false);
            }

        }

        /// <summary>
        /// 檢查 IP 是否合法
        /// </summary>
        private static bool CheckIP(string strPattern)
        {
            Regex regex = new Regex("^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$");
            Match m = regex.Match(strPattern);
            return m.Success;
        }
        /// <summary>
        /// 從IP取得地區
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string FromCountry(string ip)
        {
            IPAddress ipAddress;
            CountryResponse countryResponse;
            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipAddress))
            {
                return "0";
            }

            try
            {
                using (var reader = new DatabaseReader(GeoIPFile(), FileAccessMode.MemoryMapped))
                {
                    if (!reader.TryCountry(ipAddress, out countryResponse))
                    {
                        return "0";
                    }
                    return countryResponse.Country.IsoCode;
                }
            }
            catch (FileNotFoundException e)
            {
                return "-1";
            }

        }

        /// <summary>
        /// 設定快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="absoluteExpiration">時間一到就過期</param>
        private static void SetCache(string CacheKey, object objObject, DateTime absoluteExpiration)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, absoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration);
        }

        /// <summary>
        /// 設定快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="slidingExpiration">最後訪問超過某個時間段後就過</param>
        private static void SetCache(string CacheKey, object objObject, TimeSpan slidingExpiration)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, System.Web.Caching.Cache.NoAbsoluteExpiration, slidingExpiration);
        }
        /// <summary>
        /// 讀取快取
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        private static object GetCache(string CacheKey)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            return objCache[CacheKey];
        }
        /// <summary>
        /// 清除快取
        /// </summary>
        public static void ResetCache()
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Remove(cacheKey);
        }

        /// <summary>
        /// 建立一個TFserver驗證的token
        /// </summary>
        /// <returns></returns>
        private static string GetSecToken()
        {
            //double timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            string token = [REDACTED], 5);
            return Convert.ToBase64String(AESEncryption.AESEncrypt($"{token}:{timeStamp.ToString()}", SecKey));
        }

        //private async Task WriteProtectNodeLogAsync(string token, int userID, int action, string BackupNodeIP, string NodeIP, string userIP, string userFrom, string HDCode)
        //{
        //    using (SqlConnection conn = new SqlConnection(dbConnection))
        //    {
        //        conn.Open();
        //        string sql = "";
        //        using (SqlCommand cmd = new SqlCommand())
        //        {
        //            try
        //            {
        //                sql = $@"
        //                insert into[CenterDB].[Game].[ProtectNodeLog](Token, UserID, Action, BackupNodeIP, NodeIP, UserIP, UserFrom, HDCode) 
        //                values('{token}', {userID}, {action}, '{BackupNodeIP}', '{NodeIP}', '{userIP}', '{userFrom}', '{HDCode}')";

        //                cmd.Connection = conn;
        //                cmd.CommandType = CommandType.Text;
        //                cmd.CommandText = sql;
        //                int r = await cmd.ExecuteNonQueryAsync();    
        //            }
        //            catch (Exception ex)
        //            {
        //                pushLog(sql);
        //                pushLog($"WriteProtectNodeLogAsync Exception error: {ex.Message} StackTrace: {ex.StackTrace}");
        //            }
        //        }
        //        conn.Close();
        //    }
        //}
        private void WriteProtectNodeLog(string token, int userID, int action, string BackupNodeIP, string NodeIP, string userIP, string userFrom, string HDCode, string ErrorMessage)
        {
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                string sql = "";
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        sql = $@"
                        insert into[CenterDB].[Game].[ProtectNodeLog](Token, UserID, Action, BackupNodeIP, NodeIP, UserIP, UserFrom, HDCode, Memo, TFServer) 
                        values('{token}', {userID}, {action}, '{BackupNodeIP}', '{NodeIP}', '{userIP}', '{userFrom}', '{HDCode}', '{ErrorMessage}', '{HostName}')";
                        //pushLog(sql);

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        pushLog(sql);
                        pushLog($"WriteProtectNodeLog Exception error: {ex.Message} StackTrace: {ex.StackTrace}");
                    }
                }
                conn.Close();
            }
        }
    }




    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




    /// <summary>
    /// 蘋果審核員資訊
    /// </summary>
    public class AppleReviewUser
    {
        public int UserID { get; set; }
        public Int16 UserFlag { get; set; }
        public string HDCode { get; set; }
        public DateTime LastLogonTime { get; set; }
        public string LastLogonIPAddress { get; set; }

    }

    public class AESEncryption
    {
        //預設IV值
        private static byte[] _IV = Encoding.ASCII.GetBytes(TransferServices.SecIV);

        /// <summary>  
        /// AES加密算法  
        /// </summary>  
        public static byte[] AESEncrypt(string plainText, string strKey)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);

            aes.Key = Encoding.UTF8.GetBytes(strKey);
            aes.IV = _IV;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] cipherBytes = ms.ToArray();
            cs.Close();
            ms.Close();
            return cipherBytes;
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        public static byte[] AESDecrypt(byte[] cipherText, string strKey)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            aes.Key = Encoding.UTF8.GetBytes(strKey);
            aes.IV = _IV;
            byte[] decryptBytes = new byte[cipherText.Length];
            MemoryStream ms = new MemoryStream(cipherText);
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            cs.Read(decryptBytes, 0, decryptBytes.Length);
            cs.Close();
            ms.Close();
            return decryptBytes;
        }

        public static byte[] HexToByte(string value)
        {
            //運算後的位元組長度:16進位數字字串長/2
            byte[] byteOUT = new byte[value.Length / 2];
            for (int i = 0; i < value.Length; i = i + 2)
            {
                //每2位16進位數字轉換為一個10進位整數
                byteOUT[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }
            return byteOUT;
        }

        public static string ByteToHex(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "");
        }
    }

    public class TransferData
    {
        public int UserID;
        public int VIP;
        public string GUI;
        public string Channel;
        public int Simulation;
        public int Action; // 1正常走节点 2强迫取备用节点
        public int time;
    }

    public class AppReviewData
    {
        public int UserID;
        public int AreaID;
        public int VIP;
        public string GUI;
        public int time;
        public string Ver;
    }

    public class NodeResult
    {
        public int result;
        public string message;
        public string token;
        public NodeData data;
    }

    public class NodeData
    {
        public string Addr;
        public int TransferPort;
        public int GiveUserIP;
        public string From;
        public string UserIP;
    }

    public class Token
    {
        public string value = Guid.NewGuid().ToString().Substring(0, 6);
        //double timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        //int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        public int time = Convert.ToInt32(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }

}