using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMGridColumnGenerator<T> : IMGridColumn
    {
        RenderFragment GenerateContent(T pModel);
    }
}
