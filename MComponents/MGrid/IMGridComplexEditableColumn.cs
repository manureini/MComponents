using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MForm;
using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMGridComplexEditableColumn<T, TProperty>
    {
        RenderFragment<MComplexPropertyFieldContext<T, TProperty>> FormTemplate { get; }
    }
}
