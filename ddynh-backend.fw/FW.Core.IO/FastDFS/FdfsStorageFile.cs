using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace example.invalid.FastDFS
{
    public class FdfsStorageFile : IStorageFile
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trackerServerIPList"></param>
        public FdfsStorageFile(string trackerServerIPList)
        {
            string[] ips = trackerServerIPList.Split(new char[]{','});

            if (ips == null || ips.Length == 0)
            {
                throw new StorageException("tracker server error");
            }

            var ipLst = new List<IPEndPoint>();
            ips.ToList().ForEach(ip => {
                string[] ipArr = ip.Split(new char[]{':'});
                if (ipArr == null)
                {
                    return;
                }

                var ipe = new IPEndPoint(IPAddress.Parse(ipArr[0]), Convert.ToInt32(ipArr[1]));
                ipLst.Add(ipe);
            });

            if (ipLst.Count > 0)
            {
                ConnectionManager.Initialize(ipLst);
            }
            else
            {
                throw new StorageException("tracker server is required");
            }

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trackerServers"></param>
        public FdfsStorageFile(List<IPEndPoint> trackerServers)
        {
            if (trackerServers == null || trackerServers.Count == 0)
            {
                throw new StorageException("tracker server is required");
            }

            ConnectionManager.Initialize(trackerServers);
        }

        public string SaveFile(byte[] data, string fileExtension, string group)
        {
            if (fileExtension.StartsWith("."))
            {
                fileExtension = fileExtension.Replace(".", "");
            }

            if (String.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentNullException("group");
            }

            StorageNode node = FastDFSClient.GetStorageNode(group);
            string fileName = FastDFSClient.UploadFile(node, data, fileExtension);
            return fileName;
        }

        public bool DeleteFile(string fileName, string group)
        {
            FastDFSClient.RemoveFile(group, fileName);
            return true;
        }

        public StorageFileInfo GetFile(string fileName, string group)
        {
            StorageNode node = FastDFSClient.GetStorageNode(group);
            byte[] buffer = FastDFSClient.DownloadFile(node, fileName, 0L, 0L);

            return new StorageFileInfo()
            {
                FileName = fileName,
                Data = buffer,
                Length = buffer.Length,
                ContentType = IOHelper.GetContentType(fileName),
                UpdateTime = DateTime.MinValue
            };
        }

        public StorageInfo Stat(string group)
        {
            return new StorageInfo()
            {
                FileCount = -1,
                TotalSize = -1
            };
        }
    }
}
