using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
     

    public class ChartCommonDataStatReportViewModel
    {
        public string ChartName { get; set; }

        //public DateTime Begin { get; set; }

        //public DateTime End { get; set; }

        public List<string> Groups { get; set; }

        public List<string> Labels { get; set; }

        public List<ChartCommonColorDataStatReportItemViewModel> Datasets { get; set; }
    }



    public class ChartCommonColorDataStatReportItemViewModel
    {
        public string GroupName { get; set; }

        /// <summary>
        /// 長條圖是使用這種
        /// </summary>
        public List<ChartLabelMappingValueViewModel> Data { get; set; }

        /// <summary>
        /// 圓餅圖是使用這種
        /// </summary>
        public List<decimal> DataOfValues { get; set; }
    }


    public class ChartLabelMappingValueViewModel
    {
        public ChartLabelMappingValueViewModel()
        {

        }

        public ChartLabelMappingValueViewModel(string label, int count)
        {
            this.LabelName = label;
            this.Value = count;
        }

        public string LabelName { get; set; }

        public decimal Value { get; set; }

    }

}
