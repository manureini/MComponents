using MComponents.MTooltip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Services
{
    internal class TooltipService
    {
        public IDictionary<Guid, TooltipReference> TooltipReferences = new ConcurrentDictionary<Guid, TooltipReference>();

        public TooltipContainer TooltipContainer { get; internal set; }

        public void ShowTooltip(TooltipReference pTooltipReference)
        {
            TooltipReferences.TryAdd(pTooltipReference.Id, pTooltipReference);
            TooltipContainer?.InvokeStateHasChanged();
        }

        public void HideTooltip(Guid Id)
        {
            TooltipReferences.Remove(Id);
            TooltipContainer?.InvokeStateHasChanged();
        }
    }
}
