using MComponents.MForm;
using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMComplexColumn<T>
    {
        RenderFragment<MComplexPropertyFieldContext<T>> FormTemplate { get; }
    }
}
