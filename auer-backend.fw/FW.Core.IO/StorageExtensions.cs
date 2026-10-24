using FW.Core.Helper;
using System;
using example.invalid;
using System.Web;

namespace example.invalid
{
    public static class StorageExtensions
    {
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="file"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static string SaveFile(this IStorageFile storage, FileInfo file, string owner)
        {
            var fs = file.OpenRead();
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            return storage.SaveFile(data, file.Extension, owner);
          
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="file"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string SaveFile(this IStorageFile storage, HttpPostedFile file, string group)
        {
            try
            {
                byte[] data = new byte[file.ContentLength];
                file.InputStream.Read(data, 0, data.Length);

                return storage.SaveFile(data, IOHelper.GetFileExtensions(file.FileName), group);
            }
            catch (System.Exception ex)
            {
                throw new StorageException("保存文件失败", ex);
            }
            finally
            {
                file.InputStream.Dispose();
                file = null;
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="file"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string SaveFile(this IStorageFile storage, HttpPostedFileBase file, string group)
        {
            try
            {
                byte[] data = new byte[file.ContentLength];
                file.InputStream.Read(data, 0, data.Length);

                return storage.SaveFile(data, IOHelper.GetFileExtensions(file.FileName), group);
            }
            catch (System.Exception ex)
            {
                throw new StorageException("保存文件失败", ex);
            }
            finally
            {
                file.InputStream.Dispose();
                file = null;
            }
        }


        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="localFilePath"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string SaveFile(this IStorageFile storage, string localFilePath, string group)
        {
            localFilePath = FW.Core.Helper.IOHelper.MapPath(localFilePath);

            FileInfo finfo = new FileInfo(localFilePath);

            if (!finfo.Exists)
            {
                throw new StorageException(String.Format("文件{0}不存在", localFilePath));
            }

            FileStream fs = null;
            try
            {
                fs = finfo.OpenRead();
                var data = new byte[finfo.Length];
                fs.Read(data, 0, data.Length);

                return storage.SaveFile(data, finfo.Extension ,group);
            }
            catch (System.Exception ex)
            {
                throw new StorageException("保存文件失败," + ex.Message, ex);
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
        }

        

    }
}
