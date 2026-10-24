using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper;

namespace game.Infrastructure
{
    public static class Common
    {
        private static object logSyncObject = new object();
        public static void Log(string log, string fileName = "log.log")
        {
            lock (logSyncObject)
            {
                example.invalid.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "/log/" + fileName, log + Environment.NewLine);
            }

        }

        /// <summary>
        /// 获取分页总数
        /// </summary>
        /// <param name="pageSize">页大小</param>
        /// <param name="count">记录总数</param>
        /// <returns></returns>
        public static int GetTotalPage(int pageSize, int count)
        {
            if (pageSize < 1) return 0;
            if (count <= pageSize) return 1;
            int pageCount = count / pageSize;
            if (count % pageSize > 0) pageCount++;
            return pageCount;
        }
        /// <summary>
        /// 提交表单通过反射获取name组合
        /// </summary>
        /// <typeparam name="T">返回list</typeparam>
        /// <returns></returns>
        public static List<T> GetListByForm<T>() where T : new()
        {
            var t = SetList<T>(null, 0);
            return t;
        }
        private static List<T> SetList<T>(string appendstr, int index) where T : new()
        {
            var t = new List<T>();
            try
            {
                var properties = new T().GetType().GetProperties();
                var subNum = System.Web.HttpContext.Current.Request[appendstr + properties[index].Name].Split(',').Length;
                for (int i = 0; i < subNum; i++)
                {
                    var r = properties;
                    var model = new T();
                    foreach (var p in properties)
                    {
                        string pval = System.Web.HttpContext.Current.Request[appendstr + p.Name + ""];
                        if (!string.IsNullOrEmpty(pval))
                        {
                            pval = pval.Split(',')[i];
                            var pptypeName = p.PropertyType.Name;
                            p.SetValue(model, Convert.ChangeType(pval, p.PropertyType), null);
                        }
                    }
                    t.Add(model);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return t;
        }
        public static string base64(string code)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(code));
        }

        public static string md5(string code)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code, "md5");
        }

        public static string encrypt(string code)
        {
            return md5(base64(md5(base64(code))));
        }

    }
}
