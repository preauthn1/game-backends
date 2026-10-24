using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public abstract class FileResourceLoader : IResourceLoader
    {
        private string _folder;

        public FileResourceLoader(string folder)
        {
            _folder = folder;
        }

        public LanguageSettings Load()
        {
            //查找语言资源目录
            string folderPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + _folder;
            DirectoryInfo dInfo = new DirectoryInfo(folderPath);
            if (!dInfo.Exists)
            {
                return new LanguageSettings();
                //throw new LanguageException(String.Format("目录{0}不存在", folderPath));
            }

            //文件目录结构 folder -> module -> language
            var ls = new LanguageSettings();
            //查找资源分类目录
            Array.ForEach(dInfo.GetDirectories(), dRes => {
                var res = new LanguageResource();
                res.Name = dRes.Name;

                ls.Add(res.Name, res);

                //查找语言包目录
                Array.ForEach(dRes.GetFiles(), f =>
                {
                    var lp = loadResourceInternal(f.FullName);
                    lp.Name = f.Name.Replace(f.Extension, "");
                    res.Add(lp.Name, lp);
                });
            });

            return ls;
        }

        protected abstract LanguagePackage loadResourceInternal(string resPath);

    }

    public class XmlResourceLoader : FileResourceLoader
    {
        public XmlResourceLoader(string folder)
            :base(folder)
        {  }

        protected override LanguagePackage loadResourceInternal(string resPath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(resPath, FileMode.Open);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LanguagePackage));
                var lp = serializer.Deserialize(fs) as LanguagePackage;

                var finfo = new FileInfo(resPath);
                lp.Name = finfo.Name.Replace(finfo.Extension, "");
                return lp;
            }
            catch (System.Exception ex)
            {
                throw new LanguageException("解析语言包错误," + ex.Message, ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
    }

    public class JsonResourceLoader : FileResourceLoader
    {
        public JsonResourceLoader(string folder)
            : base(folder)
        { }

        protected override LanguagePackage loadResourceInternal(string resPath)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(resPath, System.Text.Encoding.UTF8);
                string data = reader.ReadToEnd();

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                var lp = serializer.Deserialize(data, typeof(LanguagePackage)) as LanguagePackage;
                var finfo = new FileInfo(resPath);
                lp.Name = finfo.Name.Replace(finfo.Extension, "");

                return lp;
            }
            catch (System.Exception ex)
            {
                throw new LanguageException("加载语言包错误," + ex.Message, ex);
            }
            finally
            {
                reader.Close();
            }
        }
    }

    

}
