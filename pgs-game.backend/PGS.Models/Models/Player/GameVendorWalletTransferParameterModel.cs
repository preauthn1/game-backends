using target.Models.Enums;

namespace target.Models.ViewModels
{
    public class GameVendorWalletTransferParameterModel
    {
        public TransactionTypeEnum TranferType { get; set; }

        public int PlayerID { get; set; }

        public int GameVendorID { get; set; }

        public int? GameID { get; set; }

        public decimal Amount { get; set; }

        public string OrderNo_PGS { get; set; }

        public string OrderNo_GameVendor { get; set; }


    }



}
