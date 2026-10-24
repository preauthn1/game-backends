using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Utility;

namespace target.BL.Services
{

    public class TransferRecordVendorService : BaseVendorService, ITransferRecordVendorService
    {
        public TransferRecordRepository TransferRecordRepository { get; set; }

        public GameVendorTransferRecordRepository GameVendorTransferRecordRepository { get; set; }


        public override bool IsAutoInstanceAllRepository
        {
            get
            {
                return false;
            }
        }

        public override bool IsAutoInstanceAllService
        {
            get
            {
                return false;
            }
        }


        public TransferRecordVendorViewModel GetTransferByMerchant(QueryTransferRecordRequestModel filter)
        {
            this.InstanceAllRepositoryAndService(true, true);

            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);
            
            //不允許兩個都為空
            if(string.IsNullOrWhiteSpace(filter.MerchantTransactionNo) &&
               string.IsNullOrWhiteSpace(filter.PGSTransactionNo))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, "PGSTransactionNo or MerchantTransactionNo must have value.");
            }
            
            if(filter.MerchantTransactionNo.IsNotEmptyTextNotNull() && 
               filter.PGSTransactionNo.IsNotEmptyTextNotNull())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, "PGSTransactionNo and MerchantTransactionNo can only choose one to search.");
            }

            var queryTransfer = TransferRecordRepository.GetByFilter(new QueryTransferRecordRequestViewModel
            {
                MerchantIDs = filter.MerchantID.ToListCollection(),
                OrderNo = filter.PGSTransactionNo,
                MerchantOrderNo = filter.MerchantTransactionNo
            });

            var convert = ConvertToViewModel(queryTransfer.FirstOrDefault());
            return convert;
        }

        public FishingTransferRecordVendorViewModel GetFishingTransferByMerchant(QueryFishingTransferRecordRequestModel filter)
        {
            this.InstanceAllRepositoryAndService(true, true);

            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //不允許兩個都為空
            if (string.IsNullOrWhiteSpace(filter.TransactionNo) &&
               string.IsNullOrWhiteSpace(filter.MerchantOrderNo))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, "MerchantTransactionNo must have value.");
            }

            var queryTransfer = GameVendorTransferRecordRepository.GetByFilter(new QueryGameVendorTransferRecordRequestViewModel
                {
                    MerchantIDs = filter.MerchantID.ToListCollection(),
                    TransactionNo = filter.TransactionNo,
                    PlatformOrderNo = filter.MerchantOrderNo
                })
                .FirstOrDefault();

            if (queryTransfer == null)
                return null;

            if (queryTransfer.MerchantID != filter.MerchantID && !SettingHelper.IsMSTest)
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            var convert = queryTransfer.ReturnInjecteObject<FishingTransferRecordVendorViewModel>();
            convert.Amount = queryTransfer.UpdateAmount;
            convert.MerchantOrderNo = queryTransfer.ThirdOrderNo;
            convert.TransferType = queryTransfer.Type;
            return convert;
        }


        private TransferRecordVendorViewModel ConvertToViewModel(TransferRecordEntity query)
        {
            if (query == null)
                return null;

            var convert = query.ReturnInjecteObject<TransferRecordVendorViewModel>();

            convert.PGSTransactionNo = query.OrderNo;
            convert.MerchantTransactionNo = query.MerchantOrderNo;
            convert.ConvertRate = query.ConvertRate ?? 1;
            return convert;
        }

    }
}
