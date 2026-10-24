namespace target.Models.ViewModels
{
    public class GameVendorRecordDuplicateCheckResult
    {
        /// <summary>
        /// 檢查這款遊戲是否支援加注流程(會記錄寫入額外的資訊)
        /// </summary>
        public bool IsSupportRaiseProcessGame { get; set; }

        /// <summary>
        /// 若判斷到單號已存在。會走另一個流程
        /// </summary>
        public bool IsExecuteRaiseProcess { get; set; }


        /// <summary>
        /// 注單屬於0元注單、且尚未結算 (若遊戲商屬於 投注結算二合一的話)
        /// 這種情況不拋錯誤
        /// </summary>
        public bool IsWagerZeroBetAndWatingSettle { get; set; }

        /// <summary>
        /// 判斷注單已存在。強制進入settle流程 (只適用placeBetAndSettle 二合一的遊戲商)
        /// </summary>
        public bool IsForceGoToSettleProcess { get; set; }



        public string ExistBetRecordNoPGS { get; set; }


    }
}
