using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class QueryChartDataStatReportApiRequestParameterModel
    {
        //根據呼叫端不同、會帶的參數也不同

        public List<int> MerchantIDs { get; set; }

        public int MerchantID { get; set; }

        public double TimeZone { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }


        public string Currency { get; set; }

    }

    public class ChartDataStatReportViewModel
    {
        public string ChartName { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public List<ChartDataStatReportItemViewModel> ChartDatas { get; set; }

    }

    public class ChartDataStatReportItemViewModel
    {
        public int? MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int? GameID { get; set; }

        public string GameName { get; set; }

        public List<ChartDataViewModel> Data { get; set; }

    }


    public class ChartDataViewModel
    {
        public ChartDataViewModel ()
        {

        }

        public ChartDataViewModel(DateTime time , int count)
        {
            this.StatTime = time;
            this.DataCount = count;
        }

        public DateTime StatTime { get; set; }

        public int DataCount { get; set; }

    }
}
