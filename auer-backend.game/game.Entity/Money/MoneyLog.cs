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
         public int UserID
         {
             get;
             set;
         }

         /// <summary>
         /// 流通类型
         /// </summary>
         public int SourceTypeID
         {
             get;
             set;
         }

         /// <summary>
         /// 当前数量
         /// </summary>
         [MoneyInt]
         public Int64 CurrentAmount
         {
             get;
             set;
         }

         /// <summary>
         /// 更新的数量
         /// </summary>
         [MoneyInt]
         public Int64 UpdateAmount
         {
             get;
             set;
         }


         /// <summary>
         /// IP地址
         /// </summary>
         public string IPAddress
         {
             get;
             set;
         }

         /// <summary>
         /// 说明
         /// </summary>
         public string Memo
         {
             get;
             set;
         }

         /// <summary>
         /// 创建时间
         /// </summary>
         public DateTime CreateTime
         {
             get;
             set;
         }


        /// <summary>
        /// 遊戲交易ID(用來查詢牌局編號)
        /// </summary>
        public Int64 TradeID
        {
            get;
            set;
        }



        public virtual UserInfo User
         {
             get;
             set;
         }

         public virtual MoneySourceType SourceType
         {
             get;
             set;
         }

    }

     public class MoneyLogReport 
     {
         public MoneyLogReport() {
             User = new UserInfo();
             SourceType = new MoneySourceType();
         }
         public int LogID { get; set; }

         /// <summary>
         /// 用户ID
         /// </summary>
         public int UserID
         {
             get;
             set;
         }

         /// <summary>
         /// 流通类型
         /// </summary>
         public int SourceTypeID
         {
             get;
             set;
         }

         /// <summary>
         /// 当前数量
         /// </summary>
         [MoneyInt]
         public Int64 CurrentAmount
         {
             get;
             set;
         }

         /// <summary>
         /// 更新的数量
         /// </summary>
         [MoneyInt]
         public Int64 UpdateAmount
         {
             get;
             set;
         }


        /// <summary>
        /// 流水
        /// </summary>
        [MoneyInt]
        public Int64 FlowAmount
        {
            get;
            set;
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress
         {
             get;
             set;
         }

         /// <summary>
         /// 说明
         /// </summary>
         public string Memo
         {
             get;
             set;
         }

         /// <summary>
         /// 创建时间
         /// </summary>
         public DateTime CreateTime
         {
             get;
             set;
         }

        /// <summary>
        /// 遊戲交易ID(用來查詢牌局編號)
        /// </summary>
        public Int64 TradeID
        {
            get;
            set;
        }

         public UserInfo User
         {
             get;
             set;
         }

         public MoneySourceType SourceType
         {
             get;
             set;
         }

     }

}
