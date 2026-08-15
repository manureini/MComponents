# MCards

`MCards<T>` renders a collection as a responsive CSS grid using a typed template.

## Simple

```razor
<MCards T="string" Values="new[] { "First", "Second", "Third" }">
    <Template Context="name">
        <strong>@name</strong>
    </Template>
</MCards>
```

## Advanced

```razor
<MCards T="Product" Values="products" Columns="3" OnClick="SelectProduct">
    <Template Context="product">
        <h3>@product.Name</h3>
        <p>@product.Price.ToString("C")</p>
    </Template>
    <TemplateEmpty>
        <em>No products are available.</em>
    </TemplateEmpty>
</MCards>

@code {
    private IEnumerable<Product> products = Array.Empty<Product>();
    private Product selected;

    private void SelectProduct(Product product) => selected = product;
}
```

## Parameters

| Parameter | Description |
| --- | --- |
| `Values` | Items to render. |
| `Columns` | Number of equal-width grid columns; defaults to one. |
| `Template` | Typed content rendered for each item. |
| `TemplateEmpty` | Content rendered when `Values` is empty. |
| `OnClick` | Callback receiving the selected item. |
| `CssClass` | Additional class for the cards container. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L53)
