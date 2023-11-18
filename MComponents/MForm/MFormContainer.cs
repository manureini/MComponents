using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public class MFormContainer : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public RenderFragment<MFormContainerContext> ChildContent { get; set; }

        [Parameter]
        public bool EnableSaveButton { get; set; } = true;

        [Parameter]
        public string SaveButtonText { get; set; } = "Save";

        [Parameter]
        public bool SaveButtonDisabled { get; set; } = false;

        [Parameter]
        public EventCallback<MFormContainerAfterAllFormsSubmittedArgs> OnAfterAllFormsSubmitted { get; set; }

        [Inject]
        public IStringLocalizer L { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        protected MFormContainerContext mFormContext;
        protected bool mIsLoading;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            mFormContext = new MFormContainerContext(ServiceProvider, this);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(mFormContext.GetHashCode());

            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, AdditionalAttributes);

            builder.OpenComponent<CascadingValue<MFormContainerContext>>(3);
            builder.AddAttribute(4, "IsFixed", true);
            builder.AddAttribute(5, "Value", mFormContext);
            builder.AddAttribute(6, "ChildContent", ChildContent.Invoke(mFormContext));
            builder.CloseComponent();

            builder.CloseElement();

            if (EnableSaveButton)
            {
                builder.OpenElement(55, "div");
                builder.AddAttribute(56, "class", "m-form-row m-form-row-actions");

                builder.OpenElement(55, "div");
                builder.AddAttribute(56, "class", "form-group col-12");

                builder.OpenElement(19, "button");
                builder.AddAttribute(20, "type", "button");
                builder.AddAttribute(20, "class", "m-btn m-btn-primary");
                builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, TrySubmit));

                if (SaveButtonDisabled || mIsLoading)
                {
                    builder.AddAttribute(22, "disabled", true);
                }

                builder.AddContent(23, L[SaveButtonText]);
                builder.CloseElement();

                builder.CloseElement(); //div
                builder.CloseElement(); //div
            }

            builder.CloseRegion();
        }

        public async Task<bool> TrySubmit()
        {
            if (mIsLoading)
                return false;

            var result = false;

            try
            {
                mIsLoading = true;
                StateHasChanged();

                result = await mFormContext.NotifySubmit(L);
            }
            finally
            {
                mIsLoading = false;
            }

            return result;
        }

        public bool AllFormsValid
        {
            get
            {
                bool valid = true;

                foreach (var form in mFormContext.Forms) // invoke Validate on all forms
                {
                    valid &= form.Validate();
                }

                return valid;
            }
        }

        public bool HasUnsavedChanges
        {
            get { return mFormContext.Forms.Any(f => f.HasUnsavedChanges); }
        }
    }
}
