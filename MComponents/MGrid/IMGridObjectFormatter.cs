using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public interface IMGridObjectFormatter<T>
    {
        IStringLocalizer<MComponentsLocalization> L { get; set; }

        string FormatPropertyColumnValue(IMGridPropertyColumn pColumn, IMPropertyInfo pPropertyInfo, T pRow);

        void AppendToTableRow(RenderTreeBuilder pBuilder, ref string pCssClass, T pRow, bool pSelected);

        void AppendToTableRowData(RenderTreeBuilder pBuilder, IMGridColumn pColumn, T pRow);

        void AddRowMetadata(T pRow, object pValue);

        void RemoveRowMetadata(T pRow);

        void ClearRowMetadata();
    }
}