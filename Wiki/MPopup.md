# MPopup

`MPopup` shows a floating content panel when its target is selected and closes it when focus leaves the popup.

## Simple

```razor
<MPopup>
    <Target>
        <button type="button">Open menu</button>
    </Target>
    <PopupContent>
        <a href="/">Home</a>
    </PopupContent>
</MPopup>
```

## Advanced

```razor
<MPopup @ref="languagePopup" CssClass="m-popup language-popup">
    <Target>
        <button type="button">Choose language</button>
    </Target>
    <PopupContent>
        <button @onclick="SelectEnglish">English</button>
        <button @onclick="SelectGerman">German</button>
    </PopupContent>
</MPopup>

@code {
    private MPopup languagePopup;

    private void SelectEnglish()
    {
        // Update application state.
        languagePopup.Hide();
    }

    private void SelectGerman()
    {
        // Update application state.
        languagePopup.Hide();
    }
}
```

## Parameters and methods

| Member | Description |
| --- | --- |
| `Target` | Always-visible element that toggles the popup. |
| `PopupContent` | Content shown in the floating panel. |
| `CssClass` | Classes applied to the root element. |
| `Show()`, `Hide()`, `Toggle()` | Programmatic visibility controls. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L164)
