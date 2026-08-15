# MBadge

`MBadge` renders short status or count text using the MComponents badge styling.

## Simple

```razor
<MBadge>New</MBadge>
```

## Advanced

Use `CssClass` for application styling and unmatched HTML attributes for accessibility or browser behavior:

```razor
<MBadge CssClass="text-uppercase"
        title="Three pending items"
        aria-label="Three pending items">
    3 pending
</MBadge>
```

## Parameters

| Parameter | Description |
| --- | --- |
| `ChildContent` | Badge content. |
| `CssClass` | Additional CSS classes. |
| Additional attributes | Attributes such as `title`, `id`, and `aria-*` are added to the badge element. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L45)
