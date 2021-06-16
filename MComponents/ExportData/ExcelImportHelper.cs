using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MComponents.MGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.ExportData
{
    internal static class ExcelImportHelper
    {
        public static async Task ImportFile<T>(MGrid<T> pGrid, Stream pFile)
        {
            using var doc = SpreadsheetDocument.Open(pFile, false);

            var worksheetPart = doc.WorkbookPart.WorksheetParts.FirstOrDefault();

            var worksheet = worksheetPart.Worksheet;

            SharedStringTablePart sstpart = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            SharedStringTable sst = sstpart?.SharedStringTable;

            var sheetdata = worksheet.OfType<SheetData>().FirstOrDefault();

            var rows = sheetdata.OfType<Row>();

            var firstValues = GetRow(doc, sst, rows.First());

            var propInfo = GetPropertyInfos(pGrid.PropertyInfos, firstValues);

            if (!propInfo.Any())
                return;

            foreach (var row in rows.Skip(1))
            {
                var rowValues = GetRow(doc, sst, row);

                T obj = pGrid.CreateNewT();

                for (int i = 0; i < rowValues.Count; i++)
                {
                    if (i >= rowValues.Count || i >= propInfo.Count)
                        break;

                    var rowVal = rowValues[i];
                    var pi = propInfo[i];

                    if (pi.IsReadOnly)
                        continue;

                    if (rowVal != null)
                    {
                        if (pi.PropertyType.IsEnum)
                        {
                            rowVal = Enum.Parse(pi.PropertyType, rowVal.ToString());
                        }
                        else
                        {
                            rowVal = Convert.ChangeType(rowVal, pi.PropertyType);
                        }
                    }

                    pi.SetValue(obj, rowVal);
                }

                pGrid.NewValue = obj;
                await pGrid.OnFormSubmit(new MFormSubmitArgs(null, new Dictionary<string, object>(), obj, true));
            }
        }

        private static List<IMPropertyInfo> GetPropertyInfos(Dictionary<IMGridPropertyColumn, IMPropertyInfo> pGridDict, List<object> pValues)
        {
            var ret = new List<IMPropertyInfo>();

            foreach (string propname in pValues)
            {
                var entry = pGridDict.FirstOrDefault(p => p.Value.Name == propname);

                if (entry.Value != null)
                    ret.Add(entry.Value);
            }

            return ret;
        }
        private static List<object> GetRow(SpreadsheetDocument doc, SharedStringTable sst, Row row)
        {
            List<object> values = new List<object>();

            foreach (var cell in row.OfType<Cell>())
            {
                object value = null;

                if (cell.DataType != null && cell.DataType == CellValues.InlineString)
                {
                    value = cell.InlineString.InnerText;
                }
                else if (cell.CellValue != null)
                {
                    value = cell.CellValue.Text;

                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                    {
                        int ssid = int.Parse(cell.CellValue.Text);
                        value = sst.ChildElements[ssid].InnerText;
                    }

                    if (cell.StyleIndex != null)
                    {
                        var cellFormat = doc.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ChildElements[int.Parse(cell.StyleIndex.InnerText)] as CellFormat;

                        var dateFormat = ExcelHelper.GetDateTimeFormat(cellFormat.NumberFormatId);

                        if (dateFormat != null)
                        {
                            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
                            {
                                value = DateTime.FromOADate(doubleVal);
                            }
                            else
                            {
                                if (DateTime.TryParseExact(value.ToString(), dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeVal))
                                {
                                    value = dateTimeVal;
                                }
                            }
                        }
                    }
                }

                values.Add(value);
            }

            return values;
        }

    }
}
