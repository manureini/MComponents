using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MComponents
{
    internal static class ExcelExportHelper
    {
        public static byte[] GetExcelSpreadsheet<T>(IEnumerable<IMGridColumn> pColumns, IDictionary<IMGridPropertyColumn, IMPropertyInfo> pPropertyInfos, IEnumerable<T> pData, IMGridObjectFormatter<T> pFormatter)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
                {
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    var stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylesPart.Stylesheet = new Stylesheet
                    {
                        Fonts = new Fonts(new Font()),
                        Fills = new Fills(new Fill()),
                        Borders = new Borders(new Border()),
                        CellStyleFormats = new CellStyleFormats(new CellFormat()),
                        CellFormats =
                            new CellFormats(
                                new CellFormat(),
                                new CellFormat
                                {
                                    NumberFormatId = 14,
                                    ApplyNumberFormat = true
                                })
                    };

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

                    var sheetData = new SheetData();

                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    var sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet 1"
                    };
                    sheets.AppendChild(sheet);

                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);

                    pColumns = pColumns.Where(c => c is IMGridPropertyColumn);

                    foreach (var headerrow in pColumns.Select(c => c.HeaderText))
                    {
                        row.AppendChild(CreateTextCell(headerrow ?? string.Empty));
                    }

                    foreach (var rowData in pData)
                    {
                        row = new Row { RowIndex = ++rowIdex };
                        sheetData.AppendChild(row);

                        foreach (IMGridPropertyColumn column in pColumns)
                        {
                            var iprop = pPropertyInfos[column];

                            Cell cell;

                            if (iprop.PropertyType == typeof(DateTime) || iprop.PropertyType == typeof(DateTime?))
                            {
                                var datetime = iprop.GetValue(rowData) as DateTime?;
                                cell = CreateDateCell(datetime);
                            }
                            else if (iprop.PropertyType == typeof(int) || iprop.PropertyType == typeof(int?))
                            {
                                var value = iprop.GetValue(rowData) as int?;
                                string strvalue = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                                cell = CreateNumberCell(strvalue);
                            }
                            else if (iprop.PropertyType == typeof(long) || iprop.PropertyType == typeof(long?))
                            {
                                var value = iprop.GetValue(rowData) as long?;
                                string strvalue = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                                cell = CreateNumberCell(strvalue);
                            }
                            else if (iprop.PropertyType == typeof(float) || iprop.PropertyType == typeof(float?))
                            {
                                var value = iprop.GetValue(rowData) as float?;
                                string strvalue = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                                cell = CreateNumberCell(strvalue);
                            }
                            else if (iprop.PropertyType == typeof(double) || iprop.PropertyType == typeof(double?))
                            {
                                var value = iprop.GetValue(rowData) as double?;
                                string strvalue = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                                cell = CreateNumberCell(strvalue);
                            }
                            else
                            {
                                string cellValue = pFormatter.FormatPropertyColumnValue(column, iprop, rowData);
                                cell = CreateTextCell(cellValue ?? string.Empty);
                            }

                            row.AppendChild(cell);
                        }
                    }

                    workbookpart.Workbook.Save();

                    document.Save();
                    document.Close();
                }

                return ms.ToArray();
            }
        }

        private static Cell CreateTextCell(string text)
        {
            var cell = new Cell
            {
                DataType = CellValues.InlineString,
            };

            var istring = new InlineString();
            var t = new Text { Text = text };
            istring.AppendChild(t);
            cell.AppendChild(istring);
            return cell;
        }

        private static Cell CreateDateCell(DateTime? pDate)
        {
            var cell = new Cell
            {
                DataType = CellValues.Date,
                CellValue = new CellValue(pDate?.ToString("s") ?? string.Empty),
                StyleIndex = 1
            };

            return cell;
        }

        private static Cell CreateNumberCell(string pValue)
        {
            var cell = new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(pValue),
            };

            return cell;
        }
    }
}
