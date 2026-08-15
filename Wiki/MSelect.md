# MSelect

`MSelect<T>` is a typed single- or multiple-selection input with search, custom formatting, and enum support.

## Simple

```razor
<MSelect T="string"
         Options="values"
         @bind-Value="selected" />

@code {
    private string[] values = { "First", "Second", "Third" };
    private string selected;
}
```

## Advanced

```razor
<MSelect T="Product"
         Options="products"
         @bind-Values="selectedProducts"
         EnableSearch="true"
         EnableSelectAll="true"
         Property="@nameof(Product.Name)"
         CustomFormatter="FormatProduct"
         OnSelectionChanged="SelectionChanged" />

@code {
    private ICollection<Product> selectedProducts = new List<Product>();

    private string FormatProduct(Product product)
        => $"{product.Name} ({product.Sku})";
}
```

## Important parameters

| Parameter | Description |
| --- | --- |
| `Options` | Available values. Enum values are discovered automatically when omitted. |
| `Value` / `Values` | Single or multiple bound selection. |
| `Property` | Property displayed for object options. |
| `EnableSearch` | Displays a search input. |
| `EnableSelectAll` | Adds a select-all action in multiple-selection mode. |
| `IsDisabled` | Prevents interaction. |
| `NullValueDescription` | Text shown for the null option. |
| `CustomFormatter` | Converts an option to its display string. |
| `OnSelectionChanged` | Called when the selection changes. |

Use `MSelectOption` inside the component to add special values that are not in `Options`.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L217)
