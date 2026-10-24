using CommonLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZConvertAlert
{
    class Program
    {
        static string filelog
        {
            get { return "ConverAlert.log"; }
        }

        static void Main(string[] args)
        {
            Thread t = new Thread(job);
            t.Start();

        }
        static void ConvertWinLostJob()
        {
            //取得TG設定
            GetConvertWinLostAlertConfig();

            while (true)
            {
                try
                {
                    ConvertWinLostJobProcess();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Common.FileLog(e.Message, filelog);
                }

                Thread.Sleep(5 * 1000);
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

                string msg = $"请技术注意\r\nID：{userid}\r\n玩家：{username}\r\n总输赢：{winlost}";

                Common.FileLog(msg, filelog);
                Common.pushMessage(TgBotToken, new string[] { TGChatID }, msg);
            }
        }

        static int ConvertWinLostAlert;
        static string TgBotToken;
        static string TGChatID;
        static void GetConvertWinLostAlertConfig()
        {
            try
            {
                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    
                    String sql =$@"select ConvertWinLostAlert/100 as ConvertWinLostAlert,TgBotToken,TgChatID from CenterDB.ServerInfo.SystemConfig";

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
                            TGChatID = rd["TGChatID"].ToString();
                        }
                        rd.Close();

                        Console.WriteLine("TGToken:" + TgBotToken);
                        Console.WriteLine("TGChatID:" + TGChatID);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " GetConfig() exception: " + ex.Message);
            }


        }

        static Tuple<string,string,string> GetConvertWinLostAlert()
        {
            string UserID = null;
            string UserName = null;
            string WinLost = null;

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

                                select top 1 @intID=Id, @intUserID=UserID, @chvUserName=UserName, @intWinLost=WinLost from Notify.Notify.ConvertAlert where Status=1 
                                IF @intID > 0
                                Begin
	                                update  Notify.Notify.ConvertAlert set status=0 where Id=@intID
	                                select @intUserID as UserID, @chvUserName as UserName, @intWinLost/100 as WinLost
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
                            }

                            rd.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " GetAlert() sql exception: " + ex.Message);
                        }
                        finally
                        {
                            if (rd != null && !rd.IsClosed)
                                rd.Close();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(UserID))
                    return new Tuple<string, string, string>(UserID, UserName, WinLost);
                else
                    return null;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " GetAlert() exception: " + ex.Message);
                return null;
            }
        }
    }
}
