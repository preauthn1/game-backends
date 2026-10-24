using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers
{
    public class CommonController : BaseController
    {
        public static List<UploadConfig> uploadConfigs = new List<UploadConfig>() {
            new UploadConfig(){ Key="ad_img", Alllowsize=2048, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/AD/"},
            new UploadConfig(){ Key="goods_img", Alllowsize=0.029296875, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Goods/"},
            new UploadConfig(){ Key="matchlogo_img", Alllowsize=2048, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Match/"},
            new UploadConfig(){ Key="award_img", Alllowsize=2048, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Award/"},
            new UploadConfig(){ Key="article_img", Alllowsize=2048, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Article/"},
            new UploadConfig(){ Key="adzip_img", Alllowsize=2048, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Adzip/"},
            new UploadConfig(){ Key="renter_img", Alllowsize=50, AllowFileType="jpg,jpeg,png,gif", Path="/Upload/Renter/"},//2016-04-14 zft  修改 限制主播头像大小
        };
        [HttpPost]
        public ActionResult UMEditorUpLoadFile(string key = "article_img", string extendparas = null)
        {
            UploadFileData result = new UploadFileData();
            var config = uploadConfigs.FirstOrDefault(c => c.Key == key);
            if (config != null)
            {
                string filefullname = config.Path + Guid.NewGuid().ToString("N");
                if (extendparas != null)
                {
                    filefullname = string.Format(filefullname, extendparas.Split(','));
                }
                result = UploadFile(HttpContext, filefullname, config.AllowFileType.Split(','), config.Alllowsize);
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "非法上传";
            }
            dynamic uMEditorResult = new
            {
                state = result.Message,
                url = result.Url,
                originalName = result.OriginalName,
                name = result.FullFileName,
                size = result.Size,
                type = result.ExtName
            };

            Dictionary<string, string> dict_content = new Dictionary<string, string>();
            dict_content.Add("originalName", result.OriginalName);
            dict_content.Add("name", result.FullFileName);
            dict_content.Add("url", result.Url);
            dict_content.Add("size", result.Size.ToString());
            dict_content.Add("state", result.IsSuccess ? "SUCCESS" : result.Message);
            dict_content.Add("type", result.ExtName);

            string json = "";
            foreach (var c in dict_content)
            {
                json += c.Key + ":\"" + c.Value + "\"" + (c.Key == "type" ? "" : ",");
            }
            json = "{" + json + "}";

            string callback = Request["callback"];
            if (callback != null)
            {
                return Content(String.Format("<script>{0}({1});</script>", callback, json));
            }
            else
            {
                return Content(json);
            }
        }

        /// <summary>
        /// Post请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static  string PostData(string url, string param)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                Encoding encoding = Encoding.GetEncoding("GB2312");
                req.Timeout = 1000 * 10;

                byte[] bs = encoding.GetBytes(param);
                string responseData = String.Empty;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd().ToString();
                    }

                    //var code = new System.Text.RegularExpressions.Regex("(?<=\\[\"code\"\\]=)\\d+(?=,)").Match(responseData).Value;
                    //string msg = "";
                    //var msgRegx = new System.Text.RegularExpressions.Regex("(?<=\\[\"msg\"\\]=\")[^\"]*(?=\")");
                    //if (msgRegx.IsMatch(responseData))
                    //{
                    //    msg = msgRegx.Match(responseData).Value;
                    //}                    

                    return responseData;
                }
            }
            catch
            {
                return string.Empty;
            }
        }


        [HttpPost]
        public ActionResult UpLoadFile(string key, string extendparas = null, string id = null)
        {
            UploadFileData result = new UploadFileData();
            var config = uploadConfigs.FirstOrDefault(c => c.Key == key);
            if (config != null)
            {
                if (string.IsNullOrEmpty(id) == false)
                {
                    result = UploadFile(HttpContext, config.Path + id, config.AllowFileType.Split(','), config.Alllowsize);
                }
                else
                {
                    string filefullname = config.Path + Guid.NewGuid().ToString("N");
                    if (!string.IsNullOrWhiteSpace(extendparas))
                    {
                        filefullname = string.Format(filefullname, extendparas.Split(','));
                    }
                    result = UploadFile(HttpContext, filefullname, config.AllowFileType.Split(','), config.Alllowsize);
                }
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "非法上传";
            }
            return Json(result);
        }

        /// <summary>
        /// 上传文件方法
        /// </summary>
        /// <param name="context">HttpContextBase对象</param>
        /// <param name="filename">不含扩展名的相对路径文件名,如:/upload/logo</param>        
        /// <param name="allowfiletype">允许的文件类型</param>
        /// <param name="allowsize">允许上传文件的大小,单位KB,默认为10MB</param>
        /// <returns></returns>
        private static UploadFileData UploadFile(HttpContextBase context, string filename, string[] allowfiletype, Double allowsize = 1024*10)
        {
            UploadFileData data = new UploadFileData();
            try
            {
                HttpPostedFileBase uploadFile = context.Request.Files[0];


                //获取扩展名
                data.ExtName = uploadFile.FileName.IndexOf(".") == -1 ? "" : Regex.Replace(uploadFile.FileName, @".*(?=\.[^$]+)", "");
                data.FileName = string.IsNullOrWhiteSpace(data.ExtName) ? uploadFile.FileName : Regex.Match(uploadFile.FileName, @".*(?=\.[^$]+)").Value;
                data.OriginalName = uploadFile.FileName;
                data.Size = uploadFile.ContentLength;
                if (!allowfiletype.Contains(data.ExtName.ToLower().Substring(1)))
                {
                    data.IsSuccess = false;
                    data.Message = "上传文件的类型不正确";
                    return data;
                }

                //检查文件大小是否超过
                if (uploadFile.ContentLength > allowsize * 1024)
                //if (uploadFile.ContentLength > allowsize * 1024 * 1024)
                {
                    data.IsSuccess = false;
                    data.Message = "上传文件大小不正确";
                    return data;
                }


                //获取路径
                //string path = context.Server.MapPath(Regex.Match(filename, @".*(?=\/[^$]+)/", RegexOptions.IgnoreCase).Value);
                string path = ConfigData.UploadFolder + Regex.Match(filename, @".*(?=\/[^$]+)/", RegexOptions.IgnoreCase).Value;
                //检查路径是否存在，不存在则创建
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                //uploadFile.SaveAs(context.Server.MapPath(filename + data.ExtName));
                uploadFile.SaveAs(ConfigData.UploadFolder + filename + data.ExtName);


                ////同步上传文件到FTP服务器
                ////FTP服务器设置网站的upload目录
                //try
                //{
                //    var ftpConfig = ConfigData.FtpConfig.Split(',');
                //    Ftp ftpClient = new Ftp(ftpConfig[0], ftpConfig[1], ftpConfig[2]);
                //    ftpClient.upload(filename.ToLower().Replace("/upload","") + data.ExtName, ConfigData.UploadFolder + filename + data.ExtName);


                //}
                //catch(Exception ex) { 
                
                //}


                data.Url = ConfigData.UploadFileBaseUrl + filename + data.ExtName;
                data.IsSuccess = true;
            }
            catch(Exception ex)
            {
                data.Message = ex.Message;
                data.IsSuccess = false;
            }
            //加上虚拟路径
            return data;
        }
    }

    public class UploadConfig
    {
        /// <summary>
        /// 配置的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 路径,可以用Format的格式化路径,如:/goods/{0}/{1}/
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 允许上传文件类型(配置格式:".jpg,.gif,.png")
        /// </summary>
        public string AllowFileType { get; set; }
        /// <summary>
        /// 允许文件大小单位KB
        /// </summary>
        public Double Alllowsize { get; set; }

    }
    public class UploadFileData
    {
        /// <summary>
        /// 是否上传成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 相对路径(含文件名)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 完整文件名(含扩展名)
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// 文件名(不含扩展名)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string ExtName { get; set; }

        /// <summary>
        /// 原文件名
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// 文件大小(字节数)
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 扩展信息
        /// </summary>
        public dynamic ExtendInfo { get; set; }
    }


    public class Ftp
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        /* Construct Object */
        public Ftp(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

        /* Download File */
        public void download(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Get the FTP Server's Response Stream */
                ftpStream = ftpResponse.GetResponseStream();
                /* Open a File Stream to Write the Downloaded File */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Upload File */
        public void upload(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Delete File */
        public void delete(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Rename File */
        public void rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                /* Rename the File */
                ftpRequest.RenameTo = newFileName;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Create a New Directory on the FTP Server */
        public void createDirectory(string newDirectory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Get the Date/Time a File was Created */
        public string getFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { fileInfo = ftpReader.ReadToEnd(); }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Created Date Time */
                return fileInfo;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* Get the Size of a File */
        public string getFileSize(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Size */
                return fileInfo;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* List Directory Contents File/Folder Name Only */
        public string[] directoryListSimple(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] directoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }
    }
}