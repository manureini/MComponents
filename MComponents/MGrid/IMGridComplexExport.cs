using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;

namespace MComponents.MGrid
{
    public interface IMGridComplexExport<T>
    {
        Cell GenerateExportCell(T pModel);
    }
}
