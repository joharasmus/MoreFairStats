
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace MoreFairStats.Components;

public class Routes : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<Router>(0);
        __builder.AddComponentParameter(1, "AppAssembly", RuntimeHelpers.TypeCheck(typeof(Program).Assembly));
        __builder.AddAttribute(2, "Found", (RenderFragment<RouteData>)((routeData) => (__builder2) => {
            __builder2.OpenComponent<RouteView>(3);
            __builder2.AddComponentParameter(4, "RouteData", RuntimeHelpers.TypeCheck(routeData));
            __builder2.AddComponentParameter(5, "DefaultLayout", RuntimeHelpers.TypeCheck(typeof(MainLayout)));
            __builder2.CloseComponent();
            __builder2.AddMarkupContent(6, "\r\n        ");
            __builder2.OpenComponent<FocusOnNavigate>(7);
            __builder2.AddComponentParameter(8, "RouteData", RuntimeHelpers.TypeCheck(routeData));
            __builder2.AddComponentParameter(9, "Selector", "h1");
            __builder2.CloseComponent();
        }
        ));
        __builder.CloseComponent();
    }
}
