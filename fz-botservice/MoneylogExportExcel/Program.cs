using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneylogExportExcel
{
    class Program
    {
        /// <summary>
        /// 資料庫連線
        /// </summary>
        static string dbConnection { get { return ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }

        /// <summary>
        /// 每頁資料筆數
        /// </summary>
        static int PageSize { get { return Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]); } }

        /// <summary>
        /// 匯出路徑
        /// </summary>
        static string FilePath { get { return ConfigurationManager.AppSettings["filePath"] ?? Environment.CurrentDirectory;  } }

        static string fileLogName { get { return "fileLog.log"; } }

        static void Main(string[] args)
        {         
            DateTime beginTime, endTime;
            if (args.Length > 0)
            {
                if (!DateTime.TryParse(args[0], out beginTime))
                {
                    WriteLine($"error beginTime:{args[0]}", ConsoleColor.Red);
                    Environment.Exit(0);
                }
            }
            else
            {
                beginTime = Convert.ToDateTime(DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:00"));
            }

            if (args.Length > 1)
            {
                if (!DateTime.TryParse(args[1], out endTime))
                {
                    WriteLine($"error endTime:{args[1]}", ConsoleColor.Red);
                    Environment.Exit(0);
                }
            }
            else
            {
                endTime = beginTime.AddHours(1).AddMilliseconds(-1);
            }

            WriteLine($"beginTime:{beginTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}, endTime:{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

            int pageIndex = 1;
            int pageSize = PageSize;
            int pageCount = 0;
            int totalPage = SiteHelper.GetTotalPage(pageSize, pageCount);

            for (int index = pageIndex; index <= totalPage; index++)
            {   
                var list = GetMoneyLogReport(beginTime, endTime, pageIndex, pageSize, out pageCount);
                ExportMoneyLogCSV(list, beginTime);             

                totalPage = SiteHelper.GetTotalPage(pageSize, pageCount);
                WriteLine($"PageIndex:{pageIndex} TotalPage:{totalPage}", ConsoleColor.Green);
                pageIndex++;
            }
        }

        static bool ExportMoneyLogExcel(List<MoneyLogReport> datas, DateTime beginTime)
        {
            var _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportTemplat", "Pay_MoneyLogReport.xlsx");

            try
            {
                using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
                {
                    #region 畫Excel

                    var _workbook = new XSSFWorkbook(file);
                    var _sheet = _workbook.GetSheetAt(0);
                    file.Close();

                    if (datas.Count != 0)
                    {
                        for (var i = 0; i < datas.Count; i++)
                        {
                            MoneyLogReport _order = datas[i];
                            var _row = _sheet.CreateRow(3 + i);

                            _row.CreateCell(0).SetCellValue(_order.CreateTime);
                            _row.CreateCell(1).SetCellValue(_order.PlatformName);
                            _row.CreateCell(2).SetCellValue(_order.UserID);
                            _row.CreateCell(3).SetCellValue(_order.LoginName);
                            _row.CreateCell(4).SetCellValue(_order.GradeID);
                            _row.CreateCell(5).SetCellValue(_order.SourceName);
                            _row.CreateCell(6).SetCellValue(_order.CurrentAmount);
                            _row.CreateCell(7).SetCellValue(_order.UpdateAmount);
                            _row.CreateCell(8).SetCellValue(_order.TotalAmount);
                            _row.CreateCell(9).SetCellValue(_order.IPAddress);
                            _row.CreateCell(10).SetCellValue(_order.Memo);
                        }
                    }

                    #endregion

                    var dirpath = string.Format("/Excel/{0}/", beginTime.ToString("yyyyMMdd"));
                    string dir = FilePath + dirpath;
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    string saveAsFileName = string.Format("现金记录_{0}.xlsx", DateTime.Now.ToString("yyyyMMddhhmmss"));
                    var exportPath = FilePath + dirpath + saveAsFileName;

                    using (var exportData = new FileStream(exportPath, FileMode.Create))
                    {
                        _workbook.Write(exportData);
                        exportData.Close();
                    }

                    return true;
                }
            }
            catch(Exception e)
            {
                WriteLine($"ExportOrderList exception:{e}", ConsoleColor.Red);
                return false;
            }
        }

        static bool ExportMoneyLogCSV(List<MoneyLogReport> datas, DateTime beginTime)
        {
            var dirpath = string.Format(@"\CSV\{0}\", beginTime.ToString("yyyyMM"));

            string dir = FilePath + dirpath;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var _path = Path.Combine(dir, beginTime.ToString("yyyyMMdd") + ".csv");

            try
            {
                using (FileStream file = new FileStream(_path, FileMode.Append, FileAccess.Write))
                {
                    StringBuilder sb = new StringBuilder();

                    if (datas.Count != 0)
                    {
                        if (file.Length == 0)
                            sb.AppendLine("创建时间,所属平台,用户ID,帳號,用户等级,流通类型,当前数量,更新的数量,更新后数量,IP地址,说明");
                        var data = datas.Select(x => $"{x.CreateTime},{x.PlatformName},{x.UserID},'{x.LoginName},{x.SourceName},{x.CurrentAmount},{x.UpdateAmount},{x.TotalAmount},{x.IPAddress},{x.Memo}");

                        foreach (var m in data)
                            sb.AppendLine(m);

                        using (var swWriter = new StreamWriter(file, Encoding.UTF8))
                        {
                            swWriter.WriteLine(sb.ToString());
                            swWriter.Close();
                        }
                    }
                }               

                return true;
            }
            catch (Exception e)
            {
                WriteLine($"ExportOrderList exception:{e}", ConsoleColor.Red);
                return false;
            }
        }
        
        static List<MoneyLogReport> GetMoneyLogReport(DateTime beginTime, DateTime endTime, int pageIndex, int pageSize, out int pageCount)
        {
            pageCount = 0;
            List<MoneyLogReport> lstConvertLog = new List<MoneyLogReport>();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = "dbo.PrWeb_Stat_GetMoneyLog";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        cmd.CommandType = CommandType.StoredProcedure;


                        SqlParameter[] paras = {
                                    new SqlParameter("@beginTime", SqlDbType.DateTime2) { Value = beginTime },
                                    new SqlParameter("@endTime", SqlDbType.DateTime2) { Value = endTime },
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount),
                                   };
                        paras.LastOrDefault().Direction = ParameterDirection.Output;
                        cmd.Parameters.AddRange(paras);                     

                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "log");

                        pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

                        DataTable table = ds.Tables[0];                       
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table.Rows.Count; r++)
                            {
                                MoneyLogReport item = new MoneyLogReport();
                                item.CreateTime = Convert.ToDateTime(table.Rows[r]["CreateTime"]).ToString("yyyy/MM/dd HH:mm:ss");
                                item.PlatformName = table.Rows[r]["User.Area.Platform.Name"].ToString();
                                item.UserID = Convert.ToInt32(table.Rows[r]["User.Id"].ToString());
                                item.LoginName = table.Rows[r]["User.LoginName"].ToString();
                                item.GradeID = Convert.ToInt32(table.Rows[r]["User.GradeID"].ToString());
                                item.SourceName = table.Rows[r]["SourceType.SourceName"].ToString();
                                item.CurrentAmount = Convert.ToInt32(table.Rows[r]["CurrentAmount"].ToString());
                                item.UpdateAmount = Convert.ToInt32(table.Rows[r]["UpdateAmount"].ToString());
                                item.TotalAmount = item.CurrentAmount + item.UpdateAmount;
                                item.IPAddress = table.Rows[r]["IPAddress"].ToString();
                                item.Memo = table.Rows[r]["Memo"].ToString();

                                lstConvertLog.Add(item);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLine($"GetMoneyLogReport exception:{e}", ConsoleColor.Red);
                    }

                }
            }

            return lstConvertLog;
        }  

        static void WriteLine(string msg, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor bckgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = bckgroundColor;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {msg}");
            Console.ResetColor();

            FileLog(msg, fileLogName);
        }

        static object logSyncObject = new object();

        static void FileLog(string message, string filelog)
        {

            lock (logSyncObject)
            {
                var dirpath = string.Format("/logcustom/{0}/", DateTime.Now.ToString("yyyyMMdd"));
                string dir = System.Environment.CurrentDirectory + dirpath;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = System.Environment.CurrentDirectory + dirpath + filelog;

                StreamReader sr;
                if (!File.Exists(path))
                {
                    sr = new StreamReader(File.Create(path));
                }
                else sr = new StreamReader(path);
                using (sr)
                {
                    sr.Dispose();
                }
                example.invalid.File.AppendAllText(path, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + message + Environment.NewLine);
            }

        }       
    }
}
