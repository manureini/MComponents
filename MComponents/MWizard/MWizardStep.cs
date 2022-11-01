using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MWizard
{
    public class MWizardStep : ComponentBase
    {
        private string mIdentifier;
        [Parameter]
        public string Identifier
        {
            get
            {
                if (mIdentifier == null)
                    return Title;
                return mIdentifier;
            }

            set => mIdentifier = value;
        }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public RenderFragment<MWizardStepContext> Content { get; set; }

        [Parameter]
        public MWizardStepContext StepContext { get; set; }

        [Parameter]
        public Func<bool> IsVisible { get; set; }

        [Parameter]
        public int Position { get; set; } = 0;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private IMWizard mWizard;


        [CascadingParameter]
        public IMWizard Wizard
        {
            get
            {
                return mWizard;
            }
            set
            {
                if (value != mWizard)
                {
                    mWizard = value;
                    mWizard.RegisterStep(this);
                }
            }
        }

        public bool GetIsVisible()
        {
            return IsVisible?.Invoke() ?? true;
        }

        protected override void OnParametersSet()
        {
            if (StepContext == null)
                StepContext = new MWizardStepContext();

            StepContext.CurrentStep = this;
        }
    }
}
