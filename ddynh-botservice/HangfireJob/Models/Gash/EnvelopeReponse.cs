using System.Xml.Serialization;

namespace HangfireJob.Models.Gash
{
    [XmlRoot(ElementName = "TRANS")]
    public class EnvelopeReponse
    {
        public string MSG_TYPE { get; set; }
        public string PCODE { get; set; }
        public string CID { get; set; }
        public string COID { get; set; }
        public string RRN { get; set; }
        public string CUID { get; set; }
        public string PAID { get; set; }
        public string AMOUNT { get; set; }
        public string ERPC { get; set; }
        public string PAY_STATUS { get; set; }
        public string RCODE { get; set; }
    }
}
