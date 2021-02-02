using Domain.Shared.Dto;
using Flunt.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IScraper
    {
        Task<ScraperDataResponse> GetAsync(ScraperRequest request);
        Task<List<GroupingFileInformationResponse>> GetGroupingFileInformationAsync(ScraperRequest request);
        Notifiable GetNotifiable();
    }
}