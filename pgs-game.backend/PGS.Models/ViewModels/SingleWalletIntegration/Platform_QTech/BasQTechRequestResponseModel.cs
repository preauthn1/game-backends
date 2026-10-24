namespace game.Entity.Game
{
    public class BasQTechRequestResponseModel
    {
        public int Code { get; set; }

        public string Message { get; set; }

    }


    public class QTechBalanceResponseModel : BasQTechRequestResponseModel
    {
        public decimal Balance { get; set; }

        public string Currency { get; set; }

        public string ReferenceId { get; set; }

    }

}
