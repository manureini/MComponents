# MTabs

`MTabs` switches between `MTab` content panels and can synchronize the active tab with the URL fragment.

## Simple

```razor
<MTabs>
    <MTab Title="Overview">Overview content</MTab>
    <MTab Title="Details">Details content</MTab>
</MTabs>
```

## Advanced

```razor
<MTabs SyncTabsWithFragmentIdentifierInUrl="true" CssClass="settings-tabs">
    <MTab Title="General" FragmentIdentifier="general">
        General settings
    </MTab>
    <MTab Title="Notifications"
          FragmentIdentifier="notifications"
          CssClassButton="text-uppercase">
        Notification settings
    </MTab>
</MTabs>
```

When synchronization is enabled and `FragmentIdentifier` is omitted, a value is generated from the tab title.

## Parameters

| Component | Parameter | Description |
| --- | --- | --- |
| `MTabs` | `ChildContent` | `MTab` declarations. |
| `MTabs` | `SyncTabsWithFragmentIdentifierInUrl` | Reads and updates the active URL fragment. |
| `MTabs` | `CssClass` | Additional class for the tabs container. |
| `MTab` | `Title` | Button text. |
| `MTab` | `FragmentIdentifier` | Fragment associated with the tab. |
| `MTab` | `CssClassButton` | Additional class for the tab button. |

URL synchronization requires `RegisterNavigation = true` in `AddMComponents`.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L254)
