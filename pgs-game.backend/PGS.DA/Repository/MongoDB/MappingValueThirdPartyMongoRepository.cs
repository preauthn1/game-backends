using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace target.DA.Repository
{
    public class MappingValueThirdPartyMongoRepository : BaseMongoRepository<MongoDBMappingTagValuesEntity>
    {

        protected string CollectionName
        {
            get { return "ApiRequestLogDetailThirdParty"; }
        }


        public void Insert(MongoDBMappingTagValuesEntity insertData)
        {
            MongoDBHelper.InsertSingle(CollectionName, insertData);
        }


        public void InsertMany(List<MongoDBMappingTagValuesEntity> insertItems)
        {
            MongoDBHelper.InsertMany(CollectionName, insertItems);
        }



        public MongoFilterResult FilterUniqueIDByMongoDBSearch(QueryExternalApiRequestLogApiRequestParameterModel filter)
        {
            filter.UniqueIDs = filter.UniqueIDs ?? new List<string>();

            var notFoundResult = new MongoFilterResult
            {
                IsNotFoundData = true
            };


            if (!string.IsNullOrWhiteSpace(filter.RequestContentKeyword))
            {
                var myFilter = new MongoDBKeywordSearchParameter
                {
                    BeginTime = filter.CreateTimeBegin.Value,
                    EndTime = filter.CreateTimeEnd.Value,
                    ColumnName_Time = nameof(MongoDBMappingTagValuesEntity.CreateTime),
                    Keyword = filter.RequestContentKeyword,
                    ColumnName_KeywordMatch = new List<string> { "Detail.ReqeustBody" }
                };

                var mongoSearchResult = MongoDBHelper.QueryByKeyword<MongoDBMappingTagValuesEntity>(CollectionName, myFilter);

                if (mongoSearchResult.IsEmptyOrNull())
                    return notFoundResult;

                filter.UniqueIDs.AddRange(mongoSearchResult.Select(x => x.UniqueID).ToList());
            }

            if (!string.IsNullOrWhiteSpace(filter.ResponseTextKeyword))
            {
                var myFilter = new MongoDBKeywordSearchParameter
                {
                    BeginTime = filter.CreateTimeBegin.Value,
                    EndTime = filter.CreateTimeEnd.Value,
                    ColumnName_Time = nameof(MongoDBMappingTagValuesEntity.CreateTime),
                    Keyword = filter.RequestContentKeyword,
                    ColumnName_KeywordMatch = new List<string> { "Detail.ResponseContent" }
                };

                var mongoSearchResult = MongoDBHelper.QueryByKeyword<MongoDBMappingTagValuesEntity>(CollectionName, myFilter);

                if (mongoSearchResult.IsEmptyOrNull())
                    return notFoundResult;

                filter.UniqueIDs.AddRange(mongoSearchResult.Select(x => x.UniqueID).ToList());
            }

            return new MongoFilterResult
            {

            };
        }



        public void GetSettingsByUniqueID(string uniqueID)
        {
            // MongoDBHelper.InsertMany(CollectionName, insertItems);
        }


    }


}
