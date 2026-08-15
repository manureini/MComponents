# MTooltip

`MTooltip` shows text or rich content while the pointer is over its child content.

## Setup

Place `<MComponentsRoot />` once in the layout. It contains the shared tooltip container.

## Simple

```razor
<MTooltip Text="A plain text tooltip">
    Hover over this text
</MTooltip>
```

## Advanced

```razor
<MTooltip CssClass="product-tooltip">
    <ChildContent>
        <button type="button">Product details</button>
    </ChildContent>
    <TooltipContent>
        <strong>@product.Name</strong>
        <div>@product.Description</div>
    </TooltipContent>
</MTooltip>
```

## Parameters

| Parameter | Description |
| --- | --- |
| `ChildContent` | Element that owns the hover interaction. |
| `Text` | Plain tooltip text. |
| `TooltipContent` | Rich tooltip render fragment. |
| `CssClass` | Additional class applied to the floating tooltip. |

Use either `Text` or `TooltipContent`. Rich content is useful for formatting, but interactive actions should generally remain in a popup rather than a hover-only tooltip.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L281)
