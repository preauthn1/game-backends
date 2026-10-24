namespace Lck.GoogleApi
{
    public enum PGSDocumentTypeEnum
    {
        TransferAPI_EN = 1001,
        TransferAPI_CN = 1002,

        SeamlessAPI_EN = 2001,
        SeamlessAPI_CN = 2002,
    }


    public class GoogleApiDocumentInfoModel
    {
        public string FileName { get; set; }

        public string GoogleDocUrl { get; set; }

    }

}
