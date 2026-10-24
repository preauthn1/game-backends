using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Utility.Helper.Elasticsearch.Model
{

    public class ESSearchAPIResponse<T> where T : class
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public ESHit<T> hits { get; set; }
    }


    public class ESHit<T>
    {
        public string _index { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public List<string> _ignored { get; set; }
        public double max_score { get; set; }
        public List<ESHitItem<T>> hits { get; set; }
    }


    public class ESHitItem<T>
    {
        public string _index { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public T _source { get; set; }
    }

    public class ESGetEachApiErrorCountResponse
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public ESGetEachApiErrorCountHits hits { get; set; }
        public ESGetEachApiErrorCountAggregations aggregations { get; set; }
    }

    public class ESGetEachApiErrorCountHits
    {
        public ESTotalHits total { get; set; }
        public double? max_score { get; set; }
        public List<object> hits { get; set; }
    }

    public class ESTotalHits
    {
        public long value { get; set; }
        public string relation { get; set; }
    }

    public class ESGetEachApiErrorCountAggregations
    {
        public ESApiErrorCountAggregation group_by_category { get; set; }
    }

    public class ESApiErrorCountAggregation
    {
        public long doc_count_error_upper_bound { get; set; }
        public long sum_other_doc_count { get; set; }
        public List<ESApiErrorCountBucket> buckets { get; set; }
    }

    public class ESApiErrorCountBucket
    {
        public string key { get; set; }
        public long doc_count { get; set; }
    }

    public class ESGetEachHourCountIn48HoursResponse
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public ESGetEachHourCountIn48HoursHits hits { get; set; }
        public ESGetEachHourCountIn48HoursAggregations aggregations { get; set; }
    }

    public class ESGetEachHourCountIn48HoursHits
    {
        public ESTotalHits total { get; set; }
        public double? max_score { get; set; }
        public List<object> hits { get; set; }
    }

    public class ESGetEachHourCountIn48HoursAggregations
    {
        public ESHourlyCountAggregation hourly_counts { get; set; }
    }

    public class ESHourlyCountAggregation
    {
        public List<ESHourlyCountBucket> buckets { get; set; }
    }

    public class ESHourlyCountBucket
    {
        public string key_as_string { get; set; }
        public long key { get; set; }
        public long doc_count { get; set; }
    }

    public class IndexColumnMappingDefine
    {
        public string ColumnName { get; set; }

        public string DataType { get; set; }

        /// <summary>
        /// 常用時間格式是: strict_date_optional_time||epoch_millis
        /// </summary>
        public string Format { get; set; }
    }


    public class IndexColumnMappingDefineV2
    {
        public IndexColumnMappingDefineV2(string myType)
        {
            this.type = myType;
        }

        public string type { get; set; }

    }

    public class ESKeywordSearchBasicParameter
    {

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public string IndexName { get; set; }

        public int PageSize { get; set; }


        /// <summary>
        /// 最常指定 RequestContent、ResponseText、RequestHeader
        /// </summary>
        public List<string> ExcludeColumns { get; set; }

        public ESKeywordAdvanceFilterParameter AdvanceParameter { get; set; }

    }


    public class ESKeywordAdvanceFilterParameter
    {

        /// <summary>
        /// 若有特定指定。這邊的條件一定會帶入Must查詢
        /// </summary>
        public List<ESKeywordSearchParameter> AlwaysFilter { get; set; }

        public object ShouldQuery { get; set; }

    }




    public class ESKeywordSearchParameter
    {
        public ESKeywordSearchParameter ()
        {

        }

        public ESKeywordSearchParameter(string myColumn , string myKeyword)
        {
            this.ColumnName = myColumn;
            this.Keyword = myKeyword;
        }

        public string ColumnName { get; set; }

        public string Keyword { get; set; }

        /// <summary>
        /// 大於等於
        /// </summary>
        public int? gte { get; set; }

        /// <summary>
        /// 小於等於
        /// </summary>
        public int? lte { get; set; }


        /// <summary>
        /// 若有指定物件。強制使用該物件來查詢
        /// </summary>
        public object ForceObject { get; set; }
    }


}
