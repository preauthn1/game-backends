using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.PerLog
{

    /// <summary>
    /// 每局游戏记录详细
    /// </summary>    
    public class DetailLog 
    {
        public Int64 Id { get; set; }

        /// <summary>
        /// 牌局标识
        /// </summary>
        public string LogID { get; set; }

        /// <summary>
        /// 服务器ID
        /// </summary>
        public int ServerID { get; set; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 牌局编号
        /// </summary>
        public Int64 DealNo { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 用户帐号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }


        /// <summary>
        /// 银行真实姓名
        /// </summary>
        public string UnionpayRealName { get; set; }

        /// <summary>
        /// 使用者最后登入ip
        /// </summary>
        public string LastLogonIPAddress { get; set; }

        /// <summary>
        /// 帐号身分
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 当前金币
        /// </summary>
        [MoneyInt]
        public Int64 CurrentAmount { get; set; }

        /// <summary>
        /// 更新金币
        /// </summary>
        [MoneyInt]
        public Int64 UpdateAmount { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime GameTime { get; set; }

        public string BatchNo { get; set; }

    }
}
