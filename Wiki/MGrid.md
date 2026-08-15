# MGrid

`MGrid<T>` displays, edits, filters, groups, imports, and exports tabular data.

## Data sources

Use `DataSource` for an in-memory or queryable sequence. Use `DataAdapter` when fetching, counting, updating, and deleting rows requires asynchronous operations.

## Simple

```razor
<MGrid DataSource="products" HtmlTableClass="m-grid m-grid-striped">
    <MGridColumns>
        <MGridColumn Property="@nameof(Product.Name)" />
        <MGridColumn Property="@nameof(Product.Price)" StringFormat="{0:C}" />
    </MGridColumns>
</MGrid>
```

## Advanced

```razor
<MGrid DataSource="products"
       EnableAdding="true"
       EnableEditing="true"
       EnableDeleting="true"
       EnableFilterRow="true"
       EnableGrouping="true"
       EnableUserSorting="true"
       EnableExport="true"
       EnableSaveState="true"
       ToolbarItems="ToolbarItem.Add | ToolbarItem.Edit | ToolbarItem.Delete">
    <MGridColumns>
        <MGridColumn Property="@nameof(Product.Name)" HeaderText="Product" />
        <MGridColumn Property="@nameof(Product.Price)" StringFormat="{0:C}" />
        <MGridActionColumn T="Product" />
    </MGridColumns>
    <MGridPager PageSize="20" SelectablePageSizes="new[] { 10, 20, 50 }" />
    <MGridEvents T="Product" OnAfterEdit="ProductEdited" />
</MGrid>
```

## Important parameters

| Parameter | Description |
| --- | --- |
| `Identifier` | Unique grid key, used for persisted state. |
| `DataSource` / `DataAdapter` | Synchronous or asynchronous data provider. |
| `ModelFactory` | Creates a row when a type cannot be constructed automatically. |
| `EnableAdding`, `EnableEditing`, `EnableDeleting` | Enables row operations. |
| `EnableUserSorting`, `EnableFilterRow`, `EnableGrouping` | Enables data exploration features. |
| `EnableExport`, `EnableImport` | Enables spreadsheet transfer. |
| `EnableSaveState` | Persists page, sorting, filters, and selection in local storage. |
| `ToolbarItems` | Selects toolbar actions. |
| `HtmlTableClass` | Classes applied to the generated table. |

For related-object editing, use `MGridComplexPropertyColumn`; for custom output, use `MGridComplexColumn`.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L101) · [View the data-adapter example](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/FetchDataDataAdapter.razor)
