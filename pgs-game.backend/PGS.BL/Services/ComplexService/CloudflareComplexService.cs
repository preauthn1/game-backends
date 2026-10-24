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
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models;
using target.Models.CultureText;
using target.Models.ViewModels.GameVendor;
using example.invalid;
using target.Utility.Utility;

namespace target.BL.Services
{

    public class CloudflareComplexService : BaseService, ICloudflareComplexService
    {

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }



        public void EnableDNSProxyFlag(CloudflareDNSEnum dnsType)
        {
            var zoneInfo = GetZoneInfoByUrl(dnsType);

            UpdateDNSByAPI(zoneInfo, new CloudflareDNSUpdateReqeust
            {
                name = zoneInfo.DNS_Name,
                content = zoneInfo.DNS_Content,
                proxied = true,
            });
        }

        public void DisableDNSProxyFlag(CloudflareDNSEnum dnsType)
        {
            var zoneInfo = GetZoneInfoByUrl(dnsType);

            UpdateDNSByAPI(zoneInfo, new CloudflareDNSUpdateReqeust
            {
                name = zoneInfo.DNS_Name,
                content = zoneInfo.DNS_Content,
                proxied = false,
            });
        }


        private CloudflareDNSZoneInformation GetZoneInfoByUrl(CloudflareDNSEnum dnsType)
        {
            if(dnsType == CloudflareDNSEnum.DNS_888stars_publicAPI)
            {
                return new CloudflareDNSZoneInformation
                {
                    DNS_Name = "example.invalid",
                    DNS_Content = "example.invalid",
                    zone_id = "d605b6b321fff44db80d2db3d28acd6e",
                    dns_id = "2d4c5cfdabfe6eb260996729fd961818",
                };
            }

            throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"GetZoneInfoByUrl 查無對應, {dnsType.ToString()}"
            });
        }


        private void UpdateDNSByAPI(CloudflareDNSZoneInformation zoneInfo, CloudflareDNSUpdateReqeust model)
        {
            var param = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"/client/v4/zones/{zoneInfo.zone_id}/dns_records/{zoneInfo.dns_id}",
                Method = HttpMethodType.HttpPatch,
                LogAction = WriteReqeustLog,
                AdvancedSetting = new AdvancedSetting
                {
                    Log_ApiCategory = "UpdateDNS",
                }
            };

            //有效期限 2028/01 Bearer [REDACTED]
            //有效期限 2028/01 Bearer [REDACTED]
            //有效期限 2028/01 Bearer [REDACTED]
            param.AddHeaderData("Authorization", "Bearer [REDACTED]");

            var result = APIRequestHelper.RequestApiResult<CloudflareDNSUpdateResponse>(param, model);

            var resultObj = result.ResultObject;

            if (resultObj.code > 0 && !string.IsNullOrWhiteSpace(resultObj.error))
            {
                string msg = $"呼叫CF API失敗，{result.ResponseText}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_job_api);
            }

            return;
        }


        private void WriteReqeustLog(ApiRequestLogTemporaryInfoModel model)
        {
            ExternalApiRequestLogRefService.CreateToOtherTable(model);
            return;
        }


    }
}
