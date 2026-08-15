# MSpinner

`MSpinner` renders the standard MComponents loading indicator.

## Simple

```razor
<MSpinner />
```

## Advanced

Show it conditionally around an asynchronous operation:

```razor
<button type="button" @onclick="Refresh" disabled="@isLoading">Refresh</button>

@if (isLoading)
{
    <div class="loading-state">
        <MSpinner />
        <span>Loading data…</span>
    </div>
}

@code {
    private bool isLoading;

    private async Task Refresh()
    {
        isLoading = true;
        try
        {
            await LoadData();
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

`MSpinner` has no parameters. Style `.m-spinner` or a containing element when a different size or layout is needed.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L242)
