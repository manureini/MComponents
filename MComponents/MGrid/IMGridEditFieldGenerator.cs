using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMGridEditFieldGenerator<T>
    {
        RenderFragment EditFieldTemplate(bool pIsFilterRow);
    }
}
