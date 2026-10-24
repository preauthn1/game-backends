using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace target.Utility.Helper
{

    public static class MongoDBHelper
    {
        private static readonly MongoClient _client;

        private static readonly IMongoDatabase _database;


        static MongoDBHelper()
        {
            _client = new MongoClient(SettingHelper.ThirdParty.MongoDBConn); // MongoDB 伺服器地址
            _database = _client.GetDatabase(SettingHelper.ThirdParty.MongoDBDBName); // 目標資料庫


            // 設置一個公約包 (Convention Pack)
            var pack = new ConventionPack();

            // 創建 IgnoreExtraElementsConvention 實例
            // 參數 true 表示：對於所有套用此公約的 Class，忽略文檔中的多餘欄位
            var ignoreExtraElementsConvention = new IgnoreExtraElementsConvention(true);

            // 將公約添加到公約包中
            pack.Add(ignoreExtraElementsConvention);

            // 將這個公約包註冊為全域公約
            // 參數 "IgnoreExtraElements" 是一個名稱，用於唯一標識這個公約包
            ConventionRegistry.Register(
                "IgnoreExtraElements", // 唯一的公約包名稱
                pack,
                type => true // 這個 Lambda 表達式定義了哪些 Class 應該套用這個公約
            );
        }


        public static IMongoCollection<BsonDocument> InstanceMongoCollection(string collection_name)
        {
            var collection = _database.GetCollection<BsonDocument>(collection_name); // 目標集合
            return collection;
        }


        public static void DeleteDataByID(string collectionName, string keyword)
        {
            BsonValue compare_bsonValue_delete = BsonValue.Create(keyword);

            var delete_filter = Builders<BsonDocument>.Filter
                 .Eq(x => x["id"], compare_bsonValue_delete);

            var collection = InstanceMongoCollection(collectionName);

            var queryExistData = collection.Find(_ => true).ToList();
            queryExistData = queryExistData ?? new List<BsonDocument>();

            var debug = queryExistData.Where(x => x["concate_name"].ToString().Contains("小刀尖")).ToList();

            collection.DeleteOne(delete_filter);
        }

        public static void InsertSingle<T>(string collectionName, T insertData) where T : class
        {
            InsertMany(collectionName, new List<T>
            {
                insertData
            });
        }

        public static void InsertMany<T>(string collectionName, List<T> listItems) where T : class
        {
            var collection = InstanceMongoCollection(collectionName);

            List<BsonDocument> insertItems = new List<BsonDocument>();

            foreach (var item in listItems)
            {
                var myDocument = item.ToBsonDocument();
                insertItems.Add(myDocument);
            }

            InsertMany(collectionName, insertItems);
        }


        public static void InsertMany(string collectionName, List<BsonDocument> listItems)
        {
            try
            {
                var collection = InstanceMongoCollection(collectionName);
                collection.InsertMany(listItems);
            }
            catch (Exception ex)
            {
                string msg = "MongoDBHelper InsertMany 錯誤 ," + ex.Message;
                MyTelegramUtility.SendMessage(new TelegramMessageParameter
                {
                    Message = msg,
                    TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                });
            }
        }

        public static List<T> QueryByKeyword<T>(string collectionName, MongoDBKeywordSearchParameter filter) where T : class
        {
            filter.RuleFor(x => x.BeginTime).Rule_StartTimeEndTimeRange(filter.EndTime, 8);
            filter.RuleFor(x => x.BeginTime).Rule_StartTimeMustSmallThanEndTime(filter.EndTime);

            var collection = MongoDBHelper.InstanceMongoCollection(collectionName);

            var regex = new Regex(filter.Keyword, RegexOptions.IgnoreCase);

            List<FilterDefinition<BsonDocument>> multipleFilter = new List<FilterDefinition<BsonDocument>>();

            foreach (var eachColumn in filter.ColumnName_KeywordMatch)
            {
                multipleFilter.Add(Builders<BsonDocument>.Filter.Regex(
                    eachColumn,
                    regex
                ));
            }
            var filter_date = Builders<BsonDocument>.Filter.Gte(filter.ColumnName_Time, filter.BeginTime) &
                 Builders<BsonDocument>.Filter.Lt(filter.ColumnName_Time, filter.EndTime);

            var filter1_or = Builders<BsonDocument>.Filter.Or(multipleFilter.ToArray());

            var finalFilter = Builders<BsonDocument>.Filter.And(
                filter_date,
                filter1_or
            );

            var results = collection.Find(finalFilter).ToList();

            return results.Select(x => BsonSerializer.Deserialize<T>(x)).ToList();
        }

        public static T QueryByUniqueID<T>(string collectionName, string uniqueID) where T : class
        {
            return null;
        }


        public static void Debug_CreateUniqueIndex(string collectionName)
        {
            var collection = InstanceMongoCollection(collectionName);

            collection.Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("UniqueID")
                )
            );

            collection.Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("CreateTime")
                )
            );


            //單鍵索引
            //collection.Indexes.CreateOne(
            //    new CreateIndexModel<BsonDocument>(
            //        Builders<BsonDocument>.IndexKeys.Ascending("PlayerName")
            //    )
            //);

            //// 複合索引
            //collection.Indexes.CreateOne(
            //    new CreateIndexModel<BsonDocument>(
            //        Builders<BsonDocument>.IndexKeys
            //            .Ascending("PlayerName")
            //            .Ascending("SettingKey")
            //    )
            //);
        }


    }


    public class MongoDBKeywordSearchParameter
    {
        public string Keyword { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public List<string> ColumnName_KeywordMatch { get; set; }

        public string ColumnName_Time { get; set; }
    }


    public class MongoFilterResult
    {
        public bool IsNotFoundData { get; set; }

    }



}