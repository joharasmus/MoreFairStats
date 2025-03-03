
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace MoreFairStats.Components;

public class App : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.AddMarkupContent(0, "<!DOCTYPE html>\r\n");
        __builder.OpenElement(1, "html");
        __builder.AddAttribute(2, "lang", "en");
        __builder.OpenElement(3, "head");
        __builder.AddMarkupContent(4, @"<meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <base href=""/"">
    <link rel=""icon"" type=""image/png"" href=""favicon.png"">
    ");
        __builder.CloseElement();
        __builder.AddMarkupContent(5, "\r\n\r\n");
        __builder.OpenElement(6, "body");
        __builder.OpenComponent<Routes>(7);
        __builder.CloseComponent();
        __builder.CloseElement();
        __builder.CloseElement();
    }
}
