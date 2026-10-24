namespace target.Models.Enums
{
    public enum EventSubTypeEnum
    {
        /// <summary>
        /// 捕魚獎勵
        /// </summary>
        FishBonusPrize = 51,


        /// <summary>
        /// AGG活動獎勵 虛寶卡獎勵 統一彙整區
        /// </summary>        

        //嘗試不根據event來分 而是根據eventPrize來分
        //AGGItemCard = 201,
        //AGG_GiftCode = 203,
        //AGG_BonusMission = 204,
        //AGG_Ranking = 205,
        //AGG_Raffle = 206,
        //AGG_VIP = 207,

        /// <summary>
        /// AGG活動獎勵-其他 統一彙整區
        /// </summary>
        AGGBonusOther = 219,



        Other = 999,

    }


}
