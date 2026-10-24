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
    public class BaseAzureStorageModel : TableEntity
    {
        public string PartitionKeyDate { get; set; }

        public DateTime TimestampUTC8 { get; set; }


        public BaseAzureStorageModel()
        {
            this.PartitionKey = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHH");
            this.PartitionKeyDate = DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd");
            this.RowKey = System.Guid.NewGuid().ToString("N");
            this.Timestamp = DateTime.UtcNow;
            this.TimestampUTC8 = DateTime.UtcNow.AddHours(8);
        }
    }


}
