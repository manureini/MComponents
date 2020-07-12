using DocumentFormat.OpenXml.Spreadsheet;

namespace MComponents.MGrid
{
    public interface IMGridComplexExport<T>
    {
        Cell GenerateExportCell(T pModel);
    }
}
