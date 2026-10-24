using System;

namespace game.Entity.Users
{
    public class EventContest
    {
        /// <summary>
        /// 場次ID
        /// </summary>
        public int? ID { get; set; }

        /// <summary>
        /// 場次名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 游戏名稱
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 開或關
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 是否顯示
        /// </summary>
        public byte Show { get; set; }

        /// <summary>
        /// 結束後過幾分鐘計算獎勵
        /// </summary>
        public int RewardAfterMin { get; set; }

        /// <summary>
        /// 入場費
        /// </summary>
        [MoneyInt]
        public long Charge { get; set; }

        /// <summary>
        /// 遊戲時間
        /// </summary>
        public int PlayTime { get; set; }
        /// <summary>
        /// 比賽幣
        /// </summary>
        [MoneyInt]
        public long Coin { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 进场时间
        /// </summary>
        public TimeSpan Entertime { get; set; }
        /// <summary>
        /// 终止时间
        /// </summary>
        public TimeSpan Deadline { get; set; }

        /// <summary>
        /// 周期进场时间
        /// </summary>
        public TimeSpan PeriodEntertime { get; set; }
        /// <summary>
        /// 周期终止时间
        /// </summary>
        public TimeSpan PeriodDeadline { get; set; }


        /// <summary>
        /// 區間結算週期(hour)
        /// </summary>
        public int RankInterval { get; set; }


    }
}
