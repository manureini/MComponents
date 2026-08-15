# MComponents

[![Package Version](https://img.shields.io/nuget/v/MComponents.svg)](https://www.nuget.org/packages/MComponents)
[![Package Version](https://img.shields.io/nuget/v/MComponents.Shared.svg)](https://www.nuget.org/packages/MComponents.Shared)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MComponents.svg)](https://www.nuget.org/packages/MComponents)


This is a Blazor component library with data-entry, navigation, feedback, and workflow controls.

### Components

| Data and input | Layout and navigation | Feedback and utilities |
| --- | --- | --- |
| [MForm](https://github.com/manureini/MComponents/wiki/MForm) | [MAccordion](https://github.com/manureini/MComponents/wiki/MAccordion) | [MBadge](https://github.com/manureini/MComponents/wiki/MBadge) |
| [MGrid](https://github.com/manureini/MComponents/wiki/MGrid) | [MCards](https://github.com/manureini/MComponents/wiki/MCards) | [MPaint](https://github.com/manureini/MComponents/wiki/MPaint) |
| [MInputFile](https://github.com/manureini/MComponents/wiki/MInputFile) | [MPopup](https://github.com/manureini/MComponents/wiki/MPopup) | [MProgressbar](https://github.com/manureini/MComponents/wiki/MProgressbar) |
| [MQueryBuilder](https://github.com/manureini/MComponents/wiki/MQueryBuilder) | [MScrollAnchor](https://github.com/manureini/MComponents/wiki/MScrollAnchor) | [MSpinner](https://github.com/manureini/MComponents/wiki/MSpinner) |
| [MSelect](https://github.com/manureini/MComponents/wiki/MSelect) | [MSeparator](https://github.com/manureini/MComponents/wiki/MSeparator) | [MToaster](https://github.com/manureini/MComponents/wiki/MToaster) |
|  | [MTabs](https://github.com/manureini/MComponents/wiki/MTabs) | [MTooltip](https://github.com/manureini/MComponents/wiki/MTooltip) |
|  | [MWizard](https://github.com/manureini/MComponents/wiki/MWizard) |  |

Each Wiki page includes simple and advanced examples. The same scenarios are runnable in the
[ExampleApp component gallery](MComponents.ExampleApp/Pages/Components.razor).

### Screenshots

![mgrid](https://raw.githubusercontent.com/manureini/MComponents/master/Screenshots/MGrid.PNG)
![mselect](https://raw.githubusercontent.com/manureini/MComponents/master/Screenshots/MSelect.png)
![mwizard](https://raw.githubusercontent.com/manureini/MComponents/master/Screenshots/MWizard.PNG)

### How to use?

Add the following references to your _Host.cshtml

```html
<link href="_content/MComponents/css/fontawesome.css" rel="stylesheet" />
<link href="_content/MComponents/css/mcomponents.css" rel="stylesheet" />
<script src="_content/MComponents/js/mcomponents.js"></script>
```
If you want to use MPaint add
```html
<script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
```

Add to Startup.cs:
```c#
services.AddMComponents(options =>
{
    options.RegisterResourceLocalizer = true;
    options.RegisterStringLocalizer = true;
});
```
and if you want to use RequestLocalization
```c#
app.UseRequestLocalization();
```
Add to App.razor or MainLayout.razor
```html
<MComponentsRoot />
```

### Documentation and contributions

Read the complete [GitHub Wiki](https://github.com/manureini/MComponents/wiki). Its Markdown sources are versioned in the [`Wiki`](Wiki) directory.

Please create an issue or pull request if you want to support this project.


