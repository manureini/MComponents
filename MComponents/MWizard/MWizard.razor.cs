using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MComponents.MWizard
{
    public partial class MWizard
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public RenderFragment Steps { get; set; }

        [Parameter]
        public EventCallback<StepChangedArgs> OnStepChanged { get; set; }

        [Parameter]
        public EventCallback<SubmitEventArgs> OnSubmit { get; set; }

        [Parameter]
        public bool EnableJumpToAnyStep { get; set; }

        [Parameter]
        public RenderFragment ButtonPrev { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardPrevButton>(30);
            r.CloseComponent();
        };

        [Parameter]
        public RenderFragment ButtonNext { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardNextButton>(38);
            r.CloseComponent();
        };

        [Parameter]
        public RenderFragment ButtonFinish { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardFinishButton>(45);
            r.CloseComponent();
        };

        public bool FreezeCurrentStep { get; set; }

        public int CurrentStep { get; protected set; }

        public List<MWizardStep> WizardSteps { get; protected set; } = new List<MWizardStep>();

        protected SemaphoreSlim mLocker = new SemaphoreSlim(1, 1);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JsRuntime.InvokeVoidAsync("mcomponents.registerKeyListener", DotNetObjectReference.Create(this));
        }

        [JSInvokable]
        public void JsInvokeKeyDown(string pKey)
        {
            if (pKey == "Escape")
                OnPrevClicked();
        }

        public void RegisterStep(MWizardStep pStep)
        {
            if (WizardSteps.Any(s => s.Identifier == pStep.Identifier))
                return;

            WizardSteps.Add(pStep);
            WizardSteps = WizardSteps.OrderBy(s => s.Position).ToList();

            if (CurrentStep == -1 && pStep.IsVisible)
                CurrentStep = WizardSteps.Count - 1;

            StateHasChanged();
        }

        internal async void OnNextClicked()
        {
            if (FreezeCurrentStep)
                return;

            await mLocker.WaitAsync();

            try
            {
                int oldStep = CurrentStep;

                int? next = FindNextVisible(CurrentStep, i => i + 1);
                if (!next.HasValue)
                    return;

                int newStep = next.Value;

                var args = new StepChangedArgs()
                {
                    OldStepIndex = oldStep,
                    NewStepIndex = newStep,
                    OldStep = WizardSteps[oldStep],
                    NewStep = WizardSteps[newStep],
                    UserInteract = true
                };

                await OnStepChanged.InvokeAsync(args);
                //   task.Wait();

                if (args.Cancelled)
                    return;

                CurrentStep = newStep;

                StateHasChanged();
            }
            finally
            {
                mLocker.Release();
            }
        }

        internal async void OnPrevClicked()
        {
            if (FreezeCurrentStep)
                return;

            await mLocker.WaitAsync();
            try
            {
                int oldStep = CurrentStep;

                int? prev = FindNextVisible(CurrentStep, i => i - 1);
                if (!prev.HasValue)
                    return;

                int newStep = prev.Value;

                var args = new StepChangedArgs()
                {
                    OldStepIndex = oldStep,
                    NewStepIndex = newStep,
                    OldStep = WizardSteps[oldStep],
                    NewStep = WizardSteps[newStep],
                    UserInteract = true
                };

                await OnStepChanged.InvokeAsync(args);

                if (args.Cancelled)
                    return;

                CurrentStep = newStep;

                StateHasChanged();
            }
            finally
            {
                mLocker.Release();
            }
        }

        protected void OnJumpToClicked(int pIndex)
        {
            if (FreezeCurrentStep)
                return;

            if (!CanJumpTo(pIndex))
                return;

            SetCurrentStep(pIndex, true);
        }

        public async void OnFinishClicked()
        {
            if (FreezeCurrentStep)
                return;

            await OnSubmit.InvokeAsync(new SubmitEventArgs());

            StateHasChanged();
        }

        protected string GetCurrentState()
        {
            if (FindNextVisible(CurrentStep, i => i - 1) == null)
                return "first";

            if (FindNextVisible(CurrentStep, i => i + 1) == null)
                return "last";

            return "between";
        }


        protected bool CanJumpTo(int pIndex)
        {
            if (EnableJumpToAnyStep)
                return true;

            if (FindNextVisible(CurrentStep, i => i - 1) == pIndex)
                return true;

            if (FindNextVisible(CurrentStep, i => i + 1) == pIndex)
                return true;

            return false;
        }

        public void InvokeStateHasChanged()
        {
            StateHasChanged();
        }

        protected int? FindNextVisible(int pStartIndex, Func<int, int> pModifier)
        {
            int index = pStartIndex;

            while (true)
            {
                index = pModifier(index);

                if (index < 0 || index >= WizardSteps.Count)
                    return null;

                if (WizardSteps[index].IsVisible)
                    return index;
            }
        }

        public void SetCurrentStep(int pIndex, bool pUserInteract)
        {
            if (FreezeCurrentStep)
                return;

            Task.Run(async () =>
            {

                await mLocker.WaitAsync();
                try
                {
                    if (pIndex == CurrentStep)
                        return;

                    int oldStep = CurrentStep;
                    int newStep = pIndex;

                    var args = new StepChangedArgs()
                    {
                        OldStepIndex = oldStep,
                        NewStepIndex = newStep,
                        OldStep = WizardSteps[oldStep],
                        NewStep = WizardSteps[newStep],
                        UserInteract = pUserInteract
                    };

                    await InvokeAsync(async () =>
                    {
                        await OnStepChanged.InvokeAsync(args);
                    });

                    if (args.Cancelled)
                        return;

                    CurrentStep = newStep;

                    await InvokeAsync(() =>
                    {
                        StateHasChanged();
                    });
                }
                finally
                {
                    mLocker.Release();
                }
            });
        }

    }
}
