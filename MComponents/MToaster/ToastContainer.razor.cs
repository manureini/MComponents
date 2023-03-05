using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace MComponents.MToaster
{
    public class ToastContainerModel : ComponentBase, IDisposable
    {
        [Inject]
        private IToaster Toaster { get; set; }

        protected IEnumerable<Toast> Toasts => Toaster.Configuration.NewestOnTop
                ? Toaster.ShownToasts.Reverse()
                : Toaster.ShownToasts;

        protected string Class => Toaster.Configuration.PositionClass;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Toaster.OnToastsUpdated += () => InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Toaster.OnToastsUpdated -= StateHasChanged;
        }
    }
}