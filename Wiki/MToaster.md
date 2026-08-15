# MToaster

`IToaster` displays transient info, success, warning, and error messages through the global `MComponentsRoot`.

## Setup

Add the root component once in the application layout:

```razor
<MComponentsRoot />
```

Inject `IToaster` where notifications are raised.

## Simple

```razor
@inject IToaster Toaster

<button @onclick="ShowToast">Notify</button>

@code {
    private void ShowToast()
        => Toaster.Info("The operation started.", "Information");
}
```

## Advanced

```razor
@inject IToaster Toaster

<button @onclick="ShowSaved">Save</button>
<button @onclick="Toaster.Clear">Clear all</button>

@code {
    private void ShowSaved()
        => Toaster.Success("Select this toast to open the item.", "Saved", options =>
        {
            options.RequireInteraction = true;
            options.ShowProgressBar = false;
            options.Onclick = toast => OpenSavedItem();
        });

    private Task OpenSavedItem() => Task.CompletedTask;
}
```

## API

| Member | Description |
| --- | --- |
| `Info`, `Success`, `Warning`, `Error` | Adds a typed toast with optional title and configuration. |
| `Add` | Adds a toast with an explicit `ToastType`. |
| `Clear` | Removes visible and queued toasts. |
| `Remove` | Removes a specific toast. |
| `ShownToasts` | Current visible toast sequence. |
| `Configuration` | Global toaster configuration. |

Per-toast options control interaction, durations, progress, close icons, CSS classes, opacity, and click callbacks.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L270)
