namespace PgsTokenContract.Models
{
    public class NFTInfoViewModel
    {
        public int TokenID { get; set; }
        public string Name { get; set; }
        public string OwnerAddress { get; set; }
        public int RTP { get; set; }
    }

    public class NFTMintRequestModel
    {
        public string MagicPassword { get; set; }
        public string Name { get; set; }
        public int RTP { get; set; }
        public string OwnerAddress { get; set; }
        public string TargetContractAddress { get; set; }

    }

    public class NFTMintResultViewModel
    {
        public object Result { get; set; }
    }
}
