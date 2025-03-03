
namespace MoreFairStats.Components
{
    public partial class Routes : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<Microsoft.AspNetCore.Components.Routing.Router>(0);
            __builder.AddComponentParameter(1, "AppAssembly", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(typeof(Program).Assembly));
            __builder.AddAttribute(2, "Found", (Microsoft.AspNetCore.Components.RenderFragment<Microsoft.AspNetCore.Components.RouteData>)((routeData) => (__builder2) => {
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.RouteView>(3);
                __builder2.AddComponentParameter(4, "RouteData", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(routeData));
                __builder2.AddComponentParameter(5, "DefaultLayout", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(typeof(Layout.MainLayout)));
                __builder2.CloseComponent();
                __builder2.AddMarkupContent(6, "\r\n        ");
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Routing.FocusOnNavigate>(7);
                __builder2.AddComponentParameter(8, "RouteData", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(routeData));
                __builder2.AddComponentParameter(9, "Selector", "h1");
                __builder2.CloseComponent();
            }
            ));
            __builder.CloseComponent();
        }
    }
}
