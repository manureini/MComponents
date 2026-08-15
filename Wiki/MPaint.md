# MPaint

`MPaint` is a mouse- and touch-enabled canvas for signatures and freehand drawing.

Add the canvas dependency script in the host page:

```html
<script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
```

## Simple

```razor
<MPaint Width="500" Height="200" />
```

## Advanced

Capture the component reference to inspect or export the drawing:

```razor
<MPaint @ref="paint"
        Width="640"
        Height="240"
        PenColor="#0b7285"
        CssClass="signature-pad" />

<button type="button" @onclick="Save">Save signature</button>

@code {
    private MPaint paint;

    private async Task Save()
    {
        if (!paint.ImageIsEmpty)
        {
            byte[] png = await paint.GetImage();
            // Persist png in the application.
        }
    }
}
```

## Parameters and methods

| Member | Description |
| --- | --- |
| `Width`, `Height` | Canvas dimensions in pixels. |
| `PenColor` | CSS drawing color. |
| `CssClass` | Additional canvas container class. |
| `ImageIsEmpty` | Indicates whether enough points have been drawn. |
| `GetImage()` | Returns the canvas as PNG bytes. |
| `OnResetClick()` | Clears the drawing. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L153)
