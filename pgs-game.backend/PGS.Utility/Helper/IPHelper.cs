using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Http;
using target.Models;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility
{
    public static class IPHelper
    {
        public static readonly string[] CloudflareCidrs =
        {
            "0.0.0.0/20",
            "0.0.0.0/22",
            "0.0.0.0/22",
            "0.0.0.0/22",
            "0.0.0.0/18",
            "0.0.0.0/18",
            "0.0.0.0/20",
            "0.0.0.0/20",
            "0.0.0.0/22",
            "0.0.0.0/17",
            "0.0.0.0/15",
            "0.0.0.0/13",
            "0.0.0.0/14",
            "0.0.0.0/13",
            "0.0.0.0/22",

            "2400:cb00::/32",
            "2606:4700::/32",
            "2803:f800::/32",
            "2405:b500::/32",
            "2405:8100::/32",
            "2a06:98c0::/29",
            "2c0f:f248::/32"
        };

        /// <summary>
        /// 我方反向代理使用的內部網段。
        /// 只有實際連線來源位於這些網段時，才允許解析轉發 IP Header。
        /// 若基礎設施網段異動，必須同步更新此清單。
        /// </summary>
        public static readonly string[] TrustedProxyCidrs =
        {
            "0.0.0.0/16", // Kubernetes Pod CIDR
            "0.0.0.0/16", // Docker / internal reverse proxy CIDR
            "0.0.0.0/8",
            "::1/128"
        };

        public static string GetIP()
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //符合某種條件格式 可以模擬偽造用戶端IPs
                string mockIP = Get_IPAddress_PGSMock();

                if (!string.IsNullOrWhiteSpace(mockIP))
                    return mockIP;

                //根據當前選用的網路加速服務。優先使用該服務的IP (順序很重要)
                string cf_ip = Get_IPAddress_Cloudflare();

                if (!string.IsNullOrWhiteSpace(cf_ip))
                    return cf_ip;

                var remoteIp = NormalizeIPAddress(MyHttpContext.Current.Connection.RemoteIpAddress);

                if (remoteIp == null)
                    return string.Empty;

                string forwardedIp = Get_IPAddress_FromTrustedProxyChain(remoteIp);

                if (!string.IsNullOrWhiteSpace(forwardedIp))
                    return forwardedIp;

                return remoteIp.ToString();
            }
            catch
            {

            }

            return string.Empty;

        }


        public static string Get_IPAddress_Cloudflare()
        {
            try
            {
                string cf_ip = HeaderValueHelper.GetHeaderValueByKey("cf-connecting-ip");

                if (string.IsNullOrWhiteSpace(cf_ip))
                    return string.Empty;

                if (!HeaderValueHelper.IsHeaderKeyExist("cf-ipcountry"))
                    return string.Empty;

                if (!TryParseIPAddress(cf_ip, out var parsedCloudflareClientIP))
                    return string.Empty;

                var remoteIp = NormalizeIPAddress(MyHttpContext.Current.Connection.RemoteIpAddress);

                // Cloudflare 直接連入 API。
                if (remoteIp != null && IsCloudflareNode(remoteIp))
                {
                    return parsedCloudflareClientIP.ToString();
                }

                string remoteIP_value = remoteIp?.ToString() ?? string.Empty;
                string real_ip = Get_IPAddress_Nginx();

                // Cloudflare 經我方內部 ingress 連入 API。
                // 必須先確認實際連線來源是可信代理，不能只相信可由外部偽造的 Header。
                if (TryParseIPAddress(real_ip, out var parsedRealIP))
                {
                    if(IsTrustedProxy(remoteIp) || IsCloudflareNode(parsedRealIP))
                    {
                        return parsedCloudflareClientIP.ToString();
                    }
                }

                bool isLocker = CacheHelper.CheckShortCacheLockerExist("IPAddress_Cloudflare_StrangeIP", 60, isThrowEx: false);

                if (!isLocker)
                {
                    // ::ffff:0.0.0.0 是k8s或Nginx層的內網IP

                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = $"Cloudflare: {remoteIP_value} {cf_ip} {real_ip} 不在定義的區段中。是否有人偽造IP? ",
                        TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                    });
                }

                return string.Empty;
            }
            catch
            {

            }

            return "";
        }

        public static string Get_IPAddress_Nginx()
        {
            //來自Nginx的話 會有這組
            string realIP = HeaderValueHelper.GetHeaderValueByKey("X-Real-IP");

            //我方系統k8s nginx自訂會帶 作為判斷非法偽造的依據
            if (!HeaderValueHelper.IsHeaderKeyExist(SystemConstant.HeaderKey_PGS_Nginx_AlwaysHeader))
            {
                return "";
            }

            return realIP;
        }

        /// <summary>
        /// 從最靠近 API 的節點開始，反向剝除可信代理，取得第一個非可信節點。
        /// X-Forwarded-For 左側可能包含用戶自行偽造的值，因此不可直接取第一個 IP。
        /// </summary>
        private static string Get_IPAddress_FromTrustedProxyChain(IPAddress remoteIp)
        {
            if (remoteIp == null || !IsTrustedProxy(remoteIp))
                return string.Empty;

            // 此 Header 由我方 ingress 固定加入；仍須搭配 RemoteIpAddress 網段驗證，不可只憑 Header 存在就信任轉發資訊。
            if (!HeaderValueHelper.IsHeaderKeyExist(SystemConstant.HeaderKey_PGS_Nginx_AlwaysHeader))
                return string.Empty;

            string forwardedFor = HeaderValueHelper.GetHeaderValueByKey("X-Forwarded-For");
            List<IPAddress> forwardedIPs = ParseForwardedFor(forwardedFor);

            var currentIp = remoteIp;

            for (int i = forwardedIPs.Count - 1; i >= 0; i--)
            {
                if (!IsTrustedProxy(currentIp))
                    break;

                currentIp = forwardedIPs[i];
            }

            if (forwardedIPs.Count > 0)
                return currentIp.ToString();

            // 舊的代理設定可能只提供 X-Real-IP；僅在實際來源為可信代理時相容此格式。
            string realIP = HeaderValueHelper.GetHeaderValueByKey("X-Real-IP");
            return TryParseIPAddress(realIP, out var parsedRealIP)
                ? parsedRealIP.ToString()
                : string.Empty;
        }

        private static List<IPAddress> ParseForwardedFor(string forwardedFor)
        {
            if (string.IsNullOrWhiteSpace(forwardedFor))
                return new List<IPAddress>();

            return forwardedFor
                .Split(',')
                .Select(x => x.Trim())
                .Select(x => TryParseIPAddress(x, out var ip) ? ip : null)
                .Where(x => x != null)
                .ToList();
        }

        private static bool TryParseIPAddress(string value, out IPAddress ip)
        {
            ip = null;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!IPAddress.TryParse(value.Trim(), out var parsedIP))
                return false;

            ip = NormalizeIPAddress(parsedIP);
            return true;
        }

        private static IPAddress NormalizeIPAddress(IPAddress ip)
        {
            if (ip == null)
                return null;

            return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
        }

        public static bool IsTrustedProxy(IPAddress ip)
        {
            ip = NormalizeIPAddress(ip);

            return ip != null && TrustedProxyCidrs.Any(cidr => IsInCidr(ip, cidr));
        }


        public static string Get_IPAddress_PGSMock()
        {
            //假如有發現異常 (e.g. 程式碼外洩?) 請調整改其他變數名
            string mockIP = HeaderValueHelper.GetHeaderValueByKey("X-target-Mock-IP-V3");
            string emptyIP = string.Empty;

            if (string.IsNullOrWhiteSpace(mockIP))
                return "";

            //公司IP禁止偽造模擬
            if (IsCompanyIP(mockIP))
                return "";

            if (SettingHelper.IsDev || SettingHelper.IsLocalhostWebSiteName)
                return mockIP;

            //如果Production環境。模擬MockIP，必須要帶入password (password的格式必須要是當前時間的 32碼單號)
            string password = [REDACTED]"X-target-Mock-IP-Password");

            if(RandomHelper.IsPgsUniqueOrderNoPasswordFormat(password))
            {
                return mockIP;
            }

            return "";
        }


        /// <summary>
        /// 判斷是否在指定的IP範圍中
        /// </summary>
        public static bool IsInCidr(IPAddress inputIP, string cidr)
        {
            var ip = inputIP;

            if (inputIP.IsIPv4MappedToIPv6)
            {
                ip = inputIP.MapToIPv4();
            }

            var parts = cidr.Split('/');

            var network = IPAddress.Parse(parts[0]);
            int prefixLength = int.Parse(parts[1]);

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] networkBytes = network.GetAddressBytes();

            if (ipBytes.Length != networkBytes.Length)
                return false;

            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (ipBytes[i] != networkBytes[i])
                    return false;
            }

            if (remainingBits > 0)
            {
                byte mask = (byte)(0xFF << (8 - remainingBits));

                if ((ipBytes[fullBytes] & mask) !=
                    (networkBytes[fullBytes] & mask))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsCloudflareNode(IPAddress ip)
        {
            return CloudflareCidrs.Any(cidr => IsInCidr(ip, cidr));
        }


        public static bool CheckIsMarshallHomeIPAddress(string targetIP)
        {
            if (string.IsNullOrWhiteSpace(targetIP))
                return false;

            if (!targetIP.StartsWith("101.1") && !targetIP.StartsWith("49.2") && !targetIP.StartsWith("2402:7500:"))
                return false;

            var result = CheckIPsLocationByRemoteAPI(targetIP);

            if (result == null)
                return false;

            return result.country_code2 == "TW" && (result.city.ContainsWihtoutCase("taipei") || result.state_prov.ContainsWihtoutCase("taipei") || result.state_prov.ContainsWihtoutCase("taiwan"));
        }


        public static IPGeoParseModel CheckIPsLocationByRemoteAPI(string targetIP)
        {
            //這個免費key 一天只有1000次。請勿提供給一般外部用戶使用
            string apiUrl = "https://example.invalid" + $"/ipgeo?apiKey=[REDACTED]";

            var apiResult = APIRequestHelper.RequestApiResult<IPGeoParseModel>(new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
            });

            return apiResult.ResultObject;
        }


        public static List<string> AnalyzeIPsAndCombineOne(string ipsText)
        {
            List<string> IPs = AppHelper.AnalyzeTextFromRawTextBySpaceChar(ipsText);
            return AnalyzeIPsAndCombineOne(IPs);
        }



        /// <summary>
        /// 將前三碼一樣的IPs 集合起來變成一組
        /// </summary>
        public static List<string> AnalyzeIPsAndCombineOne(List<string> ips)
        {
            var groupBy = ips.GroupBy(ip =>
            {
                var split = ip.Split('.').ToList();

                if (split.Count() <= 3)
                {
                    return ip;
                }

                return split[0] + "." + split[1] + "." + split[2];
            })
            .Select(x => new
            {
                Key = x.Key,
                Items = x.ToList()
            })
            .ToList();

            List<string> combineIPs = new List<string>();

            foreach (var eachGroup in groupBy)
            {
                if (eachGroup.Items.Count() == 1)
                {
                    combineIPs.Add(eachGroup.Items.FirstOrDefault());
                }
                else
                {
                    combineIPs.Add(eachGroup.Key + ".*");
                }
            }

            return combineIPs;
        }


        public static CloudflareIPRangeApiResponseModel FetchCloudflareIPRange()
        {
            var result = APIRequestHelper.RequestApiResult<CloudflareIPRangeApiResponseModel>(new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = "client/v4/ips",
                Method = HttpMethodType.HttpGet,
            });

            return result.ResultObject;
        }


        public class IPGeoParseModel
        {
            public string country_code2 { get; set; }

            public string state_prov { get; set; }

            public string city { get; set; }
        }


        public static bool IsCompanyIP(string myIP = "")
        {
            if(SettingHelper.IsLocalhostWebSiteName && myIP == "0.0.0.0")
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(myIP))
            {
                myIP = GetIP();
            }

            return SystemConstant.CompanyIPAddressList.Any(ip => ip == myIP);
        }

        public static bool IsIPv6(string myIP = "")
        {
            IPAddress address;

            if (IPAddress.TryParse(myIP, out address))
            {
                switch (address.AddressFamily)
                {
                    case example.invalid.Sockets.AddressFamily.InterNetwork:
                        return false;

                    case example.invalid.Sockets.AddressFamily.InterNetworkV6:
                        return true;

                    default:
                        break;
                }
            }

            return false;
        }

    }

}
