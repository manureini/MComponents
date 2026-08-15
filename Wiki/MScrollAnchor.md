# MScrollAnchor

`MScrollAnchor` marks a destination and scrolls it into view when the page is opened with the matching URL fragment.

## Simple

```razor
<a href="/orders#validation">Go to validation</a>

<MScrollAnchor Id="validation" />
<ValidationSummary />
```

## Advanced

Place anchors near dynamically rendered regions so deep links return a user to the relevant state:

```razor
@if (showAdvancedSettings)
{
    <MScrollAnchor Id="advanced-settings" />
    <AdvancedSettings Model="settings" />
}
```

The `Navigation` service must be registered through `AddMComponents`:

```csharp
services.AddMComponents(options =>
{
    options.RegisterNavigation = true;
});
```

## Parameters

| Parameter | Description |
| --- | --- |
| `Id` | Unique anchor identifier used by links and JavaScript scrolling. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L206)
