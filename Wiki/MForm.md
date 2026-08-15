# MForm

`MForm<T>` generates Blazor form controls from a model and its data-annotation and MComponents attributes. `MFormContainer` coordinates submission of one or more forms.

## Simple

```razor
<MFormContainer SaveButtonText="Save">
    <MForm Model="profile" />
</MFormContainer>

@code {
    private Profile profile = new();

    public sealed class Profile
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
```

## Advanced

Declare fields when layout or field selection must be controlled:

```razor
<MFormContainer SaveButtonText="Validate and save"
                OnAfterAllFormsSubmitted="OnSubmitted">
    <MForm Model="profile" UpdateOnInput="true" StoreOriginalValues="true">
        <Fields>
            <MFieldRow>
                <MField Property="@nameof(Profile.Name)" />
                <MField Property="@nameof(Profile.Email)" />
            </MFieldRow>
            <MField Property="@nameof(Profile.Notes)" />
        </Fields>
    </MForm>
</MFormContainer>
```

## Important parameters

| Parameter | Description |
| --- | --- |
| `Model` | Model instance edited by the form. |
| `Fields` | Optional explicit `MField`, `MFieldRow`, or `MFieldGenerator` declarations. |
| `EnableValidation` | Enables data-annotation validation. |
| `EnableValidationSummary` | Shows the validation summary. |
| `OnValidSubmit` | Called after this form validates successfully. |
| `OnValueChanged` | Called when a generated field changes. |
| `PreventDefaultRendering` | Renders only explicitly declared fields. |
| `UpdateOnInput` | Updates values on input instead of change. |
| `StoreOriginalValues` | Enables unsaved-change tracking. |

Use MComponents.Shared attributes such as `Row`, `ReadOnly`, `TextArea`, `Date`, and `Time` to customize generated controls.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L77)
