using Application.Common;
using Application.Services.Interfaces;
using Domain.Services.Interfaces;
using Domain.Shared.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class GitHubScraperAppService : ScraperAppService, IAppScraper
    {

        public GitHubScraperAppService(IScraper domainService) : base(domainService)
        {
        }

        public override async Task<ServiceTResult<List<GroupingFileInformationResponse>>> GetGroupingFileInformationAsync(ScraperRequest request)
        {
            this.FastValidation(request);

            if (Invalid) return this.HandleResult<List<GroupingFileInformationResponse>>(null);

            var data = await this.domainService.GetGroupingFileInformationAsync(request);

            return this.HandleResult(data);
        }
    }
}
