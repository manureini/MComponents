# MProgressbar

`MProgressbar` visualizes a percentage. Values below zero are rendered as zero and values above 100 as 100.

## Simple

```razor
<MProgressbar Percentage="35" />
```

## Advanced

```razor
<input type="range"
       min="0"
       max="100"
       @bind="progress"
       @bind:event="oninput" />

<MProgressbar Percentage="progress" CssClass="upload-progress" />
<span>@progress%</span>

@code {
    private int progress = 65;
}
```

## Parameters

| Parameter | Description |
| --- | --- |
| `Percentage` | Progress from 0 through 100. |
| `CssClass` | Application-specific progress styling. |

[View the runnable examples](https://github.com/manureini/MComponents/blob/master/MComponents.ExampleApp/Pages/Components.razor#L181)
