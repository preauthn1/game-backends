namespace target.Utility
{
    public class AuthorizeSenderConfigModel
    {
        public string SenderAddress { get; set; }
        public string SenderPrivateKey { get; set; }

        /// <summary>
        /// 不同分組 在queue使用情境不同
        /// </summary>
        public string QueueGroup { get; set; }


    }


}
