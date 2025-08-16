using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace MComponents.MToaster
{
    public partial class ToastContainer : ComponentBase, IDisposable
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

            if (Toaster == null)
            {
                throw new ArgumentNullException("Toaster");
            }

            Toaster.OnToastsUpdated += ToastsUpdated;
        }

        public void Dispose()
        {
            if (Toaster != null)
                Toaster.OnToastsUpdated -= ToastsUpdated;
        }

        protected void ToastsUpdated()
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }
}