using System;
using example.invalid;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FW.Core.Helper;
using MongoDB.Driver.Builders;


namespace example.invalid
{
    public class GridFSStorageFile : IStorageFile
    {
        private string _connectionString;
        private string _databaseName;
        private IFileNameGenerator _nameGenerator;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="nameGenerator"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        public GridFSStorageFile(IFileNameGenerator nameGenerator, string connectionString, string databaseName)
        {
            FW.Core.Helper.Check.NotEmpty<string>(connectionString, "connectionString");
            FW.Core.Helper.Check.NotEmpty<string>(databaseName, "databaseName");
            FW.Core.Helper.Check.NotEmpty<IFileNameGenerator>(nameGenerator, "nameGenerator");

            _connectionString = connectionString;
            _databaseName = databaseName;
            _nameGenerator = nameGenerator;
        }

        public string SaveFile(byte[] data, string fileExtension, string group)
        {
            MongoDatabase db = null;

            try
            {
                MongoGridFSCreateOptions option = new MongoGridFSCreateOptions();
                option.ContentType = IOHelper.GetContentType(fileExtension);
                option.UploadDate = DateTime.Now;
                BsonDocument doc = new BsonDocument();
                doc.Add("owner", group);
                option.Metadata = doc;

                db = GetMongoDatabase();
                string fileName = _nameGenerator.GenerateFileName(fileExtension);

                var gfinfo = db.GridFS.Upload(new MemoryStream(data), fileName, option);
                return gfinfo.Id.ToString();

            }
            catch (System.Exception ex)
            {
                throw new StorageException("上传文件失败", ex);
            }
            finally
            {
                db.Server.Disconnect();
            }

        }

        public bool DeleteFile(string id, string group)
        {
            MongoDatabase db = null;
            try
            {
                ObjectId oid = ObjectId.Parse(id);
                db = GetMongoDatabase();
                var query = Query.And(
                    Query.EQ("metadata.owner", group),
                    Query.EQ("_id", oid)
                    );

                var file = db.GridFS.FindOne(query);
                if (file == null)
                {
                    return false;
                }

                file.Delete();

                return true;
            }
            catch (System.Exception ex)
            {
                throw new StorageException("删除文件错误," + ex.Message, ex);
            }
        }

        public StorageFileInfo GetFile(string id, string group)
        {
            MongoDatabase db = null;
            try
            {
                db = GetMongoDatabase();

                ObjectId oid = ObjectId.Parse(id);
                db = GetMongoDatabase();
                var query = Query.And(
                    Query.EQ("metadata.owner", group),
                    Query.EQ("_id", oid)
                    );

                var gfinfo = db.GridFS.FindOne(query);
                var gfstream = gfinfo.OpenRead();
                byte[] data = new byte[gfinfo.Length];
                gfstream.Read(data, 0, data.Length);
                gfstream.Close();

                var fi = new StorageFileInfo();
                fi.UpdateTime = gfinfo.UploadDate;
                fi.FileName = gfinfo.Name;
                fi.Length = gfinfo.Length;
                fi.ContentType = gfinfo.ContentType;
                fi.Data = data;

                return fi;
            }
            catch (System.Exception ex)
            {
                throw new StorageException("获取文件失败," + ex.Message, ex);
            }
            finally
            {
                db.Server.Disconnect();
            }

        }

        public StorageInfo Stat(string group)
        {
            var stinfo = new StorageInfo();
            MongoDatabase db = null;
            try
            {
                db = GetMongoDatabase();

                db = GetMongoDatabase();
                var query = Query.EQ("metadata.owner", group);

                var gfinfo = db.GridFS.Find(query);

                var files =  db.GetCollection("fs.files").Find(query).SetFields("length");
                foreach (var f in files)
                {
                    stinfo.TotalSize += f.GetElement("length").Value.ToInt64();
                }
                stinfo.FileCount = gfinfo.Count();
               

                return stinfo;
            }
            catch (System.Exception ex)
            {
                throw new StorageException("获取统计信息错误," + ex.Message, ex);
            }
        }

        private MongoDatabase GetMongoDatabase()
        {
            MongoClient clt = new MongoClient(_connectionString);
            var server = clt.GetServer();
            var db = server.GetDatabase(_databaseName);

            return db;
        }

    }
}
