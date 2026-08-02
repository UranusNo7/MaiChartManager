using MaiChartManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaiChartManager.Controllers.Tools;

[ApiController]
[Route("MaiChartManagerServlet/[action]Api")]
public class ResourceJunctionController(ResourceJunctionService service) : ControllerBase
{
    private const string LocalActionHeader = "X-MCM-Local-Action";
    private const string LocalActionValue = "resource-junction";

    [HttpGet]
    public ActionResult<IReadOnlyList<ResourceJunctionItem>> GetResourceJunctionStatus()
    {
        if (StaticSettings.Config.Export) return Forbid();
        return Ok(service.Inspect());
    }

    [HttpPost]
    public ActionResult<IReadOnlyList<ResourceJunctionItem>> CreateResourceJunctions()
    {
        if (StaticSettings.Config.Export) return Forbid();
        if (Request.Headers[LocalActionHeader] != LocalActionValue) return BadRequest();
        return Ok(service.CreateLinks());
    }

    [HttpPost]
    public ActionResult<IReadOnlyList<ResourceJunctionItem>> RemoveResourceJunctions()
    {
        if (StaticSettings.Config.Export) return Forbid();
        if (Request.Headers[LocalActionHeader] != LocalActionValue) return BadRequest();
        return Ok(service.RemoveLinks());
    }
}
