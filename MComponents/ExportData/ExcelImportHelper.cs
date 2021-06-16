﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MGrid;
using System;
using System.Collections.Generic;
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
                            Enum.TryParse(pi.PropertyType, rowVal.ToString(), true, out rowVal);
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

            Console.WriteLine("------------");
            foreach (var cell in row.OfType<Cell>())
            {
                int index = ColumnIndex(cell.CellReference);

                Console.WriteLine(cell.CellReference);
                Console.WriteLine(index);

                int distance = index - lastIndex;

                if (distance > 0 && !addedAny)
                {
                    values.Add(null);
                    Console.WriteLine("add extra");
                }

                for (int i = (distance - 1); i > 0; i--)
                {
                    values.Add(null);
                    Console.WriteLine("add");
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
