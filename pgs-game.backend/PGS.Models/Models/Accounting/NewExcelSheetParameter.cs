using System.Collections.Generic;

namespace target.Models.Models
{
    public class NewExcelSheetParameter
    {

        public List<List<string>> EachRowText { get; set; }


        /// <summary>
        /// array依序設定第一欄 第二欄...
        /// </summary>
        public List<int> ColumnWidthSettings { get; set; }


        public bool IsHeaderRowHighlightStyle { get; set; }

    }

    public class NewExcelSheetV2Parameter
    {

        public List<List<string>> EachRowText { get; set; }


        /// <summary>
        /// array依序設定第一欄 第二欄...
        /// </summary>
        public List<ExcelCellConfigV2> ColumnConfigs { get; set; }


        /// <summary>
        /// 1000 大約是三排字的高度
        /// </summary>
        public short RowHeight { get; set; }


        public bool IsHeaderRowHighlightStyle { get; set; }

    }



    public class ExcelCellConfigV2
    {

        public ExcelCellConfigV2()
        {

        }


        public ExcelCellConfigV2(int width , bool isWarp)
        {
            this.Width = width;
            this.IsWrapText = isWarp;
        }


        public int Width { get; set; }

        /// <summary>
        /// 是否自動換行
        /// </summary>
        public bool IsWrapText { get; set; }

    }


}
