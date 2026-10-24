namespace target.Utility.Utility.ThirdParty
{
    public class SkypeLoginApiResponse
    {
        public string error { get; set; }

        public string error_description { get; set; }

        public int expires_in { get; set; }

        public int ext_expires_in { get; set; }

        public string token_type { get; set; }

        public string access_token { get; set; }
    }

    public class SkypeSendMessageResponse
    {
        public string id { get; set; }

        public string RawResponseText { get; set; }
    }
}
