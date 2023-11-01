using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.ExportData;
using MComponents.MGrid;
using MComponents.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MComponents
{
    internal static class ExcelExportHelper
    {
        public static Stream GetExcelSpreadsheet<T>(IEnumerable<IMGridColumn> pColumns, IDictionary<IMGridPropertyColumn, IMPropertyInfo> pPropertyInfos, IEnumerable<T> pData, IMGridObjectFormatter<T> pFormatter)
        {
            var columns = pColumns.Where(c => c.VisibleInExport);

            MemoryStream ms = new MemoryStream();

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
                                NumberFormatId = 14,
                                ApplyNumberFormat = true
                            },
                            new CellFormat()
                            {
                                NumberFormatId = 22,
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

                var sstWrapper = new SharedStringTableWrapper()
                {
                    SharedStringTable = sst
                };

                foreach (var headerrow in columns.Select(c => c.HeaderText))
                {
                    hrow.AppendChild((Cell)CreateTextCell(sstWrapper, sstCache, headerrow ?? string.Empty));
                }

                foreach (var rowData in pData)
                {
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);

                    foreach (var column in columns)
                    {
                        if (column is IMGridComplexExport<T> exporter)
                        {
                            row.AppendChild((Cell)exporter.GenerateExportCell(sstWrapper, sstCache, rowData));
                        }
                        else if (column is IMGridPropertyColumn propColumn)
                        {
                            var iprop = pPropertyInfos[propColumn];
                            Cell cell = GetPropertyColumnCell(pFormatter, rowData, propColumn, iprop, sstWrapper, sstCache);
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
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static Cell GetPropertyColumnCell<T>(IMGridObjectFormatter<T> pFormatter, T rowData, IMGridPropertyColumn popcolumn, IMPropertyInfo iprop, SharedStringTableWrapper pSsTable, Dictionary<string, int> pSstCache)
        {
            Cell cell;
            if (iprop.PropertyType == typeof(DateTime) || iprop.PropertyType == typeof(DateTime?))
            {
                var datetime = iprop.GetValue(rowData) as DateTime?;

                if (iprop.GetCustomAttribute<DateTimeAttribute>() != null)
                {
                    cell = CreateDateTimeCell(datetime);
                }
                else
                {
                    cell = CreateDateCell(datetime);
                }
            }
            else if (iprop.PropertyType == typeof(DateOnly) || iprop.PropertyType == typeof(DateOnly?))
            {
                var value = iprop.GetValue(rowData) as DateOnly?;
                var dateTime = value?.ToDateTime(TimeOnly.MinValue);
                cell = CreateDateCell(dateTime);
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
                cell = (Cell)CreateTextCell(pSsTable, pSstCache, cellValue ?? string.Empty);
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

        public static object CreateTextCell(SharedStringTableWrapper pSsTable, Dictionary<string, int> pCache, string text)
        {
            var ssTable = pSsTable.SharedStringTable as SharedStringTablePart;

            int ssIndex = InsertSharedStringItem(ssTable, pCache, text);

            var cell = new Cell
            {
                DataType = CellValues.SharedString,
                CellValue = new CellValue(ssIndex.ToString()),
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
                try
                {
                    value = pDate.Value.Date.ToOADate().ToString(CultureInfo.InvariantCulture);
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
                StyleIndex = 1, //must match to CellFormats
            };

            return cell;
        }

        public static Cell CreateDateTimeCell(DateTime? pDate)
        {
            var value = string.Empty;

            if (pDate != null)
            {
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
                StyleIndex = 2,
            };

            return cell;
        }

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
