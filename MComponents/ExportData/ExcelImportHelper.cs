using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MGrid;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
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

                ProgressRow(pGrid.L, propInfo, rowValues, (rowVal, pi) =>
                {
                    if (pi.GetCustomAttribute<RequiredAttribute>() != null && rowVal == null)
                    {
                        throw new UserMessageException($"Column {pi.Name} has an empty value!");
                    }
                });
            }

            foreach (var row in rows.Skip(1))
            {
                var rowValues = GetRow(doc, sst, row);

                T obj = pGrid.CreateNewT();

                var proceeded = ProgressRow(pGrid.L, propInfo, rowValues, (object rowVal, IMPropertyInfo pi) =>
                {
                    pi.SetValue(obj, rowVal);
                });

                if (!proceeded)
                {
                    continue;
                }

                pGrid.NewValue = obj;
                await pGrid.OnFormSubmit(new MFormSubmitArgs(null, new Dictionary<string, object>(), obj, true));
            }
        }

        private static bool ProgressRow(IStringLocalizer L, List<IMPropertyInfo> propInfo, List<object> rowValues, Action<object, IMPropertyInfo> pAction)
        {
            if (rowValues == null || rowValues.Count == 0 || rowValues.All(r => r == null))
                return false;

            for (int i = 0; i < rowValues.Count; i++)
            {
                if (i >= rowValues.Count || i >= propInfo.Count)
                    break;

                var rowVal = rowValues[i];
                var pi = propInfo[i];

                if (pi.IsReadOnly)
                    continue;

                try
                {
                    rowVal = ReflectionHelper.ChangeType(rowVal, pi.PropertyType);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw new UserMessageException(L["{0} could not be converted to {1}", rowVal, pi.Name]);
                }

                pAction(rowVal, pi);
            }

            return true;
        }

        private static List<IMPropertyInfo> GetPropertyInfos(Dictionary<IMGridPropertyColumn, IMPropertyInfo> pGridDict, List<object> pValues)
        {
            var ret = new List<IMPropertyInfo>();

            foreach (string propname in pValues)
            {
                var entry = pGridDict.FirstOrDefault(p => p.Key.Property == propname);

                if (entry.Value != null)
                {
                    ret.Add(entry.Value);
                    continue;
                }

                entry = pGridDict.FirstOrDefault(p => p.Key.HeaderText == propname);

                if (entry.Value != null)
                {
                    ret.Add(entry.Value);
                    continue;
                }
            }

            return ret;
        }

        private static List<object> GetRow(SpreadsheetDocument doc, SharedStringTable sst, Row row)
        {
            List<object> values = new List<object>();

            int lastIndex = 1;
            bool addedAny = false;

            foreach (var cell in row.OfType<Cell>())
            {
                int index = lastIndex + 1;

                if (!addedAny)
                {
                    index--;
                }

                if (cell.CellReference != null)
                {
                    index = ColumnIndex(cell.CellReference);
                }

                int distance = index - lastIndex;

                if (distance > 0 && !addedAny)
                {
                    values.Add(null);
                }

                for (int i = (distance - 1); i > 0; i--)
                {
                    values.Add(null);
                }

                lastIndex = index;
                addedAny = true;

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


        private static int ColumnIndex(string reference)
        {
            int ci = 0;
            reference = reference.ToUpper();
            for (int ix = 0; ix < reference.Length && reference[ix] >= 'A'; ix++)
                ci = (ci * 26) + ((int)reference[ix] - 64);
            return ci;
        }
    }
}
