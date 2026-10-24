using System;
using System.Collections.Generic;
using System.Text;
using com.show.api;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

namespace showapi
{
    public class Class1
    {
        static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }
        static string api { get { return System.Configuration.ConfigurationManager.AppSettings["api"]; } }
        static string appid { get { return System.Configuration.ConfigurationManager.AppSettings["appid"]; } }
        static string appSecret { get { return System.Configuration.ConfigurationManager.AppSettings["appSecret"]; } }
        static int jobFrequency { get { return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["jobFrequency"]); } }

        static void Main(string[] args)
        {
            
            Thread th = new Thread(job);
            th.Start();
            
        }


        static void job()
        {
            while (true)
            {
                try
                {
                    UpdateAgentQr();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(jobFrequency * 60 * 1000);
            }
        }

        static void UpdateLiveQR(string code, string url)
        {
            String res = new ShowApiRequest(api, appid, appSecret)

                  .addTextPara("code", code)
                  .addTextPara("url", url)
                  .post();

            Console.WriteLine(res);
        }

        static void GetLiveQrList()
        {

            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                string sql = @"
                        DECLARE @intDomainID tinyint=0
                        SELECT TOP 1 @intDomainID=ID from Club.dbo.DomainConfig where Flag=1

                        SELECT
                        CONVERT(VARCHAR(32), HashBytes('MD5', A.invitecode), 2) as code, A.DomainID,
                        D.Domain + '/?a=' + A.invitecode as Url
                        from Club.dbo.agentusers A with(nolock)
                        left join  Club.dbo.DomainConfig D with(nolock)
                        on A.DomainID = D.ID
                        where Class not in (99,100) and A.DomainID=@intDomainID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        dataAdapter.Fill(ds, "agent");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally { }
                }

            }

            DataTable table = ds.Tables[0];
            Console.WriteLine($"{DateTime.Now.ToString()} change item {ds.Tables[0].Rows.Count}");

            //紀錄需要更新的domain id
            int DomainID=0;
            
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r < table.Rows.Count; r++)
                {
                    try
                    {
                        string code = Convert.ToString(table.Rows[r]["code"]);
                        string url = Convert.ToString(table.Rows[r]["url"]);
                        DomainID = Convert.ToInt32(table.Rows[r]["DomainID"]);
                        Console.WriteLine($"{DateTime.Now.ToString()} DomainID {DomainID} code {code} url {url}");

                        if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(url))
                        {
                            //更新活碼內容
                            //Console.WriteLine("update");
                            UpdateLiveQR(code, url);
                            
                            //每1.5秒更新一次
                            Thread.Sleep(1500);
                        }
                    }
                    finally { }
                }

            }

            //關閉已經更新域名的domainid
            if (DomainID > 0)
                CloseChangeFlag(DomainID);
        }


        static void UpdateAgentQr()
        {
            Console.WriteLine($"{DateTime.Now.ToString()} 全民代理二维码检测更新");
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                string sql = @"
                        DECLARE @intDomainID tinyint=0
						DECLARE @chvDomain varchar(200)
                        
                        SELECT TOP 1 @chvDomain=Domain, @intDomainID=ID from Club.dbo.DomainConfig where Flag=1
						IF @@ROWCOUNT > 0
						BEGIN
							Update Club.dbo.AgentUsers Set QRUrl=@chvDomain+'/?a='+InviteCode where Class not in (99,100) and DomainID=@intDomainID 
							Update Club.dbo.DomainConfig Set Flag=0 Where ID=@intDomainID
						END";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally { }
                }

            }
        }

        static void CloseChangeFlag(int DomainID)
        {
            Console.WriteLine($"Close domain id {DomainID}");
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                string sql = $"update Club.dbo.DomainConfig set Flag=0 where ID={DomainID}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally { }
                }

            }
        }

    }
}
