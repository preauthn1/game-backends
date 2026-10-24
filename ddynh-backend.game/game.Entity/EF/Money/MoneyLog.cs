using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{

    /// <summary>
    /// 金币记录
    /// </summary>
     [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class MoneyLog : BaseEntity
    {
         /// <summary>
         /// 用户ID
         /// </summary>
         public int UserID { get; set; }

         /// <summary>
         /// 流通类型
         /// </summary>
         public int SourceTypeID { get; set; }

        /// <summary>
        /// 当前数量
        /// </summary>
        [MoneyInt]
         public long CurrentAmount { get; set; }

        /// <summary>
        /// 更新的数量
        /// </summary>
        [MoneyInt]
         public long UpdateAmount { get; set; }


        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 遊戲交易ID(用來查詢牌局編號)
        /// </summary>
        public long TradeID { get; set; }



        public virtual UserInfo User { get; set; }

        public virtual MoneySourceType SourceType { get; set; }

    }

    public class MoneyLogReport
    {
        public MoneyLogReport()
        {
            User = new UserInfo();
            SourceType = new MoneySourceType();
        }
        public int LogID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 流通类型
        /// </summary>
        public int SourceTypeID { get; set; }

        /// <summary>
        /// 当前数量
        /// </summary>
        [MoneyInt]
        public long CurrentAmount { get; set; }

        /// <summary>
        /// 更新的数量
        /// </summary>
        [MoneyInt]
        public long UpdateAmount { get; set; }

        /// <summary>
        /// 流水
        /// </summary>
        [MoneyInt]
        public long FlowAmount { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 遊戲交易ID(用來查詢牌局編號)
        /// </summary>
        public long TradeID { get; set; }

        public UserInfo User { get; set; }

        public MoneySourceType SourceType { get; set; }
    }

    /// <summary>
    /// 包括總流水計算及pageList :MoneyLogReport
    /// </summary>
    public class MoenyLogAddSumFlowAmountReport
    {
        public long SumFlowAmount { set; get; }

        public PagedList<MoneyLogReport> PageListMoneyLog {set;get;}
    }

    /// <summary>
    /// 玩家userInfo的頁面，金币紀錄顯示
    /// </summary>
    public class MoneyLogUserInfoPage
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID {get;set;}

        /// <summary>
        /// 流通类型
        /// </summary>
        public string SourceName {get;set;}

        /// <summary>
        /// 当前数量
        /// </summary>
        public string CurrentAmount {get;set;}

        /// <summary>
        /// 更新的数量
        /// </summary>
        public string UpdateAmount { get;set;}

        /// <summary>
        /// 当前数量 + 更新的数量
        /// </summary>
        public string TotalAmount {set;get;}

        /// <summary>
        /// 流水
        /// </summary>
        public string FlowAmount {get;set;}

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress {get;set;}

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo {get;set;}

        /// <summary>
        /// 创建时间DATETIME
        /// </summary>
        public DateTime CreateTimeStamp {get;set;}

        /// <summary>
        /// 创建时间STRING
        /// </summary>
        public string CreateTime
        {
            set { }
            get 
            {
                if (CreateTimeStamp == new DateTime())
                    return string.Empty;

                return CreateTimeStamp.ToString("yyyy/MM/dd HH:mm:ss"); 
            }
        }

        /// <summary>
        /// 遊戲交易ID(用來查詢牌局編號)
        /// </summary>
        public long TradeID { get; set; }
    }
}
