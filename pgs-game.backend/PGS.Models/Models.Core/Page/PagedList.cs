using Newtonsoft.Json;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Models.Paging
{
    public class PagedList<TEntity>
    {
        public PagedList()
        {
            Items = new List<TEntity>();
            this.PageIndex = 1;
            this.PageSize = 1;
            this.TotalCount = 0;
            this.TotalPage = 0;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalPage"></param>
        public PagedList(IEnumerable<TEntity> data, int pageIndex, int pageSize, int totalCount, object ouputData = null)
        {
            Items = new List<TEntity>();

            if (data == null)
            {
                throw new ArgumentException("参数data不能为空");
            }
            if (pageIndex < 1)
            {
                throw new ArgumentException("pageIndex必须为大于1的数字");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize必须为大于1的数字");
            }


            Items.AddRange(data);

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            this.TotalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalCount) / PageSize));
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public PagedList(IQueryable<TEntity> data, int pageIndex, int pageSize)
        {
            Items = new List<TEntity>();
            if (data == null)
            {
                throw new ArgumentException("参数data不能为空");
            }
            if (pageIndex < 1)
            {
                throw new ArgumentException("pageIndex必须为大于1的数字");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize必须为大于1的数字");
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            TotalCount = data.Count();
            TotalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalCount) / PageSize));
            if (TotalCount > 0 && pageIndex > TotalPage)
            {
                throw new ArgumentException("pageIndex不能超过" + TotalPage.ToString());
            }

            if (pageIndex > 1)
            {
                data = data.Skip((pageIndex - 1) * pageSize);
            }

            Items.AddRange(data
                .Take(pageSize)
                .ToList());
        }

        public List<TEntity> Items { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页数据行数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总数据数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPage { get; set; }



        /// <summary>
        /// 额外的擴充欄位 (如果有其他的額外數值要帶)
        /// </summary>
        [SwaggerIgnoreOutputWhenPagedList]
        public object ExtraObject { get; set; }


        [JsonIgnore]
        public object DebugInfo { get; set; }


        /// <summary>
        /// 建議搭配 DateTimeHelper.GetDisplayServerTime();
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [SwaggerIgnoreOutputWhenPagedList]
        public string ServerTimeTW { get; set; }

    }


    public class PaginationParameter
    {
        /// <summary>
        /// 当前页码 (第幾頁)
        /// </summary>   
        [SwaggerApiRequiredField]
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页数据行数
        /// </summary>
        [SwaggerApiRequiredField]
        public int PageSize { get; set; }


        [SwaggerIgnore]
        public string OrderByColumn { get; set; }


        [SwaggerIgnore]
        public bool IsSkipCheckOrderByInjection { get; set; }


        /// <summary>
        /// 排序方向 asc或desc
        /// </summary>
        [SwaggerIgnore]
        public bool IsASC { get; set; }


        /// <summary>
        /// 特殊查詢才會使用到的參數
        /// </summary>
        public DbQueryConfig DbQueryExtraConfig { get; set; }

    }


    public class GenerateWhereSQLResult
    {
        public string WhereSQL { get; set; }

        public string TempIDsAppendSQL { get; set; }

        public string TempIDsSQL_2 { get; set; }
    }

 

    public class DbQueryConfig
    {

        /// <summary>
        /// 單位是'秒'
        /// </summary>
        [SwaggerIgnore]
        public int? ConnectTimeoutSeconds { get; set; }


        [SwaggerIgnore]
        public bool IsQueryByReplicationDB { get; set; }


        [SwaggerIgnore]
        public bool IsQueryByLogDB { get; set; }


        [SwaggerIgnore]
        public bool IsQueryByOldDataStorageDB { get; set; }


        [SwaggerIgnore]
        public bool IsForceConvertToDbValueType { get; set; }


        /// <summary>
        /// 需要自己搭配TempIDs語法查詢
        /// </summary>
        [SwaggerIgnore]
        public string TempIDsAppendSQL { get; set; }

    }


}
