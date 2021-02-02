using Application.Common;
using Application.Services.Interfaces;
using Domain.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace WebScraping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WebScrapingController : ControllerBase
    {
        private readonly IAppScraper scraper;

        public WebScrapingController(IAppScraper scraper)
        {
            this.scraper = scraper;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> RunAsync([FromQuery] string url)
        {
            var request = new ScraperRequest(url);
            var response = await this.scraper.GetGroupingFileInformationAsync(request);
            return HandleResult(response);
        }

        protected virtual IActionResult HandleResult<T>(ServiceTResult<T> serviceResult)
        {
            if (serviceResult.Status == ServiceResult.StatusResult.Error)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
         
            return new JsonResult(serviceResult);
        }
    }
}
