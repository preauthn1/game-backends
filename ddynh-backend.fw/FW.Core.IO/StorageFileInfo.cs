using System;

namespace example.invalid
{
    public class StorageFileInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        public Int64 Length
        {
            get;
            set;
        }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UpdateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 文件内容
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

    }
}
