
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MoreFairStats.Components;

public class MainLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenElement(0, "main");
        __builder.AddAttribute(1, "style", "background-color: #10141f; text-align:center");
        __builder.AddContent(2, Body);
        __builder.CloseElement();
    }
}
