using Microsoft.AspNetCore.Components;

namespace MComponents.MForm
{
    public interface IMFieldGenerator<T> : IMField
    {
        RenderFragment<MFieldGeneratorContext<T>> Template { get; }
    }
}
