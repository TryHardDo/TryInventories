using Microsoft.AspNetCore.Mvc;

namespace TryInventories.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryApiController : Controller
    {
        [HttpGet("{steamId}")]
        public ActionResult<object> GetInventory(string steamId)
        {
            return Ok();
        }
    }
}
