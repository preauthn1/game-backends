using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using CommonLib;

namespace FZStatReportBot
{
    
    class Program
    {
        static string botToken
        {
            get { return ConfigurationManager.AppSettings["bottoken"]; }
        }

        static string[] chatid
        {
            get { return ConfigurationManager.AppSettings["chatid"].Split(','); }
        }

        static string filelog
        {
            get { return "FZStatReport.log"; }
        }

        static bool isTest = false;
        static void Main(string[] args)
        {

            if (args.Length > 0 && args[0] == "TEST")
                isTest = true;

            Thread th = new Thread(job);
            th.Start();

            //job();
        }

        static void job()
        {
            string data = "";
            DateTime now;
            int h = 0;
            int m = 0;
            int s = 0;




            //TEST
            if (isTest)
            {
                data = getReport();
                Console.WriteLine(data);

                if (data.Length > 0)
                    Common.pushMessage(botToken, chatid, data);

                return;
            }
            /////////////////////




            bool run = true;
            while (run)
            {
                now = DateTime.Now;
                h = now.Hour;
                m = now.Minute;
                s = now.Second;
                Console.WriteLine(string.Format("h {0} m {1} s {2}",h,m,s));

                try
                {
                    if (h == 1 && m == 0 && s == 0)
                    {
                        run = false;
                        Console.WriteLine("run getReport");
                        data = getReport();
                        Console.WriteLine(data);

                        if (data.Length > 0)
                        {
                            Console.WriteLine("push message");
                            Common.pushMessage(botToken, chatid, data);
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    run = false;
                }

                Thread.Sleep(1000);
            }

        }

        static string getReport()
        {
            try
            {
                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
                string dt = ConfigurationManager.AppSettings["reportDate"].Length > 0 ? ConfigurationManager.AppSettings["reportDate"] : DateTime.Now.ToString("yyyy-MM-dd");
                string data = "";
                string dataGame = "";
                //string dataOther = "";
                string dataAgent = "";
                string lastdata = "";

                data += "项目：超級大贏家" + Environment.NewLine + Environment.NewLine;
                data += Convert.ToDateTime(dt).AddDays(-1).ToString("yyyy-MM-dd") + Environment.NewLine + Environment.NewLine;
                data += "昨日与前日对比" + Environment.NewLine + Environment.NewLine;




                string SuccessRate="0";
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    
                    String sqlTotalCharge = $@"

                    declare @dtFrom datetime =  Cast(Convert(varchar, DateADD(DAY,-1,getdate()) , 110) as datetime)
                    declare @dtTo datetime =  Cast(Convert(varchar, getdate(), 110) as datetime)

                    SELECT ISNULL(
                    CONVERT( FLOAT, ROUND(SUM(CASE when Status = (2) THEN 1 ELSE 0 END) / (COUNT(ID) * 1.00) * 100,2) ) 
                    ,0)
                    as successrate
                    FROM pay.OrderList with (nolock) where  CreateTime >= @dtFrom and CreateTime < @dtTo";

                    Console.WriteLine(sqlTotalCharge);
                    using (SqlCommand cmdChargeSuccess = new SqlCommand(sqlTotalCharge, conn))
                    {

                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdChargeSuccess;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "chargeSuccessRate");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {
                                    SuccessRate = (table0.Rows[r]["SuccessRate"].ToString());
                                    Console.WriteLine("successrate:" + SuccessRate);
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " step 1 exception: " + ex.Message);
                            Common.FileLog(ex.Message + Environment.NewLine + ex.StackTrace, filelog);
                        }
                    }






                    String strSQL = String.Format("exec [CenterDB].[dbo].[PrWeb_Stat_GetDailyReport] '{0}'", dt);
                    Console.WriteLine(strSQL);
                    
                    using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                    {
                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmd;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "report");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                data += "-------充值-------" + Environment.NewLine;

                                int TodayCharge = Convert.ToInt32(table0.Rows[0]["TodayCharge"]);
                                int DiffCharge = Convert.ToInt32(table0.Rows[0]["DiffCharge"]);
                                data += "充值总数：" + TodayCharge.formatN() + "   " + DiffCharge.formatN() + getArrow(DiffCharge) + Environment.NewLine;
                                
                                int TodayNewCharge = Convert.ToInt32(table0.Rows[0]["TodayNewCharge"]);
                                int DiffNewCharge = Convert.ToInt32(table0.Rows[0]["DiffNewCharge"]);
                                data += "新用户充值：" + TodayNewCharge.formatN() + "   " + DiffNewCharge.formatN() + getArrow(DiffNewCharge) + Environment.NewLine;

                                int TodayOldCharge = Convert.ToInt32(table0.Rows[0]["TodayOldCharge"]);
                                int DiffOldCharge = Convert.ToInt32(table0.Rows[0]["DiffOldCharge"]);
                                data += "老用户充值：" + TodayOldCharge.formatN() + "   " + DiffOldCharge.formatN() + getArrow(DiffOldCharge) + Environment.NewLine;

                                //int TodayInAppCharge = Convert.ToInt32(table0.Rows[0]["TodayInAppCharge"]);
                                //int DiffInAppCharge = Convert.ToInt32(table0.Rows[0]["DiffInAppCharge"]);
                                //data += "在线充值：" + TodayInAppCharge.formatN() + "   " + DiffInAppCharge.formatN() + getArrow(DiffInAppCharge) + Environment.NewLine;

                                //double TodayNewInAppChargeAmount = Convert.ToDouble(table0.Rows[0]["TodayNewInAppChargeAmount"]);
                                //double DiffNewInAppChargeAmount = Convert.ToDouble(table0.Rows[0]["DiffNewInAppChargeAmount"]);
                                //data += "新用户在线充值：" + TodayNewInAppChargeAmount.formatN() + "   " + DiffNewInAppChargeAmount.formatN() + getArrow(DiffNewInAppChargeAmount) + Environment.NewLine;

                                //double TodayOldInAppChargeAmount = Convert.ToDouble(table0.Rows[0]["TodayOldInAppChargeAmount"]);
                                //double DiffOldInAppChargeAmount = Convert.ToDouble(table0.Rows[0]["DiffOldInAppChargeAmount"]);
                                //data += "老用户在线充值：" + TodayOldInAppChargeAmount.formatN() + "   " + DiffOldInAppChargeAmount.formatN() + getArrow(DiffOldInAppChargeAmount) + Environment.NewLine;

                                double TodayAgentChargeAmount = Convert.ToDouble(table0.Rows[0]["TodayAgentChargeAmount"]);
                                double DiffAgentChargeAmount = Convert.ToDouble(table0.Rows[0]["DiffAgentChargeAmount"]);
                                data += "代理充值：" + TodayAgentChargeAmount.formatN() + "   " + DiffAgentChargeAmount.formatN() + getArrow(DiffAgentChargeAmount) + Environment.NewLine;


                                data += "充值成功率：" + SuccessRate + "%" + Environment.NewLine;

                                //double TodayNewAgentChargeAmount = Convert.ToDouble(table0.Rows[0]["TodayNewAgentChargeAmount"]);
                                //double DiffNewAgentChargeAmount = Convert.ToDouble(table0.Rows[0]["DiffNewAgentChargeAmount"]);
                                //data += "新用户代理充值：" + TodayNewAgentChargeAmount.formatN() + "   " + DiffNewAgentChargeAmount.formatN() + getArrow(DiffNewAgentChargeAmount) + Environment.NewLine;


                                //int TodayOldAgentChargeAmount = Convert.ToInt32(table0.Rows[0]["TodayOldAgentChargeAmount"]);
                                //int DiffOldAgentChargeAmount = Convert.ToInt32(table0.Rows[0]["DiffOldAgentChargeAmount"]);
                                //data += "老用户代理充值：" + TodayOldAgentChargeAmount.formatN() + "   " + DiffOldAgentChargeAmount.formatN() + getArrow(DiffOldAgentChargeAmount) + Environment.NewLine;

                                //int TodayChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayChargeUserCount"]);
                                //int DiffChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffChargeUserCount"]);
                                //data += "充值人数：" + TodayChargeUserCount.formatN() + "   " + DiffChargeUserCount.formatN() + getArrow(DiffChargeUserCount) + Environment.NewLine;


                                //int TodayNewChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayNewChargeUserCount"]);
                                //int DiffNewChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffNewChargeUserCount"]);
                                //data += "新用户充值人数：" + TodayNewChargeUserCount.formatN() + "   " + DiffNewChargeUserCount.formatN() + getArrow(DiffNewChargeUserCount) + Environment.NewLine;

                                //int TodayOldChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayOldChargeUserCount"]);
                                //int DiffOldChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffOldChargeUserCount"]);
                                //data += "老用户充值人数：" + TodayOldChargeUserCount.formatN() + "   " + DiffOldChargeUserCount.formatN() + getArrow(DiffOldChargeUserCount) + Environment.NewLine;

                                //int TodayInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayInAppChargeUserCount"]);
                                //int DiffInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffInAppChargeUserCount"]);
                                //data += "在线充值人数：" + TodayInAppChargeUserCount.formatN() + "   " + DiffInAppChargeUserCount.formatN() + getArrow(DiffInAppChargeUserCount) + Environment.NewLine;

                                //int TodayNewInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayNewInAppChargeUserCount"]);
                                //int DiffNewInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffNewInAppChargeUserCount"]);
                                //data += "新用户在线充值人数：" + TodayNewInAppChargeUserCount.formatN() + "   " + DiffNewInAppChargeUserCount.formatN() + getArrow(DiffNewInAppChargeUserCount) + Environment.NewLine;


                                //int TodayOldInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayOldInAppChargeUserCount"]);
                                //int DiffOldInAppChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffOldInAppChargeUserCount"]);
                                //data += "老用户在线充值人数：" + TodayOldInAppChargeUserCount.formatN() + "   " + DiffOldInAppChargeUserCount.formatN() + getArrow(DiffOldInAppChargeUserCount) + Environment.NewLine;


                                //int TodayAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayAgentChargeUserCount"]);
                                //int DiffAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffAgentChargeUserCount"]);
                                //data += "代理充值人数：" + TodayAgentChargeUserCount.formatN() + "   " + DiffAgentChargeUserCount.formatN() + getArrow(DiffAgentChargeUserCount) + Environment.NewLine;


                                //int TodayNewAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayNewAgentChargeUserCount"]);
                                //int DiffNewAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffNewAgentChargeUserCount"]);
                                //data += "新用户代理充值人数：" + TodayNewAgentChargeUserCount.formatN() + "   " + DiffNewAgentChargeUserCount.formatN() + getArrow(DiffNewAgentChargeUserCount) + Environment.NewLine;

                                //int TodayOldAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["TodayOldAgentChargeUserCount"]);
                                //int DiffOldAgentChargeUserCount = Convert.ToInt32(table0.Rows[0]["DiffOldAgentChargeUserCount"]);
                                //data += "老用户代理充值人数：" + TodayOldAgentChargeUserCount.formatN() + "   " + DiffOldAgentChargeUserCount.formatN() + getArrow(DiffOldAgentChargeUserCount) + Environment.NewLine;

                                //int TodayChargeTimes = Convert.ToInt32(table0.Rows[0]["TodayChargeTimes"]);
                                //int DiffChargeTimes = Convert.ToInt32(table0.Rows[0]["DiffChargeTimes"]);
                                //data += "充值笔数：" + TodayChargeTimes.formatN() + "   " + DiffChargeTimes.formatN() + getArrow(DiffChargeTimes) + Environment.NewLine;

                                //int TodayInAppChargeTimes = Convert.ToInt32(table0.Rows[0]["TodayInAppChargeTimes"]);
                                //int DiffInAppChargeTimes = Convert.ToInt32(table0.Rows[0]["DiffInAppChargeTimes"]);
                                //data += "在线充值笔数：" + TodayInAppChargeTimes.formatN() + "   " + DiffInAppChargeTimes.formatN() + getArrow(DiffInAppChargeTimes) + Environment.NewLine;

                                //int TodayAgentChargeTimes = Convert.ToInt32(table0.Rows[0]["TodayAgentChargeTimes"]);
                                //int DiffAgentChargeTimes = Convert.ToInt32(table0.Rows[0]["DiffAgentChargeTimes"]);
                                //data += "代理充值笔数：" + TodayAgentChargeTimes.formatN() + "   " + DiffAgentChargeTimes.formatN() + getArrow(DiffAgentChargeTimes) + Environment.NewLine;

                                //double TodayARPPU = Convert.ToDouble(table0.Rows[0]["TodayARPPU"]);
                                //double DiffARPPU = Convert.ToDouble(table0.Rows[0]["DiffARPPU"]);
                                //data += "平均每付费用户收入ARPPU（充值总数/付费人数）：" + TodayARPPU.formatN() + "   " + DiffARPPU.formatN() + getArrow(DiffARPPU) + Environment.NewLine;

                                //double TodayARPU = Convert.ToDouble(table0.Rows[0]["TodayARPU"]);
                                //double DiffARPU = Convert.ToDouble(table0.Rows[0]["DiffARPU"]);
                                //data += "平均每用户收入(充值总数/活跃人数)：" + TodayARPU.formatN() + "   " + DiffARPU.formatN() + getArrow(DiffARPU) + Environment.NewLine;

                                //int TodayPayRate = Convert.ToInt32(table0.Rows[0]["TodayPayRate"]);
                                //int DiffPayRate = Convert.ToInt32(table0.Rows[0]["DiffPayRate"]);
                                //data += "付费率：" + TodayPayRate.formatN() + "   " + DiffPayRate.formatN() + getArrow(DiffPayRate) + Environment.NewLine;

                                //int TodayAvgLTV = Convert.ToInt32(table0.Rows[0]["TodayAvgLTV"]);
                                //int DiffAvgLTV = Convert.ToInt32(table0.Rows[0]["DiffAvgLTV"]);
                                //data += "人均贡献（（系统总税收+机器人输赢）/活跃人数）：" + TodayAvgLTV.formatN() + "   " + DiffAvgLTV.formatN() + getArrow(DiffAvgLTV) + Environment.NewLine;


                                data +=  "-------兑换-------" + Environment.NewLine;
                                double TodayConvertAmount = Convert.ToDouble(table0.Rows[0]["TodayConvertAmount"]);
                                double DiffConvertAmount = Convert.ToDouble(table0.Rows[0]["DiffConvertAmount"]);
                                data += "兑换：" + TodayConvertAmount.formatN() + "   " + DiffConvertAmount.formatN() + getArrow(DiffConvertAmount) + Environment.NewLine;

                                int TodayConvertUser = Convert.ToInt32(table0.Rows[0]["TodayConvertUser"]);
                                int DiffConvertUser = Convert.ToInt32(table0.Rows[0]["DiffConvertUser"]);
                                data += "兑换人数：" + TodayConvertUser.formatN() + "   " + DiffConvertUser.formatN() + getArrow(DiffConvertUser) + Environment.NewLine;

                                data += "兑换率：" + (TodayConvertAmount/(double)TodayCharge).ToString("P") + Environment.NewLine;




                                data +=  "-------注册-------" + Environment.NewLine;
                                int TodayRegCount = Convert.ToInt32(table0.Rows[0]["TodayRegCount"]);
                                int DiffRegCount = Convert.ToInt32(table0.Rows[0]["DiffRegCount"]);
                                data += "注册人数：" + TodayRegCount.formatN() + "   " + DiffRegCount.formatN() + getArrow(DiffRegCount) + Environment.NewLine;

                                //int TodayRegBindCount = Convert.ToInt32(table0.Rows[0]["TodayRegBindCount"]);
                                //int DiffRegBindCount = Convert.ToInt32(table0.Rows[0]["DiffRegBindCount"]);
                                //data += "注册绑定人数：" + TodayRegBindCount.formatN() + "   " + DiffRegBindCount.formatN() + getArrow(DiffRegBindCount) + Environment.NewLine;

                                int TodayLogonBind = Convert.ToInt32(table0.Rows[0]["TodayLogonBind"]);
                                data += "绑定总数：" + TodayLogonBind.formatN() + Environment.NewLine;

                                int TodayNewLoginIP = Convert.ToInt32(table0.Rows[0]["TodayNewLoginIP"]);
                                int DiffNewLoginIP = Convert.ToInt32(table0.Rows[0]["DiffNewLoginIP"]);
                                data += "今日新进入IP：" + TodayNewLoginIP.formatN() + "   " + DiffNewLoginIP.formatN() + getArrow(DiffNewLoginIP) + Environment.NewLine;


                                double TodayRegCharge = Convert.ToDouble(table0.Rows[0]["TodayRegCharge"]);
                                double DiffRegCharge = Convert.ToDouble(table0.Rows[0]["DiffRegCharge"]);
                                data += "注充比例：" + TodayRegCharge.formatN() + "%" + "   " + DiffRegCharge.formatN() + "%" + getArrow(DiffRegCharge) + Environment.NewLine;

                                double TodayRegBind = Convert.ToDouble(table0.Rows[0]["TodayRegBind"]);
                                double DiffRegBind = Convert.ToDouble(table0.Rows[0]["DiffRegBind"]);
                                data += "注绑比例：" + TodayRegBind.formatN() + "%" + "   " + DiffRegBind.formatN() + "%" + getArrow(DiffRegBind) + Environment.NewLine;

                                double TodayBindCharge = Convert.ToDouble(table0.Rows[0]["TodayBindCharge"]);
                                double DiffBindCharge = Convert.ToDouble(table0.Rows[0]["DiffBindCharge"]);
                                data += "绑充比例：" + TodayBindCharge.formatN() + "%" + "   " + DiffBindCharge.formatN() + "%" + getArrow(DiffBindCharge) + Environment.NewLine;



                           
                                data += "-------登录-------" + Environment.NewLine;
                                int TodayMaxOnline = Convert.ToInt32(table0.Rows[0]["TodayMaxOnline"]);
                                int DiffMaxOnline = Convert.ToInt32(table0.Rows[0]["DiffMaxOnline"]);
                                data += "最高在线：" + TodayMaxOnline.formatN() + "   " + DiffMaxOnline.formatN() + getArrow(DiffMaxOnline) + Environment.NewLine;

                                int TodayLogonTimes = Convert.ToInt32(table0.Rows[0]["TodayLogonTimes"]);
                                int DiffLogonTimes = Convert.ToInt32(table0.Rows[0]["DiffLogonTimes"]);
                                data += "今日登陆(登录总人次)：" + TodayLogonTimes.formatN() + "   " + DiffLogonTimes.formatN() + getArrow(DiffLogonTimes) + Environment.NewLine;

                                int TodayDAU = Convert.ToInt32(table0.Rows[0]["TodayDAU"]);
                                int DiffDAU = Convert.ToInt32(table0.Rows[0]["DiffDAU"]);
                                data += "活跃人数（登录去重）：" + TodayDAU.formatN() + "   " + DiffDAU.formatN() + getArrow(DiffDAU) + Environment.NewLine;

                                int TodayIOSLoginCount = Convert.ToInt32(table0.Rows[0]["TodayIOSLoginCount"]);
                                int DiffIOSLoginCount = Convert.ToInt32(table0.Rows[0]["DiffIOSLoginCount"]);
                                data += "IOS登录总数：" + TodayIOSLoginCount.formatN() + Environment.NewLine;

                                int TodayAndroidLoginCount = Convert.ToInt32(table0.Rows[0]["TodayAndroidLoginCount"]);
                                int DiffAndroidLoginCount = Convert.ToInt32(table0.Rows[0]["DiffAndroidLoginCount"]);
                                data += "Android登录总数：" + TodayAndroidLoginCount.formatN() + Environment.NewLine;

                                
                                


                                //dataOther += "-------其他-------" + Environment.NewLine;
                                //double TodayChargeConvertRate = Convert.ToDouble(table0.Rows[0]["TodayChargeConvertRate"]);
                                //double DiffChargeConvertRate = Convert.ToDouble(table0.Rows[0]["DiffChargeConvertRate"]);
                                //dataOther += "营收比：" + TodayChargeConvertRate.formatN() + "   " + DiffChargeConvertRate.formatN() + getArrow(DiffChargeConvertRate) + Environment.NewLine;

                                //int TodayIncome = Convert.ToInt32(table0.Rows[0]["TodayIncome"]);
                                //int DiffIncome = Convert.ToInt32(table0.Rows[0]["DiffIncome"]);
                                //dataOther += "营收：" + TodayIncome.formatN() + "   " + DiffIncome.formatN() + getArrow(DiffIncome) + Environment.NewLine;
        



                                dataAgent += "-------代理-------" + Environment.NewLine;
                                int TodayAgentOrder = Convert.ToInt32(table0.Rows[0]["TodayAgentOrder"]);
                                int DiffAgentOrder = Convert.ToInt32(table0.Rows[0]["DiffAgentOrder"]);
                                dataAgent += "代理进货：" + TodayAgentOrder.formatN() + "   " + DiffAgentOrder.formatN() + getArrow(DiffAgentOrder) + Environment.NewLine;

                                int TodayOneDayKeep = Convert.ToInt32(table0.Rows[0]["TodayOneDayKeep"]);
                                int DiffOneDayKeep = Convert.ToInt32(table0.Rows[0]["DiffOneDayKeep"]);
                                dataAgent += "昨日留存：" + TodayOneDayKeep.formatN() + "   " + DiffOneDayKeep.formatN() + getArrow(DiffOneDayKeep) + Environment.NewLine;

                                int TodayThirdKeep = Convert.ToInt32(table0.Rows[0]["TodayThirdKeep"]);
                                int DiffThirdKeep = Convert.ToInt32(table0.Rows[0]["DiffThirdKeep"]);
                                dataAgent += "三日留存：" + TodayThirdKeep.formatN() + "   " + DiffThirdKeep.formatN() + getArrow(DiffThirdKeep) + Environment.NewLine;

                                int TodayFifthKeep = Convert.ToInt32(table0.Rows[0]["TodayFifthKeep"]);
                                int DiffFifthKeep = Convert.ToInt32(table0.Rows[0]["DiffFifthKeep"]);
                                dataAgent += "五日留存：" + TodayFifthKeep.formatN() + "   " + DiffFifthKeep.formatN() + getArrow(DiffFifthKeep) + Environment.NewLine;

                                int TodayWeekKeep = Convert.ToInt32(table0.Rows[0]["TodayWeekKeep"]);
                                int DiffWeekKeep = Convert.ToInt32(table0.Rows[0]["DiffWeekKeep"]);
                                dataAgent += "七日留存：" + TodayWeekKeep.formatN() + "   " + DiffWeekKeep.formatN() + getArrow(DiffWeekKeep) + Environment.NewLine;


                                double TodayTotalTaxAmount = Convert.ToDouble(table0.Rows[0]["TodayTotalTaxAmount"]);
                                double DiffTotalTaxAmount = Convert.ToDouble(table0.Rows[0]["DiffTotalTaxAmount"]);
                                lastdata += "系统总税收：" + TodayTotalTaxAmount.formatN() + "   " + DiffTotalTaxAmount.formatN() + getArrow(DiffTotalTaxAmount) + Environment.NewLine;

                                double TodayWinLoseAmount = Convert.ToDouble(table0.Rows[0]["TodayWinLoseAmount"]);
                                double DiffWinLoseAmount = Convert.ToDouble(table0.Rows[0]["DiffWinLoseAmount"]);
                                lastdata += "系统总输赢：" + TodayWinLoseAmount.formatN() + "   " + DiffWinLoseAmount.formatN() + getArrow(DiffWinLoseAmount) + Environment.NewLine;

                            }

                            dataGame += "-------游戏-------" + Environment.NewLine;
                            DataTable table1 = ds.Tables[1];
                            if (ds.Tables[1].Rows.Count > 0)
                            {

                                for (int r = 0; r < table1.Rows.Count; r++)
                                {

                                    string GameName = table1.Rows[r]["GameName"].ToString();

                                    //double TodaySystemAmount = Convert.ToDouble(table1.Rows[r]["TodaySystemAmount"]);
                                    //double DiffSystemAmount = Convert.ToDouble(table1.Rows[r]["DiffSystemAmount"]);
                                    //dataGame += GameName + "系统输赢：" + TodaySystemAmount.formatN() + "   " + DiffSystemAmount.formatN() + getArrow(DiffSystemAmount) + Environment.NewLine;

                                    double TodayTaxAmount = Convert.ToDouble(table1.Rows[r]["TodayTaxAmount"]);
                                    double DiffTaxAmount = Convert.ToDouble(table1.Rows[r]["DiffTaxAmount"]);
                                    dataGame += GameName + "税收：" + TodayTaxAmount.formatN() + "   " + DiffTaxAmount.formatN() + getArrow(DiffTaxAmount) + Environment.NewLine;

                                    //int TodayMAXCCU = Convert.ToInt32(table1.Rows[r]["TodayMAXCCU"]);
                                    //int DiffMAXCCU = Convert.ToInt32(table1.Rows[r]["DiffMAXCCU"]);
                                    //dataGame += GameName + "最高在线：" + TodayMAXCCU.formatN() + "   " + DiffMAXCCU.formatN() + getArrow(DiffMAXCCU) + Environment.NewLine;

                                    //int gameTodayDAU = Convert.ToInt32(table1.Rows[r]["TodayDAU"]);
                                    //int gameDiffDAU = Convert.ToInt32(table1.Rows[r]["DiffDAU"]);
                                    //dataGame += GameName + "活跃人数：" + gameTodayDAU.formatN() + "   " + gameDiffDAU.formatN() + getArrow(gameDiffDAU) + Environment.NewLine;

                                    //int gameTodayARPPU = Convert.ToInt32(table1.Rows[r]["TodayARPPU"]);
                                    //int gameDiffARPPU = Convert.ToInt32(table1.Rows[r]["DiffARPPU"]);
                                    //dataGame += GameName + "人均税收贡献：" + gameTodayARPPU.formatN() + "   " + gameDiffARPPU.formatN() + getArrow(gameDiffARPPU) + Environment.NewLine;

                                    //int gameTodayARPU = Convert.ToInt32(table1.Rows[r]["TodayARPU"]);
                                    //int gameDiffARPU = Convert.ToInt32(table1.Rows[r]["DiffARPU"]);
                                    //dataGame += GameName + "人均贡献：" + gameTodayARPU.formatN() + "   " + gameDiffARPU.formatN() + getArrow(gameDiffARPU) + Environment.NewLine;

                                    //int gameTodayARPPURate = Convert.ToInt32(table1.Rows[r]["TodayARPPURate"]);
                                    //int gameDiffARPPURate = Convert.ToInt32(table1.Rows[r]["DiffARPPURate"]);
                                    //dataGame += GameName + "税收贡献占比：" + gameTodayARPPURate.formatN() + "%" + "   " + gameDiffARPPURate.formatN() + "%" + getArrow(gameDiffARPPURate) + Environment.NewLine;

                                    //int gameTodayARPURate = Convert.ToInt32(table1.Rows[r]["TodayARPURate"]);
                                    //int gameDiffARPURate = Convert.ToInt32(table1.Rows[r]["DiffARPURate"]);
                                    //dataGame += GameName + "贡献占比：" + gameTodayARPURate.formatN() + "%" + "   " + gameDiffARPURate.formatN() + "%" + getArrow(gameDiffARPURate) + Environment.NewLine;
                                }
                            }


                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " step 2 exception: " + ex.Message);
                            Common.FileLog(ex.Message + Environment.NewLine + ex.StackTrace, filelog);
                        }

                    }
                }
                
                data += dataGame;
                data += dataAgent;
                data += Environment.NewLine + lastdata;
                Common.FileLog(data, filelog);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " getReport() exception: " + ex.Message);
                Common.FileLog(ex.Message + Environment.NewLine + ex.StackTrace, filelog);

                return "";
            }
        }
        static string getArrow(Int64 diff_num )
        {
            return diff_num >= 0 ? "↑" : "↓";
        }

        static string getArrow(double diff_num)
        {
            return diff_num >= 0 ? "↑" : "↓";
        }

    }

    public static class StringExtension
    {
        //public static string formatN(this int num)
        //{
        //    int digit = 0;
        //    string formatStr = "";
        //    string strNum = num.ToString();
        //    //取小數點
        //    int dot = strNum.IndexOf('.');
        //    string dotStr = "";
        //    if (dot > 0)
        //    {
        //        dotStr = strNum.Substring(dot, strNum.Length - dot);
        //        strNum = strNum.Replace(dotStr, "");
        //    }
        //    //Console.WriteLine(dotStr);            
        //    //Console.WriteLine(num);

        //    char[] chrNum = strNum.ToCharArray();

        //    for (int i = chrNum.Length - 1; i >= 0; i--)
        //    {

        //        formatStr = chrNum[i] + formatStr;
        //        digit++;
        //        if (digit == 4)
        //        {
        //            digit = 0;

        //            if (i > 0 && chrNum[i - 1] != '-')
        //                formatStr = "," + formatStr;
        //        }
        //    }
        //    return formatStr + dotStr;
        //}
        public static string formatN<T>(this T num)
        {
            int digit = 0;
            string formatStr = "";
            string strNum = num.ToString();
            //取小數點
            int dot = strNum.IndexOf('.');
            string dotStr = "";
            if (dot > 0)
            {
                dotStr = strNum.Substring(dot, strNum.Length - dot);
                strNum = strNum.Replace(dotStr, "");
            }
            //Console.WriteLine(dotStr);            
            //Console.WriteLine(num);

            char[] chrNum = strNum.ToCharArray();

            for (int i = chrNum.Length - 1; i >= 0; i--)
            {

                formatStr = chrNum[i] + formatStr;
                digit++;
                if (digit == 4)
                {
                    digit = 0;

                    if (i > 0 && chrNum[i - 1] != '-')
                        formatStr = "," + formatStr;
                }
            }

            //取小數點後四位 dotStr = '.xxxxxxx'
            if (dotStr.Length > 5)
            {
                dotStr = dotStr.Substring(0, 5);
            }
            return formatStr + dotStr;
        }
    }
}
