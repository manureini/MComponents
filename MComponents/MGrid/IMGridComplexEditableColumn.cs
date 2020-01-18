using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MForm;
using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMGridComplexEditableColumn<TProperty>
    {
        RenderFragment<MComplexPropertyFieldContext<TProperty>> FormTemplate { get; }
    }
}
