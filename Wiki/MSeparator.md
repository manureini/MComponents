# MSeparator

`MSeparator` visually divides related regions, optionally with content centered on the divider.

## Simple

```razor
<MSeparator />
```

## Advanced

```razor
<MSeparator>
    <h3>Account details</h3>
</MSeparator>
```

## Parameters

| Parameter | Description |
| --- | --- |
| `ChildContent` | Optional heading or other content rendered on the separator. |

The component adds the `empty` class when no content is supplied, allowing the bundled stylesheet to render a plain divider.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L234)
