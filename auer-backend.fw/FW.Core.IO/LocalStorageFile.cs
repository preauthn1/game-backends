using System;
using example.invalid;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FW.Core.Helper;

namespace example.invalid
{
    public class LocalStorageFile : IStorageFile
    {
        private string _folderPath;
        private IFileNameGenerator _nameGenerator;

        public LocalStorageFile(IFileNameGenerator nameGenerator, string folder = "upload")
        {
            FW.Core.Helper.Check.NotEmpty<string>(folder, "folder");
            FW.Core.Helper.Check.NotEmpty<IFileNameGenerator>(nameGenerator, "nameGenerator");

            _folderPath = FW.Core.Helper.IOHelper.MapPath(folder);
            _nameGenerator = nameGenerator;
        }

        public string SaveFile(byte[] data, string fileExtension, string group)
        {
            if(!fileExtension.StartsWith("."))
            {
                fileExtension = "." + fileExtension;
            }

            FileStream fs = null;
            string fileName = "";
            try
            {
                fileName = _nameGenerator.GenerateFileName(fileExtension);
                string filePath = getFileFullName(fileName, group);
                fs = new FileStream(filePath, FileMode.CreateNew);
                fs.Write(data, 0, data.Length);
            }
            catch (System.Exception ex)
            {
                throw new StorageException("存储文件错误," + ex.Message, ex);
            }
            finally
            {
                fs.Close();
            }

            return fileName;
        }

        public bool DeleteFile(string fileName, string group)
        {
            string filePath = getFileFullName(fileName, group);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (System.Exception ex)
                {
                    throw new StorageException("删除文件错误," + ex.Message, ex);
                }
                    
                return true;
            }

            return false;

        }

        public StorageFileInfo GetFile(string fileName, string group)
        {
            FileStream fs = null;
            try
            {
                var fi = new FileInfo(getFileFullName(fileName, group));
                fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);

                var finfo = new StorageFileInfo();
                finfo.ContentType = IOHelper.GetContentType(fileName);
                finfo.Data = data;
                finfo.Length = data.Length;
                finfo.FileName = fileName;
                finfo.UpdateTime = fi.LastAccessTime;

                return finfo;
            }
            catch (System.Exception ex)
            {
                throw new StorageException("获取文件错误," + ex.Message, ex);
            }
            finally
            {
                fs.Close();
            }
        }

        public StorageInfo Stat(string group)
        {
            DirectoryInfo dinfo = new DirectoryInfo(getDirectoryName(group));
            var sinfo = new StorageInfo();
            sinfo.FileCount = dinfo.GetFiles().Length;

            dinfo.GetFiles().ToList().ForEach(f => {
                sinfo.TotalSize += f.Length;
            });

            return sinfo;
        }


        private string getDirectoryName(string group)
        {
            string dirPath = String.Format("{0}\\{1}", _folderPath, group);
            DirectoryInfo di = new DirectoryInfo(dirPath);
            if (!di.Exists)
            {
                di.Create();
            }

            return di.FullName;
        }

        private string getFileFullName(string fileName, string group)
        {
            if (fileName.IndexOf("\\") != -1)
            {
                return fileName;
            }

            return String.Format("{0}\\{1}", getDirectoryName(group), fileName);
        }
    }
}
