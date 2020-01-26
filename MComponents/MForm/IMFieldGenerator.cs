using Microsoft.AspNetCore.Components;

namespace MComponents.MForm
{
    public interface IMFieldGenerator : IMField
    {
        RenderFragment<MFieldGeneratorContext> Template { get; }
    }
}
