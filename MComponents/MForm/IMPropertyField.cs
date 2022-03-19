using Microsoft.AspNetCore.Components;
using System;

namespace MComponents.MForm
{
    public interface IMPropertyField : IMField
    {
        string Property { get; set; }

        Type PropertyType { get; set; }

        RenderFragment TemplateAfterLabel { get; set; }
    }
}