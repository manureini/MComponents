using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public interface IMGridComplexExport<T>
    {
        Cell GenerateExportCell(SharedStringTablePart pSharedStringTablePart, Dictionary<string, int> pSstCache, T pModel);
    }
}
