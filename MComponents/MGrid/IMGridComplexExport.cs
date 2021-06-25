using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MComponents.MGrid
{
    public interface IMGridComplexExport<T>
    {
        Cell GenerateExportCell(SharedStringTablePart pSharedStringTablePart, T pModel);
    }
}
