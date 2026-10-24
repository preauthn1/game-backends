using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ChartDataConvertRequestParameterModel
    {
        /// <summary>
        /// 根據數字大的排到小的
        /// </summary>
        public bool IsDescOrderByValue { get; set; }


        public string ChartDisplayName { get; set; }

        /// <summary>
        /// 查詢對象 e.g. 商戶名稱
        /// </summary>
        public string TargetName { get; set; }

        public List<ChartDataConvertItemModel> Items { get; set; }
    }

    public class ChartDataConvertItemModel
    {
        public string X_Label { get; set; }

        public decimal Y_Value { get; set; }

    }






}
