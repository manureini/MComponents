using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.ExportData;
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
        private const int DATE_NUMBER_FORMAT = 14;

        public static byte[] GetExcelSpreadsheet<T>(IEnumerable<IMGridColumn> pColumns, IDictionary<IMGridPropertyColumn, IMPropertyInfo> pPropertyInfos, IEnumerable<T> pData, IMGridObjectFormatter<T> pFormatter)
        {
            var columns = pColumns.Where(c => c.VisibleInExport);

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
                                new CellFormat()
                                {
                                    NumberFormatId = DATE_NUMBER_FORMAT,
                                    ApplyNumberFormat = true
                                },
                                new CellFormat()
                                {
                                    NumberFormatId = 49,
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

                    var sst = workbookpart.AddNewPart<SharedStringTablePart>();
                    sst.SharedStringTable = new SharedStringTable();

                    var sstCache = new Dictionary<string, int>();

                    UInt32 rowIdex = 0;
                    var hrow = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(hrow);

                    foreach (var headerrow in columns.Select(c => c.HeaderText))
                    {
                        hrow.AppendChild(CreateTextCell(sst, sstCache, headerrow ?? string.Empty));
                    }

                    foreach (var rowData in pData)
                    {
                        var row = new Row { RowIndex = ++rowIdex };
                        sheetData.AppendChild(row);

                        foreach (var column in columns)
                        {
                            if (column is IMGridComplexExport<T> exporter)
                            {
                                row.AppendChild(exporter.GenerateExportCell(sst, sstCache, rowData));
                            }
                            else if (column is IMGridPropertyColumn propColumn)
                            {
                                var iprop = pPropertyInfos[propColumn];
                                Cell cell = GetPropertyColumnCell(pFormatter, rowData, propColumn, iprop, sst, sstCache);
                                row.AppendChild(cell);
                            }
                            else
                            {
                                row.AppendChild(CreateGeneralCell(string.Empty));
                            }
                        }
                    }

                    sst.SharedStringTable.Save();
                    workbookpart.Workbook.Save();

                    document.Save();
                    document.Close();
                }

                return ms.ToArray();
            }
        }

        private static Cell GetPropertyColumnCell<T>(IMGridObjectFormatter<T> pFormatter, T rowData, IMGridPropertyColumn popcolumn, IMPropertyInfo iprop, SharedStringTablePart pSsTable, Dictionary<string, int> pSstCache)
        {
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
            else if (iprop.PropertyType == typeof(string))
            {
                string cellValue = pFormatter.FormatPropertyColumnValue(popcolumn, iprop, rowData);
                cell = CreateTextCell(pSsTable, pSstCache, cellValue ?? string.Empty);
            }
            else
            {
                string cellValue = pFormatter.FormatPropertyColumnValue(popcolumn, iprop, rowData);
                cell = CreateGeneralCell(cellValue ?? string.Empty);
            }

            return cell;
        }

        public static Cell CreateGeneralCell(string text)
        {
            var istring = new InlineString();
            var t = new Text { Text = text };
            istring.AppendChild(t);

            var cell = new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = istring
            };

            return cell;
        }

        public static Cell CreateTextCell(SharedStringTablePart pSsTable, Dictionary<string, int> pCache, string text)
        {
            int ssIndex = InsertSharedStringItem(pSsTable, pCache, text);

            var cell = new Cell
            {
                DataType = CellValues.SharedString,
                CellValue = new CellValue(ssIndex.ToString()),
                StyleIndex = 2
            };

            return cell;
        }

        private static int InsertSharedStringItem(SharedStringTablePart pShareStringPart, Dictionary<string, int> pCache, string pText)
        {
            if (pCache.ContainsKey(pText))
            {
                return pCache[pText];
            }

            pShareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(pText)));

            int index = pCache.Count;
            pCache.Add(pText, index);

            return index;
        }

        public static Cell CreateDateCell(DateTime? pDate)
        {
            var value = string.Empty;

            if (pDate != null)
            {
                pDate = new DateTime(pDate.Value.Year, pDate.Value.Month, pDate.Value.Day);

                try
                {
                    value = pDate.Value.ToOADate().ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            var cell = new Cell
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(value),
                StyleIndex = 1, //must match to date number format
            };

            return cell;
        }

        /*
        public static Cell CreateDateCell(DateTime? pDate)
        {
            //var formatString = ExcelHelper.GetDateTimeFormat(DATE_NUMBER_FORMAT);

            var cell = new Cell
            {
                DataType = CellValues.Date,
                CellValue = new CellValue(pDate?.ToString("s",  CultureInfo.InvariantCulture) ?? string.Empty),
                StyleIndex = 1
            };

            return cell;
        }
        */

        public static Cell CreateNumberCell(string pValue)
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
