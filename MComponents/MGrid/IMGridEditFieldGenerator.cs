using MComponents.MForm;
using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public interface IMGridEditFieldGenerator<T>
    {
        RenderFragment<MFieldGeneratorContext> Template(int? pSize);
    }
}
