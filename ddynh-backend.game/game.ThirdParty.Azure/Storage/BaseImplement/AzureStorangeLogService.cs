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
    internal class AzureStorangeLogService
    {
        private CloudTableClient ServiceClient { get; set; }

        public AzureStorangeLogService(string conn)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(conn);
            this.ServiceClient = account.CreateCloudTableClient();
        }


        public void CreateIfNotExists(string name)
        {
            CloudTable table = this.GetTable(name);
            var result = table.CreateIfNotExistsAsync().Result;
            return;
        }


        private CloudTable GetTable(string name)
        {
            return this.ServiceClient.GetTableReference(name);
        }


        public void InsertDataAsync(string tableName, ITableEntity data)
        {
            this.CreateIfNotExists(tableName);

            TableOperation insertOperation = TableOperation.Insert(data);

            CloudTable table = this.GetTable(tableName);

            var result = table.ExecuteAsync(insertOperation).Result;

            return;
        }

    }
}
