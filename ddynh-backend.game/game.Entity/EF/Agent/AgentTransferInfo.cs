using System;
using FW.Core.Helper;

namespace game.Entity.Agent
{
  public  class AgentTransferInfo: BaseEntity
    {
        /// <summary>
        /// 總代理ID
        /// </summary>
        public int Total_Agent_Id { get; set; }
        /// <summary>
        /// 總代理名稱
        /// </summary>
        public string Total_Agent_Name { get; set; }
        /// <summary>
        /// 轉帳類型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 轉帳金額
        /// </summary>
        public int Money { get; set; }
        /// <summary>
        /// 轉帳前金額
        /// </summary>
        public int Old_Money { get; set; }
        /// <summary>
        /// 轉帳後金額
        /// </summary>
        public int New_Money { get; set; }
        /// <summary>
        /// 轉帳時間
        /// </summary>
        public DateTime Created_at { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 厂商名称
        /// </summary>
        public string Name { get; set; }
    }
}
