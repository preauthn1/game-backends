using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneylogExportExcel
{
    public class MoneyLogReport
    {
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
        public Int64 CurrentAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 更新的数量
        /// </summary>      
        public Int64 UpdateAmount
        {
            get;
            set;
        }

        public Int64 TotalAmount
        {
            get;
            set;
        }        

        /// <summary>
        /// 流水
        /// </summary>      
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
        public string CreateTime
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

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        // <summary>
        /// 登陆名称
        /// </summary>       
        public string LoginName
        {
            get;
            set;
        }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>      
        public string SourceName
        {
            get;
            set;
        }

    }
}
