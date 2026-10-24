using System;
using System.Collections.Generic;

namespace target.Models.Models
{
    public class ExcelEachMerchantPromotionDetailConvertModel
    {
        public string Currency { get; set; }

        [ExcelDataFormat("#,##0.00000000")]
        public decimal ExchangeRate { get; set; }
        public string Description { get; set; }
        public decimal DiscountRate { get; set; }

        public string ParentMerchantName { get; set; }

        public string MerchantName { get; set; }

        /// <summary>
        /// 輸贏數字
        /// </summary>
        public decimal ReferenceAmount { get; set; }

        /// <summary>
        /// 盤費 (原貨幣)
        /// </summary>
        [IsMinusRedStyle]
        public decimal FinalAmount_decimail2 { get; set; }


        [IsMinusRedStyle]
        public decimal FinalAmountWithExchange { get; set; }

        public decimal AmountOrginal { get; set; }

        public decimal AmountCurrencyExchange { get; set; }

        public decimal DebugValue3 { get; set; }


        [IsNotExcelColumn]
        public string DateMemo { get; set; }

        [IsNotExcelColumn]
        public int MerchantID { get; set; }

        [IsNotExcelColumn]
        public int ParentMerchantID { get; set; }

        [IsNotExcelColumn]
        public int? SortSequence { get; set; }

        [IsNotExcelColumn]
        public int? AggregativeGroup { get; set; }


        [IsNotExcelColumn]
        public DateTime? DiscountBegin { get; set; }

        [IsNotExcelColumn]
        public DateTime? DiscountEnd { get; set; }

    }


    public class ExcelEazyConvertClassValueModel<T> where T : class
    {
        public List<HeaderCellModel> HeaderCells { get; set; }

        public List<T> RowCells { get; set; }

        public bool IsDownloadToLocalForDebug { get; set; }
    }



    public class HeaderCellModel
    {

        public string HeaderText { get; set; }

        public int ColumnWidth { get; set; }


        public HeaderCellModel()
        {

        }

        public HeaderCellModel(string text , int columnWidth = 20)
        {
            //20長度 大概等於8~10個中文字的長度
            this.HeaderText = text;
            this.ColumnWidth = columnWidth;
        }

    }







    public class ExcelDataFormatAttribute : Attribute
    {

        public ExcelDataFormatAttribute(string value)
        {
            this.MyValue = value;
        }

        public string MyValue { get; set; }
    }


    public class IsMinusRedStyleAttribute : Attribute
    {

    }


    public class IsNotExcelColumn : Attribute
    {

    }



}
