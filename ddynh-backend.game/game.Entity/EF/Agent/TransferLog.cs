using System;

namespace game.Entity.Agent
{
    public class TransferLog
    {
        /// <summary>
        /// 平台ID  null 表示所有平台的数据
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// 渠道ID null 表示所有渠道的数据
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 代理ID
        /// </summary>
        public int AgentID { get; set; }


        /// <summary>
        /// 关联游戏用户ID
        /// </summary>
        public int AgentUserID { get; set; }


        /// <summary>
        /// 代理名称
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// 代理交易类型 ID
        /// </summary>
        public int SourceID { get; set; }

        /// <summary>
        /// 代理交易类型名称
        /// </summary>
        public string SourceName { get; set; }


        /// <summary>
        /// 目标代理ID
        /// </summary>
        public int? TargetAgentID { get; set; }


        /// <summary>
        /// 目标用户ID
        /// </summary>
        public int TargetUserID { get; set; }

        /// <summary>
        /// 目标名称
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// 自己的金币数量
        /// </summary>
        [MoneyInt]
        public long SelfAmount { get; set; }

        /// <summary>
        /// 管理游戏用户的金币数量
        /// </summary>
        [MoneyInt]
        public long RelationAmount { get; set; }

        /// <summary>
        /// 更新的金币数量
        /// </summary>
        [MoneyInt]
        public long UpdateAmount { get; set; }


        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }
    }
}
