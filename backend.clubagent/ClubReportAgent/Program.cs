using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClubReportAgent
{
    class Program
    {

        static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }
        static string key
        {
            get { return ConfigurationManager.AppSettings["key"]; }
        }

        static string URL
        {
            get { return ConfigurationManager.AppSettings["url"]; }
        }

        static string FileLogName { get { return "ClubReport.txt"; } }

        static void Main(string[] args)
        {

            calcPerHour();

        }

        static string Day = "", TotalCharge = "0", TotalConvert = "0", TotalChargeUser = "0", TotalLogin = "0", TotalReg = "0";
        static void calcPerHour()
        {


            string sql = @"

	                    DECLARE @TotalReg		INT=0  --總註冊人數
	                    DECLARE @TotalLogin		INT=0  --總登入人數
	                    DECLARE @TotalChargeUser INT=0  --充值人數
	                    DECLARE @TotalCharge	BIGINT=0 --充值總額
	                    DECLARE @TotalConvert	BIGINT=0 --兌換總額
	                    DECLARE @PlatformID		INT=74 --限黑桃娛樂
	                    DECLARE @Day			varchar(20)=convert(char(10),getdate(),120)
						DECLARE @BeginDate		varchar(20)=convert(varchar,getdate(),110)
						DECLARE @EndDate		varchar(20)=convert(varchar,getdate()+1,110)


						--如果job是早上1點前驅動, 计算前一日
						IF DatePart(hour,getdate())=0
						BEGIN
							PRINT '计算前一日'
							SET @Day=convert(char(10),getdate()-1,120)
							SET @BeginDate=convert(varchar,getdate()-1,110)
							SET @EndDate=convert(varchar,getdate(),110)							
						END

	                    --充值
	                    SELECT	@TotalCharge = SUM([UpdateAmount]), @TotalChargeUser = count(distinct UserID)
	                    FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	                    WHERE
							CreateTime >= @BeginDate AND CreateTime < @EndDate
		                    AND [SourceTypeID] >= 11001 and [SourceTypeID] <=11099
		                    AND PlatformID=@PlatformID

	                    --兌換
	                    SELECT @TotalConvert = SUM(ABS(C.amount))
	                    FROM logdb.Money.ConvertLog C with (nolock)
	                    WHERE 
						C.LastUpdateTime >= @BeginDate AND C.LastUpdateTime < @EndDate
						AND Status=3
	                    AND PlatformID=@PlatformID

	                    --登入(mac去重)
	                    SELECT @TotalLogin = count(distinct U.HDCode) 
	                    FROM LogDB.Game.GameLog G with (nolock) left join CenterDB.Users.UserInfo U with (nolock) on G.UserID=U.UserID
	                    WHERE  
						( LoginTime >= @BeginDate AND LoginTime < @EndDate )
	                    and PlatformID=@PlatformID

	                    --註冊(mac去重)
	                    Select @TotalReg=Count(distinct HDCode) From CenterDB.Users.UserInfo with (nolock) where 
						CreateTime >= @BeginDate AND CreateTime < @EndDate
	                    and AreaID in (Select AreaID from CenterDB.Area.Area with (nolock) where PlatformID=@PlatformID)
	

	                    select @Day as Day,ISNULL(@TotalCharge,0) as TotalCharge, ISNULL(@TotalConvert,0) as TotalConvert, ISNULL(@TotalChargeUser,0) as TotalChargeUser, ISNULL(@TotalLogin,0) as TotalLogin, ISNULL(@TotalReg,0) as TotalReg";



            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataReader rd = null;
                    try
                    {
                            
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            Day = rd["Day"].ToString();
                            TotalCharge = rd["TotalCharge"].ToString();
                            TotalConvert = rd["TotalConvert"].ToString();
                            TotalChargeUser = rd["TotalChargeUser"].ToString();
                            TotalLogin = rd["TotalLogin"].ToString();
                            TotalReg = rd["TotalReg"].ToString();
                        }

                        rd.Close();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Common.FileLog($"{e.Message}{Environment.NewLine}{e.StackTrace}", FileLogName);
                    }
                    finally
                    {
                        if (rd != null && !rd.IsClosed)
                            rd.Close();
                    }

                    //post params
                    string param = $"day={Day}&platform_id=6&recharge_count={TotalChargeUser}&total_exchange={TotalConvert}&total_login={TotalLogin}&total_recharge={TotalCharge}&total_reg={TotalReg}";
                    string sign = $"{param}&key={key}";
                    string md5 = Common.CalculateMD5Hash(sign);
                    Console.WriteLine(sign);
                    Console.WriteLine(md5);

                    //post data
                    string responseData = "";
                    param = param + "&sign=" + md5;
                    Console.WriteLine(param);
                    int statusCode = Common.HttpPost(URL, param, ref responseData);

                    //file log
                    Common.FileLog($"{statusCode} {param} {responseData}", FileLogName);
                    Console.WriteLine(statusCode);
                    Console.WriteLine(responseData);


                }
            }

        }


    }
}
