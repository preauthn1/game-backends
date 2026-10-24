using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using System.Threading.Tasks;
using target.Models.Enums;
using target.Utility.Extensions.Web;
using target.Models;
using Lck.Utility.Extensions.Helper;
using System.Transactions;
using target.Utility.IOC;

namespace target.BL.Services
{
    public partial class ApiRequestLogService : BaseService, IApiRequestLogService
    {

        private static object _lockObject2 = new object();

        private static object _lockObject3 = new object();

        private static object _lockObject4 = new object();

        private static object _lockObject5 = new object();

        private const int maxInsertSize = 1000;


        public void ReadQueueAndSyncApiRequestLogToRemote()
        {
            try
            {
                //加個lock機制避免越積越多sql connect
                if (Monitor.TryEnter(_lockObject3, new TimeSpan(0, 0, 5)))
                {
                    try
                    {
                        while (true)
                        {
                            var myItems = EazyQueueApiRequestLogHelper.GetMultipleItems(maxInsertSize);

                            if (myItems.IsEmptyOrNull())
                                break;

                            WriteApiRequestLogToStorage(myItems);

                            if (myItems.Count() < (maxInsertSize - 1))
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $" ReadQueueAndSyncApiRequestLogToRemote 錯誤, {ex.GetMessageAndErrorLine()}";
                        SendMessageToChannel(msg);
                    }
                    finally
                    {
                        Monitor.Exit(_lockObject3);
                    }
                }
            }
            catch (Exception ex)
            {
                if (SettingHelper.IsMSTest || SettingHelper.IsDev)
                {
                    string msg = $"ReadQueueAndSyncApiRequestLogToRemote 錯誤啦，{ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel("");
                }
            }
        }


        public void ReadQueueAndSyncSwCallbackLogToRemote()
        {
            try
            {
                var tartgetLock = _lockObject2;

                //加個lock機制避免越積越多sql connect
                if (Monitor.TryEnter(tartgetLock, new TimeSpan(0, 0, 5)))
                {
                    try
                    {
                        while (true)
                        {
                            var myItems = EazyQueueSwCallbackLogHelper.GetMultipleItems(maxInsertSize);

                            if (myItems.IsEmptyOrNull())
                                break;

                            WriteSWLogToStorage(myItems);

                            if (myItems.Count() < (maxInsertSize - 1))
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $" ReadQueueAndSyncSwCallbackLogToRemote 錯誤, {ex.GetMessageAndErrorLine()}";
                        SendMessageToChannel(msg);
                    }
                    finally
                    {
                        Monitor.Exit(tartgetLock);
                    }
                }
            }
            catch
            {


            }
        }


        public void ReadQueueAndSyncErrorLogToRemote()
        {
            try
            {
                var tartgetLock = _lockObject5;

                //加個lock機制避免越積越多sql connect
                if (Monitor.TryEnter(tartgetLock, new TimeSpan(0, 0, 5)))
                {
                    try
                    {
                        while (true)
                        {
                            var myItems = EazyQueueApiErrorLogHelper.GetMultipleItems(maxInsertSize);

                            if (myItems.IsEmptyOrNull())
                                break;

                            WriteErrorLogToStorage(myItems);

                            if (myItems.Count() < (maxInsertSize - 1))
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $" ReadQueueAndSyncErrorLogToRemote 錯誤, {ex.GetMessageAndErrorLine()}";
                        SendMessageToChannel(msg);
                    }
                    finally
                    {
                        Monitor.Exit(tartgetLock);
                    }
                }
            }
            catch
            {


            }
        }



        public void ReadQueueAndSyncThirdPartyLogToRemote()
        {
            try
            {
                var tartgetLock = _lockObject4;

                //加個lock機制避免越積越多sql connect
                if (Monitor.TryEnter(tartgetLock, new TimeSpan(0, 0, 5)))
                {
                    try
                    {
                        while (true)
                        {
                            var myItems = EazyQueueThirdAPIRequestLogHelper.GetMultipleItems(maxInsertSize);

                            if (myItems.IsEmptyOrNull())
                                break;

                            string tableName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog);

                            ElasticsearchHelper.BulkInsert(tableName, myItems);

                            if (myItems.CountOrZero() < (maxInsertSize - 1))
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $" ReadQueueAndSyncThirdPartyLogToRemote 錯誤, {ex.GetMessageAndErrorLine()}";
                        SendMessageToChannel(msg);
                    }
                    finally
                    {
                        Monitor.Exit(tartgetLock);
                    }
                }
            }
            catch
            {


            }

            return;
        }


        public void WriteSWLogToStorage(List<SingleWalletMerchantApiRequestLogEntity> myItems)
        {
            try
            {
                //裡面包含了基本DB寫入 + Elasticsearch寫入
                //裡面包含了基本DB寫入 + Elasticsearch寫入
                //裡面包含了基本DB寫入 + Elasticsearch寫入
                SingleWalletMerchantApiRequestLogRepository.BulkInsertToMultipleStorage(myItems, SingleWalletMerchantApiRequestLogBriefRepository);
            }
            catch (Exception ex)
            {
                string msg = $"SW log BulkInsert 錯誤, {ex.GetMessageAndErrorLine()}";
                SendMessageToChannel(msg);

                if (SettingHelper.IsMSTest)
                {
                    throw;
                }

                var expiredTime = DateTimeHelper.GetCurrentTime().AddMinutes(-5);

                //如果超過時間還沒寫入成功 放棄寫入
                var noneExpiredData = myItems
                    .Where(x => x.CreateTime > expiredTime)
                    .ToList();

                EazyQueueSwCallbackLogHelper.AppendMultiple(noneExpiredData);
            }
        }


        public void WriteErrorLogToStorage(List<ApiRequestLogQueueTempModel> myItems)
        {
            if (myItems.IsEmptyOrNull())
                return;

            var groupByTable = myItems.GroupBy(x => x.ExtraInfoV2.TableType);

            foreach (var eachGroup in groupByTable)
            {
                ElasticLogTableTypeEnum tableType = eachGroup.Key;

                ApiRequestLogV2024Repository.BulkInsertToMultipleStorage(tableType, eachGroup.ToList());
            }

            return;
        }



        private void WriteApiRequestLogToStorage(List<ApiRequestLogQueueTempModel> myItems)
        {
            try
            {
                string logTableName = SettingHelper.ApiRequestLogTableName;

                bool isEnableElasticLogV2 = TableNameConvertHelper.IsEnableElasticLogByTableName(logTableName);

                bool isExternalVendor = SettingHelper.IsExternalVendorApiLogTable;

                if (isEnableElasticLogV2)
                {
                    //2026年開始。部分table紀錄寫到ES 
                    InsertDataToExternalVendorAPITable_Elastic_V2026(myItems);
                }
                else if (isExternalVendor)
                {
                    //2024/04 External為了節省容量 調整記錄方式
                    InsertDataToExternalVendorAPITable_V2024(myItems);
                }
                else
                {
                    //其他舊版table (全部資訊存在同一個rows)
                    InsertDataToDB_CommonLogTable(myItems);
                }
            }
            catch (Exception ex)
            {
                string ks8Name = RuntimeHelper.GetK8sInstanceName();
                string msg = $" {ks8Name} WriteApiRequestLogToDBAsync BulkInsert 錯誤，資料筆數:{myItems.Count()}, {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg);

                //2024/8 調整: 寫入失敗就放棄這些資料 (不然可能會導致記憶體爆炸)
                //SwCallbackLogQueueHelper.AppendMultiple(myItems);
            }
        }



        private void InsertDataToExternalVendorAPITable_Elastic_V2026(List<ApiRequestLogQueueTempModel> myItems)
        {
            string logTableName = SettingHelper.ApiRequestLogTableName;

            var ESIndex = TableNameConvertHelper.GetElasticsearchIndexNameByTableName(logTableName);

            var lisfOfInsertElastic = new List<FullInfoApiRequestLogESModel>();

            foreach (var item in myItems)
            {
                if (item.ExtraInfoV2 == null || string.IsNullOrWhiteSpace(item.ExtraInfoV2.UniqueID))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ExtraInfo"));
                }

                string websiteName = RuntimeHelper.GetK8sInstanceName();

                var convertES = DataModelConvertHelper.ConvertToElasticFullRequestModel(item);
                lisfOfInsertElastic.Add(convertES);
            }

            // 2026年開始。部分紀錄寫到ES (如果config有配置)
            ApiRequestLogV2024Repository.BulkInsertApiReqeustLogToElasticSearch(logTableName, lisfOfInsertElastic);
        }



        private void InsertDataToExternalVendorAPITable_V2024(List<ApiRequestLogQueueTempModel> myItems)
        {
            MappingTagValuesRepository MappingTagValuesRepository = IOCHelper.InstanceRepository<MappingTagValuesRepository>();

            string logTableName = SettingHelper.ApiRequestLogTableName;
            string logTableOther = logTableName.Replace("API", "APIOther");

            var lisfOfInsert1_1 = new List<ApiRequestLogV2025Entity>();
            var lisfOfInsert2 = new List<MappingTagValuesEntity>();
            var lisfOfInsert3 = new List<MappingTagValuesEntity>();
            var lisfOfInsertElastic = new List<FullInfoApiRequestLogESModel>();

            bool isEnableElasticLogV2 = TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi);

            foreach (var item in myItems)
            {
                if (item.ExtraInfoV2 == null || string.IsNullOrWhiteSpace(item.ExtraInfoV2.UniqueID))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ExtraInfo"));
                }

                if (IsSkipInsertByDBSetting(logTableName))
                {
                    continue;
                }

                string websiteName = RuntimeHelper.GetK8sInstanceName();

                var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(item.ExtraInfoV2.UniqueID);

                var convert = item.ApiRequestLogEntity.ReturnInjecteObject<ApiRequestLogV2025Entity>();
                convert.UniqueID = item.ExtraInfoV2.UniqueID;
                convert.PartitionTime = recover.Time.Value;
                convert.ExecutionTime = item.ApiRequestLogEntity.ExecutionTime.ParseNullToZero();
                convert.ReferOrderNo = item.ExtraInfoV2.ReferOrderNo;

                if (convert.DebugInfo2 == convert.ReferOrderNo)
                {
                    convert.DebugInfo2 = null;
                }

                ApiRequestLogV2024Repository.TruncateColumnMaxLength(convert);
                lisfOfInsert1_1.Add(convert);


                if (!isEnableElasticLogV2)
                {
                    //舊版是寫到SQL server 鍵值表
                    lisfOfInsert3.Add(InstanceMappingValueEntity(convert, ApiReqeustLogTagValueMappingTypeEnum.RequestBody, item.ApiRequestLogEntity.RequestBody));

                    lisfOfInsert3.Add(InstanceMappingValueEntity(convert, ApiReqeustLogTagValueMappingTypeEnum.ResponseText, item.ApiRequestLogEntity.ResponseText));

                    lisfOfInsert3.Add(InstanceMappingValueEntity(convert, ApiReqeustLogTagValueMappingTypeEnum.WebsiteName, websiteName));

                    if (item.ApiRequestLogEntity.Headers != SystemConstant.Default_Text_DisabledLog)
                    {
                        lisfOfInsert3.Add(InstanceMappingValueEntity(convert, ApiReqeustLogTagValueMappingTypeEnum.Headers, item.ApiRequestLogEntity.Headers));
                    }
                }
                else
                {
                    var convertES = DataModelConvertHelper.ConvertToElasticFullRequestModel(item);
                    lisfOfInsertElastic.Add(convertES);
                }
            }

            // Step 1. 主表寫入
            ApiRequestLogV2024Repository.BulkInsert(logTableName, lisfOfInsert1_1, IsolationLevel.ReadUncommitted);

            // Step 2. 額外資訊二合一寫入
            MappingTagValuesRepository.CreateMany_BundleInsert(lisfOfInsert2, lisfOfInsert3);

            // 2026年開始。部分紀錄寫到ES (如果config有配置)
            ApiRequestLogV2024Repository.BulkInsertApiReqeustLogToElasticSearch(logTableName, lisfOfInsertElastic);

        }


    }



}
