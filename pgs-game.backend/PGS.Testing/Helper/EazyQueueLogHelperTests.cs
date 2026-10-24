using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EazyQueueLogHelperTests : BaseTest
    {

        [TestMethod]
        public void GetMultipleItems_Test()
        {
            var insertList = new List<ApiRequestLogQueueTempModel>()
            {
                new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = new Models.Table.ApiRequestLogEntity
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                    }
                },
                new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = new Models.Table.ApiRequestLogEntity
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                    }
                }
            };

            EazyQueueApiErrorLogHelper.AppendMultiple(insertList);

            int size = 500;

            var queueItems = EazyQueueApiErrorLogHelper.GetMultipleItems(size);

            var firstItem = insertList.FirstOrDefault();

            Assert.IsTrue(queueItems.Count() == 2);
            Assert.IsTrue(queueItems.Any(z=> z.ApiRequestLogEntity.UniqueID == firstItem.ApiRequestLogEntity.UniqueID));

            var queueItems_again = EazyQueueApiErrorLogHelper.GetMultipleItems(size);

            Assert.IsTrue(queueItems_again.Count() == 0);

            return;

        }

        [TestMethod]
        public void Append_Maxsize_Test()
        {
            int size = 5000;

            //驗證即便插入了5000筆 queue的items也有上限
            for(int i = 0; i < size; i++)
            {
                EazyQueueApiErrorLogHelper.Append(new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = new Models.Table.ApiRequestLogEntity
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                    }
                });
            }

            var queueItems = EazyQueueApiErrorLogHelper.GetMultipleItems(size);

            //要對應  BaseQueueHelper MAX_SIZE 變數
            Assert.IsTrue(queueItems.Count() <= 2500 );

            return;

        }

    }
}
