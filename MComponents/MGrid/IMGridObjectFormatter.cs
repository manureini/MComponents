using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.MGrid
{
    public interface IMGridObjectFormatter<T>
    {
        string FormatPropertyColumnValue(IMGridPropertyColumn pColumn, IMPropertyInfo pPropertyInfo, T pRow);

        void AppendToTableRow(RenderTreeBuilder pBuilder, ref string pCssClass, T pRow, bool pSelected);

        void AppendToTableRowData(RenderTreeBuilder pBuilder, IMGridColumn pColumn, T pRow);
    }
}