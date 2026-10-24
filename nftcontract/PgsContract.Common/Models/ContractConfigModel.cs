namespace target.Utility
{
    public class ContractConfigModel
    {
        public int ChainID { get; set; }
        public string ProviderURL { get; set; }
        public string ContractAddress { get; set; }
        public string ABI_FILE_Name { get; set; }
        public long Gas { get; set; }
        public long GasPrice { get; set; }
    }


}
