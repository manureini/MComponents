# MWizard

`MWizard` presents an ordered workflow made of `MWizardStep` components with previous, next, and finish actions.

## Simple

```razor
<MWizard>
    <Steps>
        <MWizardStep Title="Start">
            <Content>Welcome to the wizard.</Content>
        </MWizardStep>
        <MWizardStep Title="Finish">
            <Content>Review and finish.</Content>
        </MWizardStep>
    </Steps>
</MWizard>
```

## Advanced

```razor
<MWizard EnableJumpToAnyStep="true"
         OnStepChanged="StepChanged"
         OnSubmit="Submit">
    <Steps>
        <MWizardStep Title="Account" Position="1"
                     OnStepLeave="ValidateAccount">
            <Content Context="step">Account fields</Content>
        </MWizardStep>
        <MWizardStep Title="Options" Position="2"
                     IsVisible="() => showOptions">
            <Content Context="step">Optional features</Content>
        </MWizardStep>
        <MWizardStep Title="Review" Position="3">
            <Content Context="step">Review the values</Content>
        </MWizardStep>
    </Steps>
    <ButtonFinish>
        <MWizardFinishButton Caption="Create account" />
    </ButtonFinish>
</MWizard>
```

`StepChangedArgs` can cancel a transition or delay it while asynchronous work completes.

## Parameters

| Component | Parameter | Description |
| --- | --- | --- |
| `MWizard` | `Steps` | `MWizardStep` declarations. |
| `MWizard` | `OnStepChanged` | Called before a step transition. |
| `MWizard` | `OnSubmit` | Called by the finish action. |
| `MWizard` | `EnableJumpToAnyStep` | Allows direct selection of any visible step. |
| `MWizard` | `ButtonPrev`, `ButtonNext`, `ButtonFinish` | Custom action fragments. |
| `MWizardStep` | `Title`, `Content` | Step heading and typed content. |
| `MWizardStep` | `IsVisible` | Function controlling conditional steps. |
| `MWizardStep` | `OnStepEnter`, `OnStepLeave` | Step lifecycle callbacks. |
| `MWizardStep` | `Position` | Explicit ordering value. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L295)
