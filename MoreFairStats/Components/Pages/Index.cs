﻿
namespace MoreFairStats.Components.Pages
{
    [Microsoft.AspNetCore.Components.Route("/")]
    public partial class Index : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.AddMarkupContent(0, @"<iframe title=""MFSPowerBI"" width=""1024"" height=""1060"" src=""https://app.powerbi.com/view?r=eyJrIjoiZWJjZTI0NTUtMjFmMy00ZTdiLTkzMmEtYWRhNjczZWE4MDQzIiwidCI6ImZjMWZmYTJhLTU1MGYtNDFmNy04NDE5LWVjM2Q4NGIyMDM0ZiJ9&pageName=8a7cd5b7dd98f376173d"" frameborder=""0"" allowFullScreen=""true""></iframe>");
        }
    }
}
