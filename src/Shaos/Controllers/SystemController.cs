using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shaos.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(ApiContractVersions.VersionOne)]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        [HttpGet]
        public string GetVersion()
        {
            return "1.0.0";
        }
    }
}
