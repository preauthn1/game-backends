using System.Collections.Generic;
using System.Data;
using example.invalid;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace game.Infrastructure
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 由Excel导入DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文件流</param>
        /// <param name="sheetName">Excel工作表名称</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <param name="_03or07">Excel03:True,Excel07:False</param>
        /// <returns>DataTable</returns>
        public static DataTable ImportDataTableFromExcel(Stream excelFileStream, string sheetName, int headerRowIndex,bool _03or07)
        {
            IWorkbook workbook;
            if (_03or07) workbook = new HSSFWorkbook(excelFileStream);
            else workbook = new XSSFWorkbook(excelFileStream);
            var sheet = workbook.GetSheet(sheetName);
            var table = new DataTable();
            var headerRow = sheet.GetRow(headerRowIndex);
            int cellCount = headerRow.LastCellNum;
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                table.Columns.Add(column);
            }
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var dataRow = table.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                }

                table.Rows.Add(dataRow);
            }
            
            return table;
        }

        /// <summary>
        /// 由Excel导入DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文件流</param>
        /// <param name="sheetNames">Excel工作表名称</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <param name="_03or07">Excel03:True,Excel07:False</param>
        /// <returns></returns>
        public static List<DataTable> ImportDataTableFromExcel(Stream excelFileStream, string[] sheetNames, int headerRowIndex, bool _03or07)
        {
            IWorkbook workbook;
            if (_03or07) workbook = new HSSFWorkbook(excelFileStream);
            else workbook = new XSSFWorkbook(excelFileStream);

            List<DataTable> dts=new List<DataTable>();

            foreach (var sheetName in sheetNames)
            {

                var sheet = workbook.GetSheet(sheetName);
                var table = new DataTable();
                var headerRow = sheet.GetRow(headerRowIndex);
                int cellCount = headerRow.LastCellNum;
                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                {
                    var column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                    table.Columns.Add(column);
                }
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var dataRow = table.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j)==null)
                        {
                            dataRow[j] = "";continue;
                        }
                        switch (row.GetCell(j).CellType)
                        {
                            case CellType.Blank:
                                dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                                break;
                                case CellType.String:
                                dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                                break;                                
                                case CellType.Numeric:
                                //if (row.GetCell(j) != null && DateUtil.IsValidExcelDate(row.GetCell(j).NumericCellValue))
                                //{
                                //    dataRow[j] = row.GetCell(j).DateCellValue;
                                //}
                                //else
                                //{
                                //    dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                                //}
                                short Format = row.GetCell(j).CellStyle.DataFormat;
                                if (Format==14||Format==31||Format==57||Format==58||Format==20)
                                {
                                    dataRow[j] = row.GetCell(j).DateCellValue;
                                }
                                else
                                {
                                    dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                                }
                                break;
                            default:
                                dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                                break;
                                
                        }

                    }

                    table.Rows.Add(dataRow);
                }
                table.TableName = sheetName;
                dts.Add(table);
            }

            return dts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="headerRowIndex"></param>
        /// <returns></returns>
        public static DataTable ImportDataTableFromExcel(ISheet sheet, int headerRowIndex)
        {
            var table = new DataTable();
            var headerRow = sheet.GetRow(headerRowIndex);
            int cellCount = headerRow.LastCellNum;
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                table.Columns.Add(column);
            }
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var dataRow = table.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    dataRow[j] = row.GetCell(j) != null ? row.GetCell(j).ToString() : "";
                }
                table.Rows.Add(dataRow);
            }
            return table;
        }

        public static DataSet ImportDataSetFromExcel(Stream excelFileStream)
        {
            var ds = new DataSet();
            var workbook = new HSSFWorkbook(excelFileStream);
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i) as ISheet;
                var dt = ImportDataTableFromExcel(sheet, 0);
                ds.Tables.Add(dt);
            }
            return ds;
        }

        public static MemoryStream RenderToExcel(DataTable table)
        {
            var ms = new MemoryStream();

            using (table)
            {
                var workbook = new HSSFWorkbook();
                var sheet = workbook.CreateSheet();
                var headerRow = sheet.CreateRow(0);
                var cellstyle = workbook.CreateCellStyle();
                cellstyle.FillBackgroundColor = HSSFColor.Yellow.Index;

                foreach (DataColumn column in table.Columns)
                {
                    var cell = headerRow.CreateCell(column.Ordinal);
                    cell.SetCellValue(column.Caption);
                    cell.CellStyle = cellstyle;
                }
                var rowIndex = 1;
                foreach (DataRow row in table.Rows)
                {
                    var dataRow = sheet.CreateRow(rowIndex);
                    foreach (DataColumn column in table.Columns)
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    }
                    rowIndex++;
                }
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }
            return ms;
        }

    }
}
