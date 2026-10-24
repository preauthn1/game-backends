using target.BL.Interface;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.BL.Services
{
    public class ServiceInstanceHelper
    {


        public static IBasicSemalessCallbackService InstanceCallbackService(SingleWalletPlatformApiLogicTypeEnum platformApiType)
        { 
            //如果MSTest 會注入Mock
            if (SettingHelper.IsMSTest)
            {
                var type = typeof(IBasicSemalessCallbackService);

                if(IOCHelper.IsMatchAnyMock(type))
                {
                    return (IBasicSemalessCallbackService)IOCHelper.Resolve(type);
                }
            }
                             
            switch (platformApiType)
            {
                case SingleWalletPlatformApiLogicTypeEnum.Default:
                    return new SingleWalletPlatformApiRequestRefService();

                case SingleWalletPlatformApiLogicTypeEnum.QTech:
                    return new SWQTechFormatCallbackRefService();

                case SingleWalletPlatformApiLogicTypeEnum.OPS:
                    return new SWOPSFormatCallbackRefService();

                case SingleWalletPlatformApiLogicTypeEnum.THub:
                    return new SWTHubFormatCallbackRefService();

                case SingleWalletPlatformApiLogicTypeEnum.Notix:
                    return new SWNotixFormatCallbackRefService();

                default:
                    throw new TerproException(ApiErrorCode.NotImplement , new ErrorDetailModel
                    {
                        LogExtraInfo = $"InstanceCallbackService: {platformApiType.ToInteger()}",
                        IsAlwaysNotify = true,
                    });
            }
        }

        public static SWW88FormatCallbackRefService InstanceW88Service()
        {
            return new SWW88FormatCallbackRefService();
        }


        public static IGameVendorIntegrationService InstanceVendorService(GameVendorTypeEnum vendorID)
        {
            return InstanceVendorService(vendorID.ToInteger());
        }


        public static ThirdPartySlotsGameVendorComplexService InstanceSlotsGameVendorService(GameVendorTypeEnum vendorID)
        {
            if(!SystemConstant.SlotsExtensionApiSupportVendors.Contains(vendorID.ToInteger()))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"{vendorID}"));
            }

            return new ThirdPartySlotsGameVendorComplexService(vendorID);
        }


        public static IGameVendorIntegrationService InstanceVendorService(int vendorID)
        {
            try
            {                
                return IOCHelper.ResolveByKey<IGameVendorIntegrationService>(vendorID);
            }
            catch(Exception ex)
            {
                //如果這邊拋錯誤 
                //請確認 IOCInitializeHelper.cs 有正確註冊 每一家遊戲商的service
                //請確認 IOCInitializeHelper.cs 有正確註冊 每一家遊戲商的service
                //請確認 IOCInitializeHelper.cs 有正確註冊 每一家遊戲商的service
                //請確認 IOCInitializeHelper.cs 有正確註冊 每一家遊戲商的service
                throw ex;
            }
        }


    }
}
