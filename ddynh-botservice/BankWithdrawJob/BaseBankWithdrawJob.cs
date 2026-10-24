using BankWithdrawJob.Entity;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankWithdrawJob
{
    public class BaseBankWithdrawJob
    {
        #region 參數

        /// <summary>
        /// 資料庫連線
        /// </summary>
        public static string dbConnection { get { return ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }

        /// <summary>
        /// 第三方銀行參考表
        /// </summary>
        public static List<ConvertBankCode> BankCode { get; set; }

        /// <summary>
        /// 執行記錄檔
        /// </summary>
        public static string fileLogName { get { return ConfigurationManager.AppSettings["fileLog"] ?? "fileLog.log"; } }

        public static string _merId;
        /// <summary>
        /// 商戶號
        /// </summary>
        public static string MerId
        {
            get { return _merId ?? ConfigurationManager.AppSettings["merId"] ?? string.Empty; }
            set { _merId = value; }
        }

        private static string _md5_Key;
        /// <summary>
        /// MD5密鑰
        /// </summary>        
        public static string MD5_Key
        {
            get { return _md5_Key ?? ConfigurationManager.AppSettings["md5_key"] ?? string.Empty; }
            set { _md5_Key = value;  }
        }

        private static　string _withdrawUrl;
        /// <summary>
        /// 代付接口位址
        /// </summary>
        public static string WithdrawUrl
        {
            get { return _withdrawUrl ??　ConfigurationManager.AppSettings["withdrawUrl"] ?? string.Empty; }
            set { _withdrawUrl = value; }
        }

        private static string _callbackUrl;
        /// <summary>
        /// 代付回調位址
        /// </summary>
        public static string CallbackUrl
        {
            get { return _callbackUrl ?? ConfigurationManager.AppSettings["callbackUrl"] ?? string.Empty; }
            set { _callbackUrl = value; }
        }

        /// <summary>
        /// 兑换商编号
        /// </summary>
        public static string ConvertGWID
        {
            get { return ConfigurationManager.AppSettings["convert_gwid"] ?? string.Empty; }
        }

        /// <summary>
        /// 兑换类型(1:支付宝;2:银联)
        /// </summary>
        public static string ConvertType
        {
            get { return ConfigurationManager.AppSettings["convert_type"] ?? string.Empty; }
        }

        /// <summary>
        /// 兌換單狀態(2:審核通過)
        /// </summary>
        public static string ConvertStatus
        {
            get { return ConfigurationManager.AppSettings["convert_status"] ?? "2"; }
        }

        /// <summary>
        /// 测试模式(true:测试;false:正式)
        /// </summary>
        public static bool IsDebug
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["convert_Debug"] ?? "0"); }
        }

        #endregion        

        /// <summary>
        /// 測試兌換單更新自動打款程式排队中
        /// </summary>
        /// <param name="item"></param>
        public static void Test(ConvertLog item)
        {
            int intUpdStatus = 99;                          //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id);  //兑换订单号            
            string returnMsg = "null";
            string returnTranId = "0";           

            string message = string.Empty;           
            if(UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message))
                WriteLine($"Id:{intTeamOrderId} msg:{message}", ConsoleColor.Green);
        }

        /// <summary>
        /// 審核通過的兌換單
        /// </summary>
        /// <returns></returns>
        public static List<ConvertLog> GetConvertBankLog()
        {
            List<ConvertLog> lstConvertLog = new List<ConvertLog>();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = "dbo.PrWeb_Money_WithdrawGet";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@intGWID", ConvertGWID);
                        cmd.Parameters.AddWithValue("@intConvertType", ConvertType);
                        cmd.Parameters.AddWithValue("@intStatus", ConvertStatus);

                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "log");

                        DataTable table = ds.Tables[0];

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table.Rows.Count; r++)
                            {
                                ConvertLog item = new ConvertLog();
                                item.Id = table.Rows[r]["ID"].ToString();
                                item.Amount = Math.Abs(Convert.ToInt64(table.Rows[r]["Amount"].ToString()));
                                item.BankName = table.Rows[r]["BankName"].ToString();
                                item.RealName = table.Rows[r]["RealName"].ToString();
                                item.Account = table.Rows[r]["Account"].ToString();
                                item.LoginName = table.Rows[r]["LoginName"].ToString();
                                item.BankID = table.Rows[r]["BankID"].ToString();
                                item.BankCode = table.Rows[r]["BankCode"].ToString();
                                item.CreateIP = table.Rows[r]["CreateIP"].ToString();
                                item.UserID = table.Rows[r]["UserID"].ToString();
                                item.WithdrawInfoData = table.Rows[r]["WithdrawData"].ToString();
                                lstConvertLog.Add(item);
                            }
                        }
                    }
                    catch (Exception e)
                    {                      
                        WriteLine($"GetConvertBankLog exception:{e}", ConsoleColor.Red);
                    }
                }
            }

            return lstConvertLog;
        }


        /// <summary>
        /// 第三方银行代码
        /// </summary>
        /// <returns></returns>
        public static List<ConvertBankCode> GetConvertBankCode()
        {
            List<ConvertBankCode> lstConvertBankCode = new List<ConvertBankCode>();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = "CenterDB.dbo.PrWeb_Pay_GetConvertBankCode";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@intGWID", ConvertGWID);                      

                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "log");

                        DataTable table = ds.Tables[0];

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table.Rows.Count; r++)
                            {
                                ConvertBankCode item = new ConvertBankCode();
                                item.GWID = Convert.ToInt32(table.Rows[r]["GWID"].ToString());
                                item.Code = table.Rows[r]["Code"].ToString();
                                item.TradeCode = table.Rows[r]["TradeCode"].ToString();
                                item.Status = Convert.ToInt32(table.Rows[r]["Status"].ToString());
                                lstConvertBankCode.Add(item);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLine($"GetConvertBankCode exception:{e}", ConsoleColor.Red);
                    }
                }
            }

            return lstConvertBankCode;
        }

        /// <summary>
        /// 更新返回的結果
        /// </summary>
        /// <param name="status">兌換狀態</param>
        /// <param name="orderId">兌換單號</param>
        /// <param name="transId">第三方單號</param>
        /// <param name="transData">第三方訊息</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool UpdateConvertLogStatus(int status, int orderId, string transId, string transData, ref string message)
        {
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = $"[LogDB].[dbo].[PrWeb_Money_WithdrawCallback]";

                SqlParameter chvErrMsg = new SqlParameter("@chvErrMsg", message);
                chvErrMsg.Direction = ParameterDirection.Output;
                chvErrMsg.SqlDbType = SqlDbType.VarChar;
                chvErrMsg.Size = 128;

                SqlParameter[] paras = {
                                    new SqlParameter("@intStatus",(object)status ?? DBNull.Value),
                                    new SqlParameter("@chvTransId",(object)transId ?? DBNull.Value),
                                    new SqlParameter("@chvTransData",(object)transData ?? DBNull.Value),
                                    new SqlParameter("@intOrderId", (object)orderId ?? DBNull.Value),
                                    chvErrMsg
                                   };

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(paras);
                        int returnValue = cmd.ExecuteNonQuery();
                        message = Convert.ToString(chvErrMsg.Value);

                        return returnValue == 1;
                    }
                    catch (Exception e)
                    {
                        string err = $"UpdateConvertLogStatus exception:{e}";   
                        WriteLine(err, ConsoleColor.Red);
                        return false;
                    }

                }
            }
        }

        public static int HttpPost(string httpUrl, string param, ref string responseData)
        {
            return HttpPost(httpUrl, param, null, ref responseData);
        }

        public static int HttpPost(string httpUrl, string param, Dictionary<string, string> headers, ref string responseData)
        {
            return HttpRequest(httpUrl, param, "POST", "application/x-www-form-urlencoded", null, ref responseData);
        }

        public static int HttpPostJson(string httpUrl, string param, ref string responseData)
        {
            return HttpPostJson(httpUrl, param, null, ref responseData);
        }

        public static int HttpPostJson(string httpUrl, string param, Dictionary<string, string> headers, ref string responseData)
        {
            return HttpRequest(httpUrl, param, "POST", "application/json", headers, ref responseData);
        }

        public static int HttpGet(string httpUrl, ref string responseData)
        {
            return HttpGet(httpUrl, "application/x-www-form-urlencoded", ref responseData);
        }

        public static int HttpGet(string httpUrl, string contentType, ref string responseData)
        {
            return HttpRequest(httpUrl, null, "GET", contentType, null, ref responseData);
        }

        static int HttpRequest(string httpUrl, string param, string method , string contentType, Dictionary<string, string> headers, ref string responseData)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = null;
            int returnCode = 0;

            try
            {
                request = HttpWebRequest.Create(httpUrl) as HttpWebRequest;
                request.Method = method;                
                request.ContentType = contentType;

                if (headers != null)
                {
                    foreach(var hheader in headers)
                        request.Headers.Add(hheader.Key, hheader.Value);
                }

                if (!string.IsNullOrEmpty(param))
                {
                    byte[] bs = Encoding.UTF8.GetBytes(param);

                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                        reqStream.Flush();
                    }
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), encoding);
                    responseData = sr.ReadToEnd();
                    sr.Close();
                    returnCode = (int)response.StatusCode;
                }
            }
            catch (Exception e)
            {
                responseData = e.Message;
                returnCode = 500;
            }
            finally
            {
                if (request != null)
                    request.Abort();

                request = null;
            }
            return returnCode;
        }                

        public static void WriteLine(string msg, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor bckgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = bckgroundColor;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {msg}");
            Console.ResetColor();

            FileLog(msg, fileLogName);
        }

        private static object logSyncObject = new object();

        public static void FileLog(string message, string filelog)
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

        #region 加解密函数

        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(string str)
        {
            return Md5(str, Encoding.UTF8);
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ec"></param>
        /// <returns></returns>
        public static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        /// <summary>  
        /// AES加密算法  
        /// </summary>  
        public static string AESEncrypt(string text, string key)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(text);
            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);

            var transform = aes.CreateEncryptor();
            return Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
        }

        /// <summary>
        /// SHA1雜湊演算法
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ec"></param>
        /// <returns></returns>
        public static string SHA1(string str, Encoding ec)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] source = ec.GetBytes(str);
            byte[] crypto = sha1.ComputeHash(source);

            return BitConverter.ToString(crypto).Replace("-", "");
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        public static string AESDecrypt(string text, string key)
        {
            var encryptBytes = Convert.FromBase64String(text);
            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);

            var transform = aes.CreateDecryptor();
            return Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));
        }

        /// <summary>
        /// RSA公钥格式转换，java->.net
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        /// <summary>
        /// RSA私钥格式转换，java->.net
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        /// <summary>
        /// RSA加密算法  
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string publicKey, string content)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(publicKey);

            int bufferSize = (rsa.KeySize / 8) - 11;
            byte[] buffer = new byte[bufferSize];
            //分段加密
            using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream ouput = new MemoryStream())
            {
                while (true)
                {
                    int readLine = input.Read(buffer, 0, bufferSize);
                    if (readLine <= 0)
                    {
                        break;
                    }
                    byte[] temp = new byte[readLine];
                    Array.Copy(buffer, 0, temp, 0, readLine);
                    byte[] encrypt = rsa.Encrypt(temp, false);
                    ouput.Write(encrypt, 0, encrypt.Length);
                }
                return Convert.ToBase64String(ouput.ToArray());
            }
        }

        /// <summary>
        /// RSA數位簽章
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncryptSignData(string publicKey, string content)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(publicKey);


            int bufferSize = (rsa.KeySize / 8) - 11;
            byte[] buffer = new byte[bufferSize];
            //分段加密
            using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream ouput = new MemoryStream())
            {
                while (true)
                {
                    int readLine = input.Read(buffer, 0, bufferSize);
                    if (readLine <= 0)
                    {
                        break;
                    }
                    byte[] temp = new byte[readLine];
                    Array.Copy(buffer, 0, temp, 0, readLine);
                    //byte[] encrypt = rsa.Encrypt(temp, false);

                    byte[] encrypt = rsa.SignData(temp, CryptoConfig.MapNameToOID("SHA256"));

                    ouput.Write(encrypt, 0, encrypt.Length);
                }
                return Convert.ToBase64String(ouput.ToArray());
            }
        }

        /// <summary>
        /// RSA解密 
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="encryptedContent"></param>
        /// <returns></returns>
        public static string RSADecrypt(string privateKey, string encryptedContent)
        {
            RSACryptoServiceProvider rsa = CreateRsaProviderFromPrivateKey(privateKey);

            var decryptString = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedContent), false));

            return decryptString;
        }

        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider(2048);
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        #endregion
    }
}
