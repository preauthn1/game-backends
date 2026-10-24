using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using target.Utility;

namespace target.API.File.Helper
{
    public class MinioStorageHelper
    {

        private static string _endPoint = SettingHelper.FileApiSetting.MinioApiEndPoint;
        private static string _accessKey = SettingHelper.FileApiSetting.MinioApiAccessKey;
        private static string _secretKey = SettingHelper.FileApiSetting.MinioApiSecretKey;
        private static string _bucketName = SettingHelper.FileApiSetting.MinioApiBucket;
        private static string _imageUrl = SettingHelper.FileApiSetting.MinioImageUrl;
        private static bool _isSSL = SettingHelper.FileApiSetting.IsMinioUseSSL;


        public static async Task<string> UploadFileToRemote(MemoryStream stream)
        {
            IMinioClient minio = new MinioClient()
                                   .WithEndpoint(_endPoint)
                                   .WithCredentials(_accessKey, _secretKey)
                                   .WithSSL(_isSSL)
                                   .Build();

            string fileName = Guid.NewGuid() + ".png";

            stream.Position = 0;

            var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithObjectSize(stream.Length)
                    .WithStreamData(stream)
                    .WithContentType("image/png");

            await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(true);

            string imageUrl = $"{_imageUrl}/{_bucketName}/{fileName}";
            return imageUrl;
        }

    }
}
