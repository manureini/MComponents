# MInputFile

`MInputFile` uploads one or more files through the registered `IFileUploadService`. `MInputFileSimple` provides a compact single-file variant.

## Setup

Register an application implementation:

```csharp
services.AddScoped<IFileUploadService, FileUploadService>();
```

Both controls are Blazor input components and must receive `@bind-Value` or `@bind-Values`.

## Simple

```razor
<MInputFileSimple @bind-Value="document" Accept=".pdf,image/*" />

@code {
    private IFile document;
}
```

## Advanced

```razor
<MInputFile MaxAllowedFiles="5"
            @bind-Values="documents"
            Accept=".pdf,image/*"
            FileInputName="attachments"
            AdditionalHeaders="uploadHeaders"
            OnFileClicked="OpenFile" />

@code {
    private ICollection<IFile> documents = new List<IFile>();
    private IDictionary<string, string> uploadHeaders =
        new Dictionary<string, string> { ["Category"] = "Invoices" };
}
```

## Parameters

| Parameter | Description |
| --- | --- |
| `MaxAllowedFiles` | Maximum number of files; defaults to one. |
| `Value` / `Values` | Single or multiple bound file values. |
| `Accept` | Browser file-type filter. |
| `FileInputName` | Logical input name sent to the upload service. |
| `AdditionalHeaders` | Application metadata passed to the upload service. |
| `Attributes` | Validation or display attributes associated with the field. |
| `OnFileClicked` | Called when an uploaded file is selected. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L137) · [View the upload service example](https://github.com/manureini/MComponents/tree/master/MComponents.ExampleApp/Service)
