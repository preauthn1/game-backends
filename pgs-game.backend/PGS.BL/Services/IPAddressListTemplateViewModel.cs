using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class IPAddressListTemplateViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string IPAddressList { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int AssignMerchantCount { get; set; }
        public List<int> AssignMerchantIDs { get; set; }
        public List<string> IMChannels { get; set; }
        public int IPAddressCount { get; set; }
        public int AssignDefaultIPTemplateCount { get; set; }

        /// <summary>
        /// 包含換行符號的
        /// </summary>
        public string IPAddressWithLinebreak { get; set; }
        public string IPAddressForDisplay { get; set; }
        public Dictionary<int , string> MerchantNameMapping { get; set; }


    }


    public class IPAddressListTemplateCreateRequestModel
    {

        public string DisplayName { get; set; }

        /// <summary>
        /// 包含換行符號的ip列表
        /// </summary>
        public string IpAddressWithLinebreak { get; set; }
        public string Memo { get; set; }

    }


    public class IPAddressListTemplateUpdateRequestModel
    {

        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string IpAddressWithLinebreak { get; set; }
        public string Memo { get; set; }

    }

    public class IPAddressListAppendRequestModel
    {
        public int Id { get; set; }


        public string IPAddress { get; set; }       


        public string Memo { get; set; }


        /// <summary>
        /// 若當前配置為*號 通常會拋exception、可以指定不要拋錯誤
        /// </summary>
        public bool IsSkipExceptionWhenWildCard { get; set; }

    }


    public class IPAddressListMultipleAppendRequestModel
    {
        public int Id { get; set; }



        public string MultipleIPAddress { get; set; }


        /// <summary>
        /// 若當前配置為*號 不insert
        /// </summary>
        public bool IsSkipInsertWhenWildCard { get; set; }


        public string Memo { get; set; }
    }



    public class IPAddressListTemplateUpdateAssignMerchantIDRequestModel
    {

        public int Id { get; set; }

        public bool IsDelete { get; set; }

        public int MerchantID { get; set; }

        public List<int> MerchantIDs { get; set; }

    }



}
