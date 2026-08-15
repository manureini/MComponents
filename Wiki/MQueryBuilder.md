# MQueryBuilder

`MQueryBuilder<T>` lets users create nested filter groups and conditions, then applies those rules to an `IQueryable<T>`.

## Simple

```razor
<MQueryBuilder T="Product" @bind-RuleGroup="rules">
    <MQueryBuilderField RuleName="@nameof(Product.Name)" />
</MQueryBuilder>

@code {
    private MQueryBuilderRuleGroup rules = new();
}
```

## Advanced

```razor
<MQueryBuilder T="Product"
               @bind-RuleGroup="rules"
               @bind-RuleGroup:after="RefreshResults">
    <MQueryBuilderField RuleName="@nameof(Product.Name)" Title="Product" />
    <MQueryBuilderField RuleName="@nameof(Product.Category)" Title="Category" />
    <MQueryBuilderField RuleName="@nameof(Product.Price)" Title="Price" />
</MQueryBuilder>

@code {
    private MQueryBuilderRuleGroup rules = new();
    private Product[] results = Array.Empty<Product>();

    private void RefreshResults()
    {
        results = MQueryBuilderHelper
            .ApplyRules<Product>(
                products.AsQueryable(),
                rules,
                (ruleName, arguments) => null)
            .ToArray();
    }
}
```

## Members

| Member | Description |
| --- | --- |
| `RuleGroup` / `RuleGroupChanged` | Bound root rule group. |
| `ChildContent` | `MQueryBuilderField` and `MQueryBuilderComplexField` definitions. |
| `MQueryBuilderField.RuleName` | Model property represented by the field. |
| `MQueryBuilderField.Title` | User-facing field name. |
| `AllowedOperators` | Restricts the operators available for a field. |
| `ToJson()` | Serializes the current rule group. |

The expression callback can map custom rule names to expressions; return `null` to use the model property named by the rule. An optional value modifier can normalize condition values before comparison.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L191) · [View the filtering example](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/QueryBuilder.razor)
