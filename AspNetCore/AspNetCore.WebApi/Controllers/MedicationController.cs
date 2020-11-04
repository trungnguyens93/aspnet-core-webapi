using System.Threading.Tasks;
using AspNetCore.WebApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.WebApi.Controllers
{
    [Route("api/medication")]
    [Authorize]
    [ApiController]
    public class MedicationController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new BaseResponse<object>
            {
                Code = StatusCodes.Status200OK,
                Message = "Hello world!!!"
            });
        }
    }
}