using Application.Common;
using Domain.Shared.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IAppScraper
    {
        Task<ServiceTResult<ScraperDataResponse>> GetAsync(ScraperRequest request);
        Task<ServiceTResult<List<GroupingFileInformationResponse>>> GetGroupingFileInformationAsync(ScraperRequest request);
    }
}