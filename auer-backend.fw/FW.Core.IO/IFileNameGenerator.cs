using System;
using FW.Core.Helper;


namespace example.invalid
{
    public interface IFileNameGenerator
    {
        /// <summary>
        /// 生成文件名
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        string GenerateFileName(string extension);
    }

    /// <summary>
    /// 默认的文件名生成器
    /// 根据当前时间，文件类型，GUID命名文件
    /// </summary>
    public class DefauleFileNameCreater : IFileNameGenerator
    {

        public string GenerateFileName(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return String.Format("{0}_{1}{2}",  Guid.NewGuid().ToString().Replace("-","").Substring(0,8), DateTime.Now.Ticks, extension);
        }
    }

}
