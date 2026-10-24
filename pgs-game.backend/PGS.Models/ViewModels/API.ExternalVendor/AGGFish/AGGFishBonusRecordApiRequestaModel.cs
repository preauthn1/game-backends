using System;

namespace target.Models.ViewModels
{
    public class AGGFishBonusRecordApiRequestaModel
    {
        public string Merchantid { get; set; }

        public string Token { get; set; }

        public decimal Amount { get; set; }

        public DateTime Receivetime { get; set; }

        /// <summary>
        /// 可參考 AGGFishBonusTypeEnum
        /// </summary>
        public int Bonustype { get; set; }

        public string BonustypeDes { get; set; }

        public string TxtID { get; set; }
    }

    public class AGGFishGetPaymentLinkApiRequestaModel
    {
        public string Merchantid { get; set; }

        public string Token { get; set; }

    }

}
