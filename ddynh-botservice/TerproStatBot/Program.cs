using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CommonLib;
using System.Threading.Tasks;

namespace TerproStatBot
{
    class Program
    {
        static bool isUnion = false; //是否是採用聯運計算
        static string ProductName = "FZ";

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
            get { return ConfigurationManager.AppSettings["filelogname"] != null ? ConfigurationManager.AppSettings["filelogname"] : "filelog.txt"; }
        }

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                isUnion = args[0].ToLower() == "-union" ? true : false;
                ProductName = args[0].ToLower() == "-union" ? "联运" : "FZ";
            }
            else
            {
                ProductName = "FZ";
                isUnion = false;
            }

            //Console.Write(isUnion);
            //return;

            job();
        }

        static void job()
        {

            try
            {
                Task t = process();
                
                //wait task complete
                t.Wait();

                Console.WriteLine("job done.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Common.FileLog(e.Message, filelog);
            }

        }

        static async Task process ()
        {
            string data = getReport();

            //Common.FileLog(data, filelog);
            await Common.PrintAndLogAsync(data, filelog);

            if (data.Length > 0)
                Common.pushMessage(botToken, chatid, data);
            
        }

        static string getReport()
        {
            try
            {
                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
                string dt = DateTime.Now.ToString("yyyy-MM-dd");
                string data = "";

                //如果job是早上1點前驅動, 计算前一日
                if (DateTime.Now.Hour == 0)
                {
                    dt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }

                data += ProductName + " 產品資訊：" + Environment.NewLine + Environment.NewLine;
                data += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + Environment.NewLine;

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    string FilterArea = "";
                    if (isUnion)
                        FilterArea += " and areaid in (select areaid from  centerdb.area.area with (nolock) where PlatformID in (select PlatformID from  centerdb.Area.UnionPlatform)	)";
                    else
                        FilterArea += " and areaid not in (select areaid from  centerdb.area.area with (nolock) where PlatformID in (select PlatformID from  centerdb.Area.UnionPlatform)	)";

                    //註冊付費人數
                    String sqlReg =
                        $@"
                        declare @intTotalConvertUser int
                        declare @intSumConvertAmount bigint
                        select @intTotalConvertUser=isnull(count(distinct userid),0), @intSumConvertAmount=isnull(sum(UpdateAmount/100),0) from [LogDB].[Money].[MoneyLog] with (nolock)
                        where sourcetypeid=14000
                        and userid in (select userid from centerdb.users.UserInfo with (nolock) where CreateTime>='{dt}')
                        {FilterArea}

                        select
                        @intTotalConvertUser as TotalConvertUser,
                        @intSumConvertAmount as SumConvertAmount,
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

                                data += "[今日新用戶]" + Environment.NewLine;
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {
                                    int TotalConvertUser = Convert.ToInt32(table0.Rows[r]["TotalConvertUser"]);
                                    int SumConvertAmount = Convert.ToInt32(table0.Rows[r]["SumConvertAmount"]);
                                    int RegUser = Convert.ToInt32(table0.Rows[r]["RegUser"]);
                                    int SumCharge = Convert.ToInt32(table0.Rows[r]["SumCharge"]); //第三方充值
                                    int SumAgentCharge = Convert.ToInt32(table0.Rows[r]["SumAgentCharge"]); //代理充值
                                    int ChargeUser = Convert.ToInt32(table0.Rows[r]["ChargeUser"]); //第三方充值
                                    int AgentChargeUser = Convert.ToInt32(table0.Rows[r]["AgentChargeUser"]); //代理充值
                                    data += $"註冊人數： {RegUser.ToString()}" + Environment.NewLine;
                                    data += $"充值人數： {(ChargeUser + AgentChargeUser).ToString()} (第三方 {ChargeUser.ToString()} 代理 {AgentChargeUser.ToString()})" + Environment.NewLine;
                                    data += $"充值金額： {(SumCharge + SumAgentCharge).ToString("#,0")} (第三方 {SumCharge.ToString("#,0")} 代理 {SumAgentCharge.ToString("#,0")})" + Environment.NewLine;
                                    data += $"兑换人數： {TotalConvertUser.ToString()}" + Environment.NewLine;
                                    data += $"兑换金額： {SumConvertAmount.ToString()}" + Environment.NewLine;
                                }
                                data += Environment.NewLine;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '今日新用戶' exception: " + ex.Message);
                            return "";
                        }
                    }


                    //總充值 ＆ 充值成功率
                    int totalCharge = 0;
                    //充值成功率
                    string successRate = "";
                    data += "[今日充值兌換數]" + Environment.NewLine;

                    string union_charge_str = "", union_charge_str2= "";

                    if (isUnion) {
                        union_charge_str = "PlatformID in (select PlatformID from Area.UnionPlatform)";
                        union_charge_str2 = "U.AreaID in (select areaid from area.area with (nolock) where PlatformID in (select PlatformID from Area.UnionPlatform)	)";
                    }
                    else {
                        union_charge_str = "PlatformID not in (select PlatformID from Area.UnionPlatform)";
                        union_charge_str2 = "U.AreaID not in (select areaid from area.area with (nolock) where PlatformID in (select PlatformID from Area.UnionPlatform)	)";
                    }



                    String sqlTotalCharge = $@"
                    SELECT ISNULL(SUM(SumCharge),0) as SumCharge, ISNULL(SUM(SumAgentCharge),0) as SumAgentCharge, SUM(SuccessRate) as SuccessRate
                    FROM (

	                    SELECT 
	                    0 as SumCharge,
	                    SUM(UpdateAmount/100) as SumAgentCharge, 
	                    0 as successrate 
	                    from LogDB.Money.MoneyLog with (nolock) where SourceTypeID = 11008 and CreateTime >= '{dt}'
                        and {union_charge_str}

	                    UNION

	                    SELECT 
	                    SUM(CASE when Status = (2) THEN Amount ELSE 0 END) as SumCharge, 
	                    0 as SumAgentCharge,
	                    CONVERT( FLOAT, ROUND(SUM(CASE when Status = (2) THEN 1 ELSE 0 END) / (COUNT(ID) * 1.00) * 100,2) ) as successRate
	                    FROM pay.OrderList O with (nolock) left join Users.Userinfo U with (nolock) on O.UserID=U.UserID
                        where  O.CreateTime >= '{dt}'
                        and {union_charge_str2}
                    ) AS a";

                    using (SqlCommand cmdReg = new SqlCommand(sqlTotalCharge, conn))
                    {

                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdReg;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "totalCharge");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {
                                    int sumCharge = Convert.ToInt32(table0.Rows[r]["sumCharge"]);
                                    int sumAgentCharge = Convert.ToInt32(table0.Rows[r]["sumAgentCharge"]);
                                    totalCharge = sumCharge + sumAgentCharge;
                                    successRate = (table0.Rows[r]["successRate"].ToString());
                                    data += $"總充值金額： {(sumCharge + sumAgentCharge).ToString("#,0")} (第三方 {sumCharge.ToString("#,0")} 代理 {sumAgentCharge.ToString("#,0")})" + Environment.NewLine;
                                    
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '總充值金額' exception: " + ex.Message);
                            return "";
                        }
                    }



                    //兌換
                    int totalConvert = 0;
                    string union_convert_str = "";
                    if (isUnion)
                    {
                        union_convert_str = "PlatformID in (select PlatformID from Area.UnionPlatform)";
                    }
                    else
                    {
                        union_convert_str = "PlatformID not in (select PlatformID from Area.UnionPlatform)";
                    }
                    String sqlTotalConvert = $@"
								  SELECT ISNULL(SUM(amount)/100,0) as totalConvert
								  from LogDB.Money.ConvertLog with (nolock) where  LastUpdateTime >= '{dt}'
								  and Status in (3,98)
                                  and {union_convert_str}
                                    ";
                    using (SqlCommand cmdReg = new SqlCommand(sqlTotalConvert, conn))
                    {

                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdReg;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "totalConvert");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                for (int r = 0; r < table0.Rows.Count; r++)
                                {
                                    totalConvert = Convert.ToInt32(table0.Rows[r]["totalConvert"]);
                                    data += $"總兌換金額： {totalConvert.ToString("#,0")}" + Environment.NewLine;
                                }
                                
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '總兌換金額' exception: " + ex.Message);
                            return "";
                        }
                    }

                    string converRate =
                        totalCharge == 0 ? "" : ((Math.Abs(totalConvert) * 1.0) / totalCharge * 100).ToString("#0.00");
                    data += $"充值成功率： {successRate}%" + Environment.NewLine;
                    data += $"兌換率： {converRate}%" + Environment.NewLine;
                    data += Environment.NewLine;


                    //每小时充值量
                    string charge_perhour_table = "";
                    if (isUnion)
                    {
                        charge_perhour_table = "stat.ChargePerHourUnionplatform";
                    }
                    else
                    {
                        charge_perhour_table = "stat.ChargePerHour";
                    }

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

                        select * from {charge_perhour_table} with(nolock) WHERE YYYYMMDD >= @intFromYYYYMMDD and YYYYMMDD <=@intToYYYYMMDD and HH=@intHour order by YYYYMMDD desc";


                            data += $"[最近7天充值總額]" + Environment.NewLine;
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;
                            rd = cmd.ExecuteReader();
                            if (rd.Read())
                            {
                                int HH = Convert.ToInt32(rd["HH"].ToString());
                                int MM = Convert.ToInt32(rd["MM"].ToString());
                                int DD = Convert.ToInt32(rd["DD"].ToString());
                                //int TotalAmount = Convert.ToInt32(rd["TotalAmount"].ToString());
                                int HourAmount = Convert.ToInt32(rd["HourAmount"].ToString());

                                data += $"今日此刻充值总额:{totalCharge.ToString("#,0")}(1小时充值:{HourAmount.ToString("#,0")})" + Environment.NewLine;
                            }

                            while (rd.Read())
                            {
                                int HH = Convert.ToInt32(rd["HH"].ToString());
                                int MM = Convert.ToInt32(rd["MM"].ToString());
                                int DD = Convert.ToInt32(rd["DD"].ToString());
                                int TotalAmount = Convert.ToInt32(rd["TotalAmount"].ToString());
                                int HourAmount = Convert.ToInt32(rd["HourAmount"].ToString());

                                data += $"{MM}/{DD}此刻充值总额:{TotalAmount.ToString("#,0")}(1小时充值:{HourAmount.ToString("#,0")})" + Environment.NewLine;
                            }
                            data += Environment.NewLine;
                            rd.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '最近7天充值总额' exception: " + ex.Message);
                        }
                        finally
                        {
                            if (rd != null && !rd.IsClosed)
                                rd.Close();
                        }

                    }

                    


                    //遊戲即時人數
                    String strOnline = String.Format(@"
                                    select b.GameID, c.GameName, COUNT(UserID) as onlineUser from Game.GameUserList as a with(nolock)
                                    left join ServerInfo.GameServerList as b with(nolock)
                                    on a.ServerID = b.ServerID
                                    left join ServerInfo.GameList as c with(nolock)
                                    on c.GameID = b.GameID
                                    where a.UserID not in (select UserID from Robot.RobotList with (nolock))
                                    group by b.gameid,c.GameName");

                    using (SqlCommand cmdOnline = new SqlCommand(strOnline, conn))
                    {
                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdOnline;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "Online");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                data += "[遊戲在線人數]" + Environment.NewLine;
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {

                                    string gamename = table0.Rows[r]["GameName"].ToString();
                                    int onlineuser = Convert.ToInt32(table0.Rows[r]["onlineuser"]);
                                    data += gamename + "：   " + onlineuser.ToString() + Environment.NewLine;
                                }
                                data += Environment.NewLine;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '遊戲在線人數' exception: " + ex.Message);
                            return "";
                        }

                    }


                    //渠道最高付費top5
                    string union_topcharge_str = "";
                    if (isUnion)
                    {
                        union_topcharge_str = "a.PlatformID in (select PlatformID from Area.UnionPlatform)";
                    }else
                    {
                        union_topcharge_str = "a.PlatformID not in (select PlatformID from Area.UnionPlatform)";
                    }


                    String sqlTopAreaCharge = String.Format($@"
		            select top 5 stattime, a.AreaID, b.AreaName, (oldCharge+NewCharge)/100 as cg, (OldConvert+NewConvert)/100 as ct from [CenterDB].[Stat].[AreaDataDay] as a
		            left join area.Area as b
		            on a.AreaID = b.AreaID
		            where StatTime >='{dt}' 
		            and a.AreaID <> 0 and a.PlatformID <> 0
                    and {union_topcharge_str}
		            group by stattime, a.areaid, b.AreaName ,oldcharge,NewCharge , OldConvert, NewConvert
		            order by cg desc");

                    using (SqlCommand cmdTopAreaCharge = new SqlCommand(sqlTopAreaCharge, conn))
                    {
                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdTopAreaCharge;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "TopAreaCharge");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                data += "[渠道充值數量top5]" + Environment.NewLine;
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {

                                    string AreaName = table0.Rows[r]["AreaName"].ToString();
                                    int cg = Convert.ToInt32(table0.Rows[r]["cg"]);
                                    int ct = Convert.ToInt32(table0.Rows[r]["ct"]);
                                    data += AreaName + Environment.NewLine + " 充值：" + cg.ToString("#,0") + "   兌換：" + ct.ToString("#,0") + Environment.NewLine;
                                }
                                data += Environment.NewLine;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '渠道充值數量top5' exception: " + ex.Message);
                            return "";
                        }

                    }


                    //渠道最高兌換top5
                    string union_topconvert_str = "";
                    if (isUnion)
                    {
                        union_topconvert_str = "a.PlatformID in (select PlatformID from Area.UnionPlatform)";
                    }
                    else
                    {
                        union_topconvert_str = "a.PlatformID not in (select PlatformID from Area.UnionPlatform)";
                    }

                    String sqlTopAreaConvert = String.Format($@"
		            select top 5 stattime, a.AreaID, b.AreaName,  (oldCharge+NewCharge)/100 as cg, (OldConvert+NewConvert)/100 as ct from [CenterDB].[Stat].[AreaDataDay] as a
		            left join area.Area as b
		            on a.AreaID = b.AreaID
		            where StatTime >='{dt}' 
		            and a.AreaID <> 0 and a.PlatformID <> 0
                    and {union_topconvert_str}
		            group by stattime, a.areaid , b.AreaName ,oldcharge,NewCharge , OldConvert, NewConvert
		            order by (OldConvert+NewConvert)");

                    using (SqlCommand cmdsqlTopAreaConvert = new SqlCommand(sqlTopAreaConvert, conn))
                    {
                        try
                        {
                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = cmdsqlTopAreaConvert;
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds, "TopAreaCharge");

                            DataTable table0 = ds.Tables[0];

                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                data += "[渠道兌換數量top5]" + Environment.NewLine;
                                for (int r = 0; r < table0.Rows.Count; r++)
                                {

                                    string AreaName = table0.Rows[r]["AreaName"].ToString();
                                    int cg = Convert.ToInt32(table0.Rows[r]["cg"]);
                                    int ct = Convert.ToInt32(table0.Rows[r]["ct"]);
                                    data += AreaName + Environment.NewLine + " 充值：" + cg.ToString("#,0") + "   兌換：" + ct.ToString("#,0") + Environment.NewLine;
                                }
                                data += Environment.NewLine;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " getReport() '渠道兌換數量top5' exception: " + ex.Message);
                            return "";
                        }


                    }
 
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " getReport() exception: " + ex.Message);
                return "";
            }
        }


    }
}
