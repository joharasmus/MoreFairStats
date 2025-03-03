
namespace MoreFairStats.Components.Layout
{
    using Microsoft.AspNetCore.Components;
    public partial class MainLayout : LayoutComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, "main");
            __builder.AddAttribute(1, "style", "background-color: #10141f; text-align:center");
            __builder.AddContent(2, Body);
            __builder.CloseElement();
        }
    }
}
