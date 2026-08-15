# MAccordion

`MAccordion` organizes content into expandable `MAccordionCard` sections.

## Simple

```razor
<MAccordion>
    <Cards>
        <MAccordionCard Identifier="details" Title="Details" InitialOpen="true">
            Details are visible initially.
        </MAccordionCard>
        <MAccordionCard Identifier="history" Title="History">
            History is shown on demand.
        </MAccordionCard>
    </Cards>
</MAccordion>
```

## Advanced

Allow several sections to remain open and keep hidden card bodies rendered when they contain stateful controls:

```razor
<MAccordion AllowMultipleOpenCards="true" RenderHiddenCards="true">
    <Cards>
        <MAccordionCard Identifier="profile" Title="<strong>Profile</strong>" InitialOpen="true">
            <MForm Model="profile" />
        </MAccordionCard>
        <MAccordionCard Identifier="preferences" Title="Preferences">
            Preference controls
        </MAccordionCard>
    </Cards>
</MAccordion>
```

## Parameters

| Parameter | Description |
| --- | --- |
| `Cards` | The collection of `MAccordionCard` declarations. |
| `AllowMultipleOpenCards` | Allows more than one card to remain expanded. |
| `IsReadOnly` | Displays content without allowing cards to be toggled. |
| `RenderHiddenCards` | Keeps collapsed card content in the render tree. |

Every card needs a unique `Identifier`. Its `Title` accepts markup, while its child content contains the card body.

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L20)
