using System;
using System.Collections.Generic;


namespace example.invalid
{
    public interface IStorageFile
    {
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileExtension"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        string SaveFile(byte[] data,string fileExtension, string group);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        bool DeleteFile(string fileName, string group);

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        StorageFileInfo GetFile(string fileName, string group);

        /// <summary>
        /// 统计存储信息
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        StorageInfo Stat(string group);

    }
}
