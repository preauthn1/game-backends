using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using System;

namespace target.BL.Interface
{
    public interface ICloudflareComplexService
    {
        void EnableDNSProxyFlag(CloudflareDNSEnum dnsType);

        void DisableDNSProxyFlag(CloudflareDNSEnum dnsType);
    }

}
