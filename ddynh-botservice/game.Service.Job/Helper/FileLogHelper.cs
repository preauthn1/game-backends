using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using example.invalid;

namespace game.Service.Service
{
    public class FileLogHelper
    {
        private const string _bot_service_direction = @"D:\website\botServices";

        private const string _log_folder_common = "log_custom";

        private const string _log_folder_error = "log_error";

        private static object _logSyncLockObject = new object();


        public static void WriteFileLog(string message, string fileName)
        {
            lock (_logSyncLockObject)
            {
                string dir = GetWriteFilePath();

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var path = dir + fileName;

                WriteFileToTargetPath(message, path);
            }
        }


        public static void WriteErrorLog(string message, string fileName)
        {
            lock (_logSyncLockObject)
            {
                string dir = GetWriteFilePath(_log_folder_error);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var path = dir + fileName;

                WriteFileToTargetPath(message, path);
            }
        }



        private static void WriteFileToTargetPath(string message, string path)
        {
            StreamReader sr;

            if (!File.Exists(path))
            {
                sr = new StreamReader(File.Create(path));
            }
            else
            {
                sr = new StreamReader(path);
            }

            using (sr)
            {
                sr.Dispose();
            }

            example.invalid.File.AppendAllText(path, message + Environment.NewLine);
        }


        /// <summary>
        /// 根據是否有D槽website資料夾，來決定是否為VM執行環境
        /// </summary>
        /// <returns></returns>
        public static bool IsOnVirtualMachine()
        {
            return Directory.Exists(_bot_service_direction);
        }


        public static string GetWriteFilePath(string folderName = _log_folder_common)
        {
            var dirpath = string.Format("/{0}/{1}/", folderName , DateTime.Now.ToString("yyyyMMdd"));

            if(IsOnVirtualMachine())
            {
                return _bot_service_direction + dirpath;
            }

            return System.Environment.CurrentDirectory + dirpath;           

        }


    }

}
