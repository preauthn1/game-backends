using Newtonsoft.Json;

namespace game.Entity.Game
{
    public class BaseNotixResponseModel
    {
        public int ErrorCode { get; set; }

    }

    public class NotixAPIBalanceAPIResponseModel : BaseNotixResponseModel
    {
        public NotixAPIBalanceDetailModel Balance { get; set; }

        public string PlayerName { get; set; }
    }


    public class NotixAPIPlaceBetAPIResponseModel : BaseNotixResponseModel
    {
        public NotixAPIBalanceDetailModel Balance { get; set; }

    }

    public class NotixAPISettleAPIResponseModel : BaseNotixResponseModel
    {
        public NotixAPIBalanceDetailModel Balance { get; set; }

    }

    public class NotixAPICancelAPIResponseModel : BaseNotixResponseModel
    {
        public NotixAPIBalanceDetailModel Balance { get; set; }

    }

    public class NotixAPIEventBonusAPIResponseModel : BaseNotixResponseModel
    {
        public NotixAPIBalanceDetailModel Balance { get; set; }

    }



    public class NotixAPIBalanceDetailModel
    {
        public string Currency { get; set; }

        public decimal Amount { get; set; }

    }



}
