# MComponents

[![Package Version](https://img.shields.io/nuget/v/MComponents.svg)](https://www.nuget.org/packages/MComponents)
[![Package Version](https://img.shields.io/nuget/v/MComponents.Shared.svg)](https://www.nuget.org/packages/MComponents.Shared)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MComponents.svg)](https://www.nuget.org/packages/MComponents)


This is another Blazor component libary which supports

* Grids
* Wizards
* Forms
* Paint

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


### Please create an issue or make Pull requests if you want to support this project

The documentation is pretty limited, because I'm lazy:
https://github.com/manureini/MComponents/wiki



