using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading;
using CommonLib;
using System.Web.Script.Serialization;
using System.Web;
using System.Diagnostics;
using example.invalid.Cache;
using System.Threading.Tasks;

namespace FZAlertBot
{
    class Program
    {


        #region 設定參數
        /// <summary>
        /// 資料庫連線
        /// </summary>
        static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }
        /// <summary>
        /// 執行記錄檔
        /// </summary>
        static string fileLogName { get { return "FZalert.log"; } }

        /// <summary>
        /// 預設監測週期 (min)
        /// </summary>
        static int paySuccessRateMonitorInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["paySuccessRateMonitorInterval"])  * 60 * 1000; }
        }

        static int payOrderStatusMonitorInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["payOrderStatusMonitorInterval"]) * 60 * 1000; }
        }
        static int payOrderSilenceTimeLimit
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["payOrderSilenceTimeLimit"]); }
        }
        static int payOrderFailTimeLimit
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["payOrderFailTimeLimit"]); }
        }


        /// <summary>
        /// URL檢測週期 (sec)
        /// </summary>
        //static int HttpMonitorInterval
        //{
        //    get { return Convert.ToInt32(ConfigurationManager.AppSettings["HttpMonitorInterval"]) * 1000; }
        //}
        /// <summary>
        /// 短線檢查週期 (min)
        /// </summary>
        static int smsMonitorInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["smsMonitorInterval"]) * 60 * 1000; }
        }


        /// <summary>
        /// telegram報警token
        /// </summary>
        static string botToken
        {
            get { return ConfigurationManager.AppSettings["bottoken"]; }
        }
        /// <summary>
        /// telegram發送對象id
        /// </summary>
        static string[] chatid
        {
            get { return ConfigurationManager.AppSettings["chatid"].Split(','); }
        }
        /// <summary>
        /// 啟用服務成功率檢測
        /// </summary>

        static bool paySuccessRateCheck
        {
            get { return ConfigurationManager.AppSettings["paySuccessRateCheck"] == "1" ? true : false; }
        }

        /// <summary>
        /// 是否啟用支付狀態檢查
        /// </summary>
        static bool payOrderStatusCheck
        {
            get { return ConfigurationManager.AppSettings["payOrderStatusCheck"] == "1" ? true : false; }
        }

        /// <summary>
        /// 是否啟用每小時充值總額報警
        /// </summary>
        static bool payTotalPerHourCheck
        {
            get { return ConfigurationManager.AppSettings["payTotalPerHourCheck"] == "1" ? true : false; }
        }

        static bool smsSuccessRateCheck
        {
            get { return ConfigurationManager.AppSettings["smsSuccessRateCheck"] == "1" ? true : false; }
        }

        /// <summary>
        /// 啟用短信額度檢測
        /// </summary>
        static bool smsQuotaCheck
        {
            get { return ConfigurationManager.AppSettings["smsQuotaCheck"] == "1" ? true : false; }
        }

        /// <summary>
        /// 兌換輸贏檢查監控是否啟用
        /// </summary>
        static bool convertWinLostCheck
        {
            get { return ConfigurationManager.AppSettings["convertWinLostCheck"] == "1" ? true : false; }
        }


        /// <summary>
        /// 成功率低於%警報
        /// </summary>
        static int paymentAlertLine
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["paymentAlert"]); }
        }

        /// <summary>
        /// 成功率低於%警報
        /// </summary>
        static int smsAlertLine
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["smsAlert"]); }
        }

        /// <summary>
        /// 額度警報
        /// </summary>
        static int SMSQuota
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["smsQuota"]); }
        }

        /// <summary>
        /// 短信 submail是否在啟用中
        /// </summary>
        static bool SubmailEnable { get; set; } = false;

        static string SMSAppid
        {
            get { return ConfigurationManager.AppSettings["smsAppid"]; }
        }

        static string SMSSignature
        {
            get { return ConfigurationManager.AppSettings["smsSignature"]; }
        }

        static int convertWinLostInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["convertWinLostInterval"]) * 1000; }
        }


        static bool userInfoCheck
        {
            get { return ConfigurationManager.AppSettings["userInfoCheck"] == "1" ? true : false; }
        }
        static int userInfoCheckInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["userInfoCheckInterval"]) * 1000; }
        }

        static bool onlineUserCheck
        {
            get { return ConfigurationManager.AppSettings["onlineUserCheck"] == "1" ? true : false; }
        }
        static int onlineUserCheckInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["onlineUserCheckInterval"]) * 1000; }
        }

        #endregion


        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(936);

            //支付成功率監控
            Thread th = new Thread(paySuccessRateCheckJob);
            th.Start();

            //支付状态監控
            Thread thPayStatus = new Thread(payOrderStatusCheckJob);
            thPayStatus.Start();

            //每小時充直總額監控
            Thread thPayTotalPerHour = new Thread(payTotalPerHourCheckJob);
            thPayTotalPerHour.Start();

            //短信額度監控
            Thread thQuota = new Thread(SMSQuotaCheckJob);
            thQuota.Start();

            //短信成功率監控
            Thread thSMS = new Thread(SMSSuccessRateCheckJob);
            thSMS.Start();

            //兌換輸贏警報
            Thread t = new Thread(ConvertWinLostJob);
            t.Start();

            //註冊用戶監控
            Thread regT = new Thread(UserinfoCheckJob);
            regT.Start();


            //各遊戲在線人數統計
            Thread onlineT = new Thread(onlineUserCheckJob);
            onlineT.Start();


        }



        #region 支付成功率排程
        static void paySuccessRateCheckJob()
        {
            while (paySuccessRateCheck)
            {
                try
                {            
                    //calc payment succcess rate
                    calcPaymentSuccessRate();

                }
                catch (Exception e)
                {
                    Common.PrintAndLog(string.Format("paySuccessRateCheckJob exception: {0}", e),fileLogName);
                }
                Thread.Sleep(paySuccessRateMonitorInterval);
            }
        }

        static void calcPaymentSuccessRate()
        {
            string paymentAlertClose = "";
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataReader rd = null;
                    try
                    {
                        string sql = @"
                        Declare @Names varchar(500);
                        select @Names = COALESCE(@Names + ', ', '') + gatewayname   from pay.PayGateway with (nolock) where ID not in (
                            select distinct gwid from pay.PayType with (nolock) where status = 1
                        );
                        select @Names;";
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            paymentAlertClose = rd[0].ToString();
                            Common.PrintAndLog(String.Format("{0} 支付檢查關閉清單:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), paymentAlertClose), fileLogName);
                        }
                        rd.Close();
                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcPaymentSuccessRate exception: {0}", e), fileLogName);
                    }
                    finally
                    {
                        if (rd != null && !rd.IsClosed)
                            rd.Close();
                    }


                    try
                    {
                        int pageCount = 0;
                        int beforeHours = -12;
                        SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId",1),
                                    new SqlParameter("@intPlatformID",null),
                                    new SqlParameter("@intAreaID",null),
                                    new SqlParameter("@dtStatTime",DateTime.Now.AddHours(beforeHours).ToString("yyyy-MM-dd HH:mm")),
                                    new SqlParameter("@dtEndTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm")),
                                    new SqlParameter("@intPayTypeID",null),
                                    new SqlParameter("@intPageIndex",1),
                                    new SqlParameter("@intPageSize",20),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };
                        paras.LastOrDefault().Direction = ParameterDirection.Output;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "PrWeb_Pay_GetOrderStatList";
                        cmd.Parameters.AddRange(paras);


                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "payment");

                        DataTable table = ds.Tables[0];
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string payData = "";
                            for (int r = 0; r < table.Rows.Count; r++)
                            {
                                string typename = Convert.ToString(table.Rows[r]["typename"]);
                                string gatewayname = Convert.ToString(table.Rows[r]["gatewayname"]);
                                int successcount = Convert.ToInt32(table.Rows[r]["successcount"]);
                                int totalcount = Convert.ToInt32(table.Rows[r]["totalcount"]);
                                double successrate = Convert.ToDouble(table.Rows[r]["successrate"]);


                                if (!paymentAlertClose.Contains(gatewayname))
                                {
                                    Common.PrintAndLog(string.Format("{3} {0}{1} 成功率{2}% 成功笔数{4} 总笔数{5}", gatewayname, typename, successrate, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), successcount, totalcount), fileLogName);

                                    if (successcount != 0 && totalcount != 0)
                                    {
                                        payData += $"{gatewayname}{typename} 成功率{successrate}% 成功笔数{successcount} 总笔数{totalcount}" + Environment.NewLine;
                                    }
                                }

                                if (totalcount > 0 && successrate < paymentAlertLine && !paymentAlertClose.Contains(gatewayname))
                                {
                                    string data = string.Format("{3} {0}{1} 成功率过低 {2}%",
                                          gatewayname, typename, successrate, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    Common.PrintAndLog(data, fileLogName);
                                    Common.pushMessage(botToken, chatid, data);
                                }

                            }

                            if (payData.Length > 0)
                                Common.pushMessage(botToken, chatid, payData);
                        }



                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcPaymentSuccessRate exception: {0}", e), fileLogName);
                    }
                    finally { }
                }

            }


        }

        #endregion

        #region 支付笔订单超時提醒
        static void payOrderStatusCheckJob()
        {
            while (payOrderStatusCheck)
            {
                try
                {
                    //calc payment status
                    calcPaymentOrderStatus();

                }
                catch (Exception e)
                {
                    Common.PrintAndLog(string.Format("payOrderStatusCheckJob exception: {0}", e), fileLogName);
                }
                Thread.Sleep(payOrderStatusMonitorInterval);
            }
        }

        //最後報警時間
        static DateTime lastPaymentOrderStatusAlertTime = DateTime.Now.AddHours(-1);
        static void calcPaymentOrderStatus()
        {

            int failTimeLimit = payOrderFailTimeLimit; //最後一筆訂單超過多少時間沒有完成充值(min)
            int silenceTimeLimit = payOrderSilenceTimeLimit; //超過多少時間無任何訂單(min)
            int alertAgainTime = 15; //預期多久後再發警報(min)
            int lastAlertTimeDiff = (int)(DateTime.Now - lastPaymentOrderStatusAlertTime).TotalMinutes; //距離上次警報經過多久時間(min)

            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataReader rd = null;
                    try
                    {
                        string sql = $@"
						select top 1 a.*,C.GatewayName, DATEDIFF(MINUTE,CreateTime,getdate()) as DiffTime --,b.TypeName 
						from pay.OrderList a with(nolock)
						--left join pay.PayType b with(nolock) on b.TypeID=a.PayTypeID and b.GWID=a.GWID
						left join pay.PayGateway c with(nolock) on c.ID = a.GWID
                        where CreateTime >= DATEADD(MINUTE,{-silenceTimeLimit},getdate())
						order by a.id desc";


                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            int id = Convert.ToInt32(rd["id"].ToString());
                            int status = Convert.ToInt32(rd["status"].ToString());
                            DateTime dtCreateTime = Convert.ToDateTime(rd["CreateTime"].ToString());
                            int diffMinTime = Convert.ToInt32(rd["DiffTime"].ToString());
                            Common.PrintAndLog(String.Format("{0} 支付 最後一筆訂單 {3} 建立時間{1} 狀態 {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dtCreateTime, status, id), fileLogName);

                            if (status == 1 && diffMinTime >= failTimeLimit)
                            {
                                string data = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 支付提醒 最后一笔订单超过{failTimeLimit}分钟没有完成充值";
                                Common.PrintAndLog(data, fileLogName);

                                if (lastAlertTimeDiff >= alertAgainTime)
                                {
                                    Common.pushMessage(botToken, chatid, data);
                                    lastPaymentOrderStatusAlertTime = DateTime.Now;
                                }
                            }

                        }
                        else
                        {
                            string data = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 支付提醒 超过{silenceTimeLimit}分钟没有任何订单建立";
                            Common.PrintAndLog(data, fileLogName);

                            if (lastAlertTimeDiff >= alertAgainTime)
                            {
                                Common.pushMessage(botToken, chatid, data);
                                lastPaymentOrderStatusAlertTime = DateTime.Now;
                            }

                        }
                        rd.Close();
                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcPaymentOrderStatus exception: {0}", e), fileLogName);
                    }
                    finally
                    {
                        if (rd != null && !rd.IsClosed)
                            rd.Close();
                    }

                }

            }
        }

        #endregion

        #region 最近7天充值总额排程
        static void payTotalPerHourCheckJob()
        {

            string data = "";
            DateTime now;
            int h = 0;
            int m = 0;
            int s = 0;

            while (payTotalPerHourCheck)
            {

                now = DateTime.Now;
                h = now.Hour;
                m = now.Minute;
                s = now.Second;
                try
                {
                    if (m == 1 && s == 0)//每小時
                    {
                        calcPaymentTotalAmountPerHour();
                    }
                }
                catch (Exception e)
                {
                    Common.PrintAndLog(string.Format("payTotalPerHourCheckJob exception: {0}", e), fileLogName);
                }
                Thread.Sleep(1000); 
            }
        }

        static void calcPaymentTotalAmountPerHour()
        {

            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataReader rd = null;
                    try
                    {
                        string sql = $@"
                        declare @intFromYYYYMMDD int = Cast(Convert(varchar, DateAdd(DAY,-7,getdate()), 112) as int)
                        declare @intToYYYYMMDD int = Cast(Convert(varchar,getdate(), 112) as int)
                        declare @intHour int = Datepart(hour, getdate())

                        print @intFromYYYYMMDD
                        print @intToYYYYMMDD
                        print @intHour

                        select * from stat.ChargePerHour with(nolock) WHERE YYYYMMDD >= @intFromYYYYMMDD and YYYYMMDD <=@intToYYYYMMDD and HH=@intHour order by YYYYMMDD desc";


                        string alertdata = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 最近7天充值总额监控" + Environment.NewLine;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            int HH = Convert.ToInt32(rd["HH"].ToString());
                            int MM = Convert.ToInt32(rd["MM"].ToString());
                            int DD = Convert.ToInt32(rd["DD"].ToString());
                            int TotalAmount = Convert.ToInt32(rd["TotalAmount"].ToString());
                            int HourAmount = Convert.ToInt32(rd["HourAmount"].ToString());

                            alertdata += $"今日此刻充值总额:{TotalAmount}(1小时充值:{HourAmount})" + Environment.NewLine;
                        }

                        while (rd.Read())
                        {
                            int HH = Convert.ToInt32(rd["HH"].ToString());
                            int MM = Convert.ToInt32(rd["MM"].ToString());
                            int DD = Convert.ToInt32(rd["DD"].ToString());
                            int TotalAmount = Convert.ToInt32(rd["TotalAmount"].ToString());
                            int HourAmount = Convert.ToInt32(rd["HourAmount"].ToString());

                            alertdata += $"{MM}/{DD}此刻充值总额:{TotalAmount}(1小时充值:{HourAmount})" + Environment.NewLine;
                        }

                        Common.PrintAndLog(alertdata, fileLogName);
                        Common.pushMessage(botToken, chatid, alertdata);

                        rd.Close();
                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcPaymentTotalAmountPerHour exception: {0}", e), fileLogName);
                    }
                    finally
                    {
                        if (rd != null && !rd.IsClosed)
                            rd.Close();
                    }

                }
            }
        }
        #endregion

        #region 短信額度檢查監控
        static void SMSQuotaCheckJob()
        {

            //submail
            string submailAPI = "https://example.invalid/balance/sms";
            string param = string.Format("signature={0}&appid={1}", SMSSignature, SMSAppid);
            string responseData = "";
            while (smsQuotaCheck)
            {
                try
                {
                    //取得submail是否有啟用
                    GetSubmailStatus();

                    int statusCode = Common.HttpPost(submailAPI, param, ref responseData);
                    if (statusCode == 200)
                    {
                        JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                        dynamic item = json_serializer.Deserialize<object>(responseData);
                        int quota = Convert.ToInt32(item["transactional_balance"]);
                        string printstr = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Submail 短信额度 {quota} 是否启用 {SubmailEnable}";
                        Common.PrintAndLog(printstr,fileLogName);

                        if (quota < SMSQuota && SubmailEnable)
                        {
                            string pushmsg = $"Submail 短信额度即将用尽, 剩余 {quota}";
                            Common.pushMessage(botToken, chatid, pushmsg);   
                        }
                    }
                }
                catch (Exception e)
                {
                    Common.PrintAndLog($"SMSQuotaCheckJob exception: {e}",fileLogName);
                }

                Thread.Sleep(smsMonitorInterval);
            }

        }

        //短信狀態監控
        static void GetSubmailStatus()
        {

            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = String.Format(@"SELECT status FROM [CenterDB].[ServerInfo].[SMSConfig] where vender = 'submail'");

                using (SqlCommand cmdReg = new SqlCommand(sql, conn))
                {

                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmdReg;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "sms");

                        DataTable table0 = ds.Tables[0];

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table0.Rows.Count; r++)
                            {
                                SubmailEnable = Convert.ToInt32(table0.Rows[r]["status"]) == 1 ? true : false;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog($"GetSubmailStatus exception: {e}", fileLogName);
                        SubmailEnable = false;
                    }

                }
            }

        }
        #endregion

        #region 短信成功率監控
        static void SMSSuccessRateCheckJob()
        {

            while (smsSuccessRateCheck)
            {
                try
                {
                    //calc sms success rate
                    calcSMSSuccessRate();
                }
                catch (Exception e)
                {
                    Common.PrintAndLog($"SMSSuccessRateCheckJob exception: {e}", fileLogName);
                }
                Thread.Sleep(smsMonitorInterval);
            }
        }

        //前一次的短信成功率
        static double prevSMSSuccessRate = 1000;
        //计算短信成功率
        static void calcSMSSuccessRate()
        {

            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                //成功率計算
                String sqlTotalCharge = String.Format(@"
				SELECT
                ISNULL(SUM(CASE when Status = 0 THEN 1 ELSE 0 END),0) as fail,
                ISNULL(SUM(CASE when Status <> 0 THEN 1 ELSE 0 END),0) as success,
                ISNULL(
                CONVERT(FLOAT,ROUND(
                SUM(CASE when Status <> 0 THEN 1 ELSE 0 END) / (COUNT(CodeID) * 1.00) * 100
                ,2)),0) as successrate
                FROM [Notify].[Notify].[SMSVCode] with (nolock)  where  CreateTime > dateadd(MINUTE,-10,GETDATE())");
                using (SqlCommand cmdReg = new SqlCommand(sqlTotalCharge, conn))
                {

                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmdReg;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "sms");

                        DataTable table0 = ds.Tables[0];

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table0.Rows.Count; r++)
                            {
                                int success = Convert.ToInt32(table0.Rows[r]["success"]);
                                int fail = Convert.ToInt32(table0.Rows[r]["fail"]);
                                double successrate = Convert.ToDouble(table0.Rows[r]["successrate"]);

                                if (prevSMSSuccessRate == 1000)
                                    prevSMSSuccessRate = successrate;


                                Common.PrintAndLog(string.Format("{3} 短信服務成功率 {0}% 成功{1}笔 失败{2}笔", successrate, success, fail, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),fileLogName);

                                if (success == 0 || fail == 0)
                                    break;

                                if (successrate < smsAlertLine)
                                {
                                    var msg = "";
                                    if (successrate > prevSMSSuccessRate) msg = "短信服務成功率恢复";
                                    else
                                        if (successrate < prevSMSSuccessRate) msg = "短信服務成功率下滑";
                                    else
                                        msg = "短信服務成功率过低";

                                    string data = $"{msg} {successrate}% 成功{success}笔 失败{fail}笔";
                                    Common.pushMessage(botToken, chatid, data);
                                }

                                prevSMSSuccessRate = successrate;

                            }

                        }

                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcSMSSuccessRate exception: {0}", e),fileLogName);
                    }

                }
            }
        }

        #endregion

        #region 兌換輸贏警報
        static void ConvertWinLostJob()
        {
            int feq = 5;
            while (convertWinLostCheck)
            {
                try
                {
                    //取得發送設定
                    feq++;
                    if (feq >= 5) { GetConvertWinLostAlertConfig(); feq = 0; }
                    //取得發送數據
                    ConvertWinLostJobProcess();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Common.FileLog(e.Message, fileLogName);
                }

                Thread.Sleep(convertWinLostInterval);
            }

        }
        static void ConvertWinLostJobProcess()
        {

            var data = GetConvertWinLostAlert();
            if (data != null)
            {
                string userid = data.Item1;
                string username = data.Item2;
                string winlost = data.Item3;
                string multiple = data.Item4;

                string msg = $"请风控注意\r\nID：{userid}\r\n玩家：{username}\r\n总输赢：{winlost}\r\n输赢倍数：{multiple}";
                
                Common.PrintAndLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 兑换输赢警报 {msg}", fileLogName);
                Common.pushMessage(TgBotToken, new string[] { TgChatID }, msg);
            }
        }

        static int ConvertWinLostAlert;
        static string TgBotToken="0";
        static string TgChatID="0";
        static void GetConvertWinLostAlertConfig()
        {
            string oldTgBotToken;
            string oldTgChatID;
            try
            {
                oldTgBotToken = [REDACTED];
                oldTgChatID = TgChatID;

                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    String sql = $@"select ConvertWinLostAlert/100 as ConvertWinLostAlert,TgBotToken,TgChatID from CenterDB.ServerInfo.SystemConfig";

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        SqlDataReader rd = null;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            ConvertWinLostAlert = Convert.ToInt32(rd["ConvertWinLostAlert"].ToString());
                            TgBotToken = [REDACTED]"TgBotToken"].ToString();
                            TgChatID = rd["TgChatID"].ToString();
                        }
                        rd.Close();
                
                    }
                }

                //如果誤填bot開頭, 則去除
                if (TgBotToken.ToLower().StartsWith("bot"))
                    TgBotToken = [REDACTED], 3);


                if (TgBotToken != oldTgBotToken || TgChatID!= oldTgChatID)
                    Common.PrintAndLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 兑换输赢警报设定 TgToken: [REDACTED] TgChatID: {TgChatID}", fileLogName);

            }
            catch (Exception ex)
            {
                Common.PrintAndLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 兑换输赢警报设定 exception: {ex.Message}", fileLogName);
            }


        }

        static Tuple<string, string, string, string> GetConvertWinLostAlert()
        {
            string UserID = null;
            string UserName = null;
            string WinLost = null;
            string Multiple = null;

            try
            {
                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();


                    using (SqlCommand cmd = new SqlCommand())
                    {
                        SqlDataReader rd = null;
                        try
                        {
                            string sql = $@"
                                declare @intID int
                                declare @intUserID int
                                declare @chvUserName varchar(50)
                                declare @intWinLost bigint
								declare @fltMultiple float

                                select top 1 @intID=Id, @intUserID=UserID, @chvUserName=UserName, @intWinLost=WinLost, @fltMultiple=Multiple from Notify.Notify.ConvertAlert where Status=1 
                                IF @intID > 0
                                Begin
	                                update  Notify.Notify.ConvertAlert set status=0 where Id=@intID
	                                select @intUserID as UserID, @chvUserName as UserName, @intWinLost/100 as WinLost, @fltMultiple as Multiple
                                End";


                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;
                            rd = cmd.ExecuteReader();
                            if (rd.Read())
                            {
                                UserID = rd["UserID"].ToString();
                                UserName = rd["UserName"].ToString();
                                WinLost = rd["WinLost"].ToString();
                                Multiple = rd["Multiple"].ToString();
                            }

                            rd.Close();
                        }
                        catch (Exception ex)
                        {
                            Common.PrintAndLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 兑换输赢警报 sql exception: {ex.Message}", fileLogName);
                        }
                        finally
                        {
                            if (rd != null && !rd.IsClosed)
                                rd.Close();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(UserID))
                    return new Tuple<string, string, string, string>(UserID, UserName, WinLost, Multiple);
                else
                    return null;

            }
            catch (Exception ex)
            {
                Common.PrintAndLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 兑换输赢警报 exception: {ex.Message}", fileLogName);
                return null;
            }
        }
        #endregion

        #region 每小時新增注册用户数,新增付费用户数,新增用户兑换数,兑换总额
        static void UserinfoCheckJob()
        {

            while (userInfoCheck)
            {
                try
                {
                    calcUserinfo();
                }
                catch (Exception e)
                {
                    Common.PrintAndLog(string.Format("UserinfoCheckJob exception: {0}", e), fileLogName);
                }
                Thread.Sleep(userInfoCheckInterval); //per hour
            }
        }
        static void calcUserinfo() {

            try
            {
                
                string dt = DateTime.Now.ToString("yyyy-MM-dd");
                string data = "";

                //如果job是早上1點前驅動, 计算前一日
                if (DateTime.Now.Hour == 0)
                {
                    dt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }


                using (SqlConnection conn = new SqlConnection(dbConnection))
                {
                    conn.Open();

                    string FilterArea = "";
                    //排除聯運
                    FilterArea += " and areaid not in (select areaid from  centerdb.area.area with (nolock) where PlatformID in (select PlatformID from  centerdb.Area.UnionPlatform)	)";

                    //註冊付費人數
                    String sqlReg =
                        $@"
                        declare @intTotalConvertUser int
                        declare @intSumConvertAmount bigint
                        declare @intTotalChargeAmount bigint
                        declare @intTotalConvertAmount	bigint

                        select @intTotalConvertUser=isnull(count(distinct userid),0), @intSumConvertAmount=isnull(sum(UpdateAmount/100),0) from [LogDB].[Money].[MoneyLog] with (nolock)
                        where sourcetypeid=14000
                        and userid in (select userid from centerdb.users.UserInfo with (nolock) where CreateTime>='{dt}')
                        {FilterArea}

                        SELECT @intTotalChargeAmount = ISNULL(SUM(UpdateAmount),0)
                        FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
                        WHERE A.[CreateTime] >= '{dt}' AND [SourceTypeID] >= 11001 AND [SourceTypeID] < 11099

                        SELECT @intTotalConvertAmount = ISNULL(SUM(ABS(amount)) ,0)
                        FROM logdb.Money.ConvertLog  with (nolock) 
                        WHERE LastUpdateTime >= '{dt}' and Status=3
        
                        select
                        @intTotalConvertUser as TotalConvertUser,
                        @intSumConvertAmount as SumConvertAmount,
                        @intTotalChargeAmount as  TotalChargeAmount,
                        @intTotalConvertAmount as TotalConvertAmount,
                        COUNT(userid) as RegUser,
                        ISNULL(SUM(SumCharge / 100), 0) as SumCharge,
                        ISNULL(SUM(sumAgentCharge / 100), 0) as SumAgentCharge, 
                        ISNULL(SUM(CASE WHEN sumcharge > 0 THEN 1 ELSE 0 END), 0) as ChargeUser,
                        ISNULL(SUM(CASE WHEN sumAgentCharge > 0 THEN 1 ELSE 0 END), 0) as AgentChargeUser
                        from users.UserInfo with (nolock) 
                        where CreateTime >= '{dt}' 
                        {FilterArea}
                        ";

                    using (SqlCommand cmdReg = new SqlCommand(sqlReg, conn))
                    {

                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdReg;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "RegUser");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {
                                    int TotalConvertUser = Convert.ToInt32(table0.Rows[r]["TotalConvertUser"]);
                                    int SumConvertAmount = Convert.ToInt32(table0.Rows[r]["SumConvertAmount"]);
                                    int RegUser = Convert.ToInt32(table0.Rows[r]["RegUser"]);
                                    int SumCharge = Convert.ToInt32(table0.Rows[r]["SumCharge"]); //第三方充值
                                    int SumAgentCharge = Convert.ToInt32(table0.Rows[r]["SumAgentCharge"]); //代理充值
                                    int ChargeUser = Convert.ToInt32(table0.Rows[r]["ChargeUser"]); //第三方充值
                                    int AgentChargeUser = Convert.ToInt32(table0.Rows[r]["AgentChargeUser"]); //代理充值
                                    long TotalChargeAmount = Convert.ToInt64(table0.Rows[r]["TotalChargeAmount"]);      //充值金額
                                    long TotalConvertAmount = Convert.ToInt64(table0.Rows[r]["TotalConvertAmount"]);    //兌換金額

                                    data += $"新增注册用户数： {RegUser.ToString()}" + Environment.NewLine;
                                    data += $"新增付费用户数： {(ChargeUser + AgentChargeUser).ToString()}" + Environment.NewLine;
                                    data += $"新增付费总额： {(SumCharge + SumAgentCharge).ToString()}" + Environment.NewLine;
                                    data += $"新增付费率： {( ((double)ChargeUser + (double)AgentChargeUser) / RegUser * 100 ).ToString("0.00")}%" + Environment.NewLine;

                                    data += $"新增兑换用户数： {TotalConvertUser.ToString()}" + Environment.NewLine;
                                    data += $"新增兑换总额： {SumConvertAmount.ToString()}" + Environment.NewLine;
                                    data += $"新增兑换率： { (((double)Math.Abs(SumConvertAmount)) / ((double)SumCharge + (double)SumAgentCharge) * 100).ToString("0.00")}% " + Environment.NewLine;
                                    data += $"兑换率： { (((double)Math.Abs(TotalConvertAmount)) / (double)TotalChargeAmount * 100).ToString("0.00")}% " + Environment.NewLine;

                                }
                                data += Environment.NewLine;
                            }

                            Common.PrintAndLog(data, fileLogName);
                            Common.pushMessage(botToken, chatid, data);

                        }
                        catch (Exception e)
                        {
                            Common.PrintAndLog(string.Format("calcUserinfo exception: {0}", e), fileLogName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.PrintAndLog(string.Format("calcUserinfo exception: {0}", e), fileLogName);
            }

        }
        #endregion 


        #region 游戏在线人数
        static void onlineUserCheckJob()
        {

            while (onlineUserCheck)
            {
                try
                {
                    calcOnlineUser();
                }
                catch (Exception e)
                {
                    Common.PrintAndLog(string.Format("onlineUserCheckJob exception: {0}", e), fileLogName);
                }
                Thread.Sleep(onlineUserCheckInterval); //30 mins
            }
        }

        static void calcOnlineUser()
        {

            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                
                String sqlTotalOnline = String.Format(@"
                select 
                Count(distinct A.UserID) as UserAmount, 
                A.GameName 
                from (
	                select U.UserID,S.GameID, G.GameName from Game.GameUserList U  with(nolock)
	                left join ServerInfo.GameServerList S with(nolock) on U.ServerID=S.ServerID
	                left join ServerInfo.GameList G  with(nolock) on S.GameID = G.GameID
	                where U.UserID not in (select UserID from Users.UserInfo with(nolock) where UserType=2) --exclude robot
                ) as A
                group by A.GameName
                ");
                using (SqlCommand cmd = new SqlCommand(sqlTotalOnline, conn))
                {

                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "online");

                        DataTable table0 = ds.Tables[0];
                        string data = $"游戏在线人数：{Environment.NewLine}";
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table0.Rows.Count; r++)
                            {
                                string GameName = table0.Rows[r]["GameName"].ToString();
                                string UserAmount = table0.Rows[r]["UserAmount"].ToString();
                                data += $"{GameName}：{UserAmount}{Environment.NewLine}";
                            }
                            Common.PrintAndLog(data, fileLogName);
                            Common.pushMessage(botToken, chatid, data);
                        }

                    }
                    catch (Exception e)
                    {
                        Common.PrintAndLog(string.Format("calcOnlineUser exception: {0}", e), fileLogName);
                    }

                }
            }
        }

        #endregion


    }

}
