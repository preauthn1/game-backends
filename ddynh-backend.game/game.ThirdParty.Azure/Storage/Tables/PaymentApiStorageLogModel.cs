using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.ThirdParty.Azure.Storage
{
    public class PaymentApiStorageLogModel : BaseAzureStorageModel
    {
        public string LogGuid { get; set; }

        public long OrderId { get; set; }

        public long UserId { get; set; }

        public int Amount { get; set; }

        public string PaymentType { get; set; }

        public string Category { get; set; }

        public string PayTypeInfo { get; set; }

        public string GWID { get; set; }


        public string LogMsg { get; set; }

        public string LogMsg2 { get; set; }

        public string LogMsg3 { get; set; }


        public string ExceptionMsg { get; set; }

        public string ExceptionStackTrace { get; set; }



    }


}
