namespace game.Entity.Game
{
    public class BaseSingleWalletModeResponseModel<T>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }


}
