using System;
using System.Web;


namespace FW.Core.Helper
{
    public static class IOHelper
    {
        /// <summary>
        /// 获取物理路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MapPath(string path)
        {
            if (path.IndexOf("\\") != -1)
            {
                return path;
            }

            if (SystemEnvironment.IsWebContext)
            {
                return HttpContext.Current.Server.MapPath(path);
            }

            path = path.Replace("/", "\\");

            return System.AppDomain.CurrentDomain.BaseDirectory + path;

        }

        /// <summary>
        /// 检查目录路径，附加\\
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string CheckFolderPathEndWith(string folder)
        {
            if (!folder.EndsWith("\\"))
            {
                folder += "\\";
            }

            return folder;
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtensions(string fileName)
        {
            int pos = fileName.LastIndexOf(".");
            if (pos == -1)
            {
                throw new Exception("文件名不正确");
            }

            return fileName.Substring(pos);
        }

        /// <summary>
        /// 获取文件ContentType
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetContentType(string extension)
        {
            extension = extension.Replace(".", "");

            extension = extension.ToUpper();
            string contentType = "";

            if (extension == "*")
                contentType = "application/octet-stream";
            else if (extension == "323")
                contentType = "text/h323";
            else if (extension == "ACX")
                contentType = "application/internet-property-stream";
            else if (extension == "AI")
                contentType = "application/postscript";
            else if (extension == "AIF")
                contentType = "audio/x-aiff";
            else if (extension == "AIFC")
                contentType = "audio/x-aiff";
            else if (extension == "AIFF")
                contentType = "audio/x-aiff";
            else if (extension == "ASF")
                contentType = "video/x-ms-asf";
            else if (extension == "SR")
                contentType = "video/x-ms-asf";
            else if (extension == "SX")
                contentType = "video/x-ms-asf";
            else if (extension == "AU")
                contentType = "audio/basic";
            else if (extension == "AVI")
                contentType = "video/x-msvideo";
            else if (extension == "AXS")
                contentType = "application/olescript";
            else if (extension == "BAS")
                contentType = "text/plain";
            else if (extension == "BCPIO")
                contentType = "application/x-bcpio";
            else if (extension == "BIN")
                contentType = "application/octet-stream";
            else if (extension == "BMP")
                contentType = "image/bmp";
            else if (extension == "C")
                contentType = "text/plain";
            else if (extension == "CAT")
                contentType = "application/vnd.ms-pkiseccat";
            else if (extension == "CDF")
                contentType = "application/x-cdf";
            else if (extension == "CER")
                contentType = "application/x-x509-ca-cert";
            else if (extension == "CLASS")
                contentType = "application/octet-stream";
            else if (extension == "CLP")
                contentType = "application/x-msclip";
            else if (extension == "CMX")
                contentType = "image/x-cmx";
            else if (extension == "COD")
                contentType = "image/cis-cod";
            else if (extension == "CPIO")
                contentType = "application/x-cpio";
            else if (extension == "CRD")
                contentType = "application/x-mscardfile";
            else if (extension == "CRL")
                contentType = "application/pkix-crl";
            else if (extension == "CRT")
                contentType = "application/x-x509-ca-cert";
            else if (extension == "CSH")
                contentType = "application/x-csh";
            else if (extension == "CSS")
                contentType = "text/css";
            else if (extension == "DCR")
                contentType = "application/x-director";
            else if (extension == "DER")
                contentType = "application/x-x509-ca-cert";
            else if (extension == "DIR")
                contentType = "application/x-director";
            else if (extension == "DLL")
                contentType = "application/x-msdownload";
            else if (extension == "DMS")
                contentType = "application/octet-stream";
            else if (extension == "DOC" || extension == "DOCX")
                contentType = "application/msword";
            else if (extension == "DOT")
                contentType = "application/msword";
            else if (extension == "DVI")
                contentType = "application/x-dvi";
            else if (extension == "DXR")
                contentType = "application/x-director";
            else if (extension == "EPS")
                contentType = "application/postscript";
            else if (extension == "ETX")
                contentType = "text/x-setext";
            else if (extension == "EVY")
                contentType = "application/envoy";
            else if (extension == "EXE")
                contentType = "application/octet-stream";
            else if (extension == "FIF")
                contentType = "application/fractals";
            else if (extension == "FLR")
                contentType = "x-world/x-vrml";
            else if (extension == "GIF")
                contentType = "image/gif";
            else if (extension == "GTAR")
                contentType = "application/x-gtar";
            else if (extension == "GZ")
                contentType = "application/x-gzip";
            else if (extension == "H")
                contentType = "text/plain";
            else if (extension == "HDF")
                contentType = "application/x-hdf";
            else if (extension == "HLP")
                contentType = "application/winhlp";
            else if (extension == "HQX")
                contentType = "application/mac-binhex40";
            else if (extension == "HTA")
                contentType = "application/hta";
            else if (extension == "HTC")
                contentType = "text/x-component";
            else if (extension == "HTM")
                contentType = "text/html";
            else if (extension == "HTML")
                contentType = "text/html";
            else if (extension == "HTT")
                contentType = "text/webviewhtml";
            else if (extension == "ICO")
                contentType = "image/x-icon";
            else if (extension == "IEF")
                contentType = "image/ief";
            else if (extension == "III")
                contentType = "application/x-iphone";
            else if (extension == "INS")
                contentType = "application/x-internet-signup";
            else if (extension == "ISP")
                contentType = "application/x-internet-signup";
            else if (extension == "JFIF")
                contentType = "image/pipeg";
            else if (extension == "JPG")
                contentType = "image/jpeg";
            else if (extension == "JPEG")
                contentType = "image/jpeg";
            else if (extension == "PNG")
                contentType = "image/x-png";
            else if (extension == "TXT")
                contentType = "text/plain";
            else if (extension == "LOG")
                contentType = "text/plain";

            return contentType;
        }

        /// <summary>
        /// 获取目录路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFolder(string path)
        {
            return path.Substring(0, path.LastIndexOf("\\"));
        }

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            return path.Substring(path.LastIndexOf("\\"));
        }
    }
}
