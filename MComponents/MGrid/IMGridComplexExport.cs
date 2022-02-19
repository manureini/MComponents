using MComponents.ExportData;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public interface IMGridComplexExport<T>
    {
        object GenerateExportCell(SharedStringTableWrapper pSharedStringTablePart, Dictionary<string, int> pSstCache, T pModel);
    }
}
