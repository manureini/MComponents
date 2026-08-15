# MComponents Wiki

MComponents is a Blazor component library for data entry, navigation, feedback, and rich application workflows.

## Getting started

1. Install the `MComponents` NuGet package.
2. Register the services with `services.AddMComponents(...)`.
3. Add the bundled CSS and JavaScript references described in the [repository README](https://github.com/manureini/MComponents#how-to-use).
4. Place `<MComponentsRoot />` in the application layout so tooltips and toasts can render.

## Components

| Data and input | Layout and navigation | Feedback and utilities |
| --- | --- | --- |
| [MForm](MForm) | [MAccordion](MAccordion) | [MBadge](MBadge) |
| [MGrid](MGrid) | [MCards](MCards) | [MPaint](MPaint) |
| [MInputFile](MInputFile) | [MPopup](MPopup) | [MProgressbar](MProgressbar) |
| [MQueryBuilder](MQueryBuilder) | [MScrollAnchor](MScrollAnchor) | [MSpinner](MSpinner) |
| [MSelect](MSelect) | [MTabs](MTabs) | [MToaster](MToaster) |
|  | [MWizard](MWizard) | [MTooltip](MTooltip) |
|  | [MSeparator](MSeparator) |  |

The [ExampleApp component gallery](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor) contains runnable simple and advanced scenarios for every component.
